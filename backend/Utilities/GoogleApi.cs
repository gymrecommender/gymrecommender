using System.Text.Json;
using backend.Views;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using backend.Models;
using backend.ViewModels.WorkingHour;

namespace backend.Utilities;

using DotNetEnv;

public class GoogleApi {
    private string _apiKey;
    private readonly HttpClient _client;

    public GoogleApi(HttpClient client) {
        _client = client;

        Env.Load();
        _apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
    }

    private async Task<Response> GetRequest(string url) {
        HttpResponseMessage response = await _client.GetAsync(url);

        if (response.IsSuccessStatusCode) {
            var res = new Dictionary<string, object>();
            string json = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(json).RootElement;

            //Trying to retrieve the fields of the Json response that we are receiving from the Google API
            if (jsonDoc.TryGetProperty("next_page_token", out var token)) res["nextPageToken"] = token.GetString();
            if (jsonDoc.TryGetProperty("results", out var results)) res["results"] = results.EnumerateArray();
            if (jsonDoc.TryGetProperty("result", out var result)) res["result"] = result;

            return new Response(res);
        }

        return new Response(
            response.ReasonPhrase ?? "Unknown external error",
            response.StatusCode.ToString(),
            true
        );
    }

    public async Task<Response> GetCity(double latitude, double longitude) {
        //Retrieve the city based on the current location of the user (or any other coordinates provided)
        var urlCoord = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={_apiKey}";
        var response = await GetRequest(urlCoord);
        if (!response.Success) return response;

        //We have to explicitly convert the types as the value in the Response is a generic (can be of any type)
        var result = ((JsonElement.ArrayEnumerator)((Dictionary<string, object>)response.Value)["results"]).First();
        var addressComponents = result.GetProperty("address_components");
        var geocode = new Geocode();
        foreach (var component in addressComponents.EnumerateArray()) {
            var types = component.GetProperty("types").EnumerateArray()
                                 .Select(type => type.GetString());

            if (geocode.City == null) {
                if (types.Any(type => type == "administrative_area_level_1")) {
                    geocode.City = component.GetProperty("long_name").GetString();
                } else if (types.Any(type => type == "administrative_area_level_2")) {
                    geocode.City = component.GetProperty("long_name").GetString();
                } else if (types.Any(type => type == "locality")) {
                    geocode.City = component.GetProperty("long_name").GetString();
                }
            } else if (types.Any(type => type == "country")) {
                geocode.Country = component.GetProperty("long_name").GetString();
            }
        }

        //We need the viewport of the whole city, not just some area around the requested coordinates
        var urlAddress =
            $"https://maps.googleapis.com/maps/api/geocode/json?address={geocode.City},{geocode.Country}&key={_apiKey}";
        response = await GetRequest(urlAddress);
        if (!response.Success) return response;

        //We may have multiple cities with the same name in the same country, so we need to additionally check the coordinates
        foreach (var city in ((JsonElement.ArrayEnumerator)((Dictionary<string, object>)response.Value)["results"])) {
            var viewport = city.GetProperty("geometry").GetProperty("viewport");
            var northeast = viewport.GetProperty("northeast");
            double nelat = northeast.GetProperty("lat").GetDouble();
            double nelng = northeast.GetProperty("lng").GetDouble();

            var southwest = viewport.GetProperty("southwest");
            double swlat = southwest.GetProperty("lat").GetDouble();
            double swlng = southwest.GetProperty("lng").GetDouble();

            //We check whether the requested location actually lies within the viewport of the city
            if ((swlat <= latitude && latitude <= nelat) &&
                (swlng <= longitude && longitude <= nelng)) {
                geocode.Nelatitude = nelat;
                geocode.Nelongitude = nelng;
                geocode.Swlongitude = swlng;
                geocode.Swlatitude = swlat;
                break;
            }
        }

        return new Response(geocode);
    }

    public async Task<Response> GetGyms(double latitude, double longitude, int rad, City city) {
        var placesUrl =
            $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={latitude},{longitude}&radius={rad}&type=gym&keyword=gym&key={_apiKey}";
        var placesDetailUrl =
            $"https://maps.googleapis.com/maps/api/place/details/json?fields=user_ratings_total,utc_offset,website,wheelchair_accessible_entrance,rating,name,opening_hours,formatted_address,formatted_phone_number,geometry&key={_apiKey}";

        string? token = null;
        var returnResult = new List<Tuple<Gym, List<GymWorkingHoursViewModel>>>();
        do {
            //We retrieve the list of the gyms for the current location
            var response = await GetRequest($"{placesUrl}{(string.IsNullOrEmpty(token) ? "" : $"&pagetoken={token}")}");
            if (!response.Success) return response;


            var responseDict = (Dictionary<string, object>)response.Value;
            //Google API returns up to 20 elements in its response and up to 60 elements altogether
            //In order to access the next page, a token for the next page is sent, and it needs to be added as a get parameter in the subsequent request
            //The token is also an indicator of the absence of more gyms related to the specified parameters
            token = responseDict.ContainsKey("nextPageToken") ? responseDict["nextPageToken"].ToString() : null;
            var gyms = (JsonElement.ArrayEnumerator)responseDict["results"];

            foreach (var oneGym in gyms) {
                Gym gym = new Gym();
                gym.ExternalPlaceId = oneGym.GetProperty("place_id").GetString();

                //For each gym we must request detailed information in order to have some important fields stored
                var detResponse = await GetRequest($"{placesDetailUrl}&place_id={gym.ExternalPlaceId}");
                if (!detResponse.Success) return detResponse;
                var placeDetail = (JsonElement)((Dictionary<string, object>)detResponse.Value)["result"];

                gym.Address = placeDetail.GetProperty("formatted_address").GetString();
                if (placeDetail.TryGetProperty("formatted_phone_number", out var phone)) {
                    gym.PhoneNumber = phone.GetString();
                }

                if (placeDetail.TryGetProperty("website", out var website)) {
                    gym.Website = website.GetString();
                }

                gym.IsWheelchairAccessible = placeDetail.TryGetProperty("wheelchair_accessible_entrance", out _);

                //We will need to also store working hours that are related to the gym
                //Since they are stored in a separate table, it makes sense to have a separate list of working hours
                var workingHours = new List<GymWorkingHoursViewModel>();
                if (placeDetail.TryGetProperty("opening_hours", out var openingHours)) {
                    var openHoursList = openingHours.GetProperty("periods")
                                                    .EnumerateArray();
                    // In this case we have regular opening and closing hours, no 24 hours options
                    if (openHoursList.Count() > 1 && !openHoursList.First().TryGetProperty("close", out _)) {
                        foreach (var oneDay in openHoursList) {
                            workingHours.Add(new GymWorkingHoursViewModel {
                                Weekday = Convert.ToInt32(oneDay.GetProperty("close").GetProperty("day")),
                                OpenUntil = TimeOnly.Parse(oneDay.GetProperty("close").GetProperty("time").GetString()
                                                                 .Insert(2, ":")),
                                OpenFrom = TimeOnly.Parse(oneDay.GetProperty("open").GetProperty("time").GetString()
                                                                .Insert(2, ":"))
                            });
                        }
                    } else {
                        //if there is only one element in the opening hours and there is no close attribute, it means that the gym works 24/7
                        for (int i = 0; i <= 6; i++) {
                            workingHours.Add(new GymWorkingHoursViewModel {
                                Weekday = i,
                                OpenUntil = TimeOnly.Parse("23:59:59"),
                                OpenFrom = TimeOnly.Parse("00:00:00"),
                            });
                        }
                    }
                }

                if (placeDetail.TryGetProperty("rating", out var rating)) {
                    gym.ExternalRating = oneGym.GetProperty("rating").GetDecimal();
                } else {
                    gym.ExternalRating = 0;
                }

                if (placeDetail.TryGetProperty("user_ratings_total", out var ratingsTotal)) {
                    gym.ExternalRatingNumber = oneGym.GetProperty("user_ratings_total").GetInt32();
                } else {
                    gym.ExternalRatingNumber = 0;
                }

                gym.Name = oneGym.GetProperty("name").GetString();

                var location = oneGym.GetProperty("geometry").GetProperty("location");
                gym.Latitude = location.GetProperty("lat").GetDouble();
                gym.Longitude = location.GetProperty("lng").GetDouble();

                gym.CityId = city.Id;
                returnResult.Add(new Tuple<Gym, List<GymWorkingHoursViewModel>>(gym, workingHours));
            }
        } while (token != null);

        return new Response(returnResult);
    }
}
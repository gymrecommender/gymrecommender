using System.Text.Json;
using backend.Views;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using backend.Models;

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

            res["results"] = jsonDoc.GetProperty("results").EnumerateArray();

            jsonDoc.GetProperty("results").EnumerateArray();
            if (jsonDoc.TryGetProperty("next_page_token", out var token)) {
                res["nextPageToken"] = token.GetString();
            }

            return new Response(res);
        }

        return new Response(
            response.ReasonPhrase ?? "Unknown external error",
            response.StatusCode.ToString(),
            true
        );
    }

    public async Task<Response> GetCity(double latitude, double longitude) {
        var urlCoord = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={_apiKey}";
        var response = await GetRequest(urlCoord);
        if (!response.Success) return response;

        var result = ((JsonElement.ArrayEnumerator)((Dictionary<string, object>)response.Value)["results"]).First();
        var addressComponents = result.GetProperty("address_components");
        var geocode = new Geocode();
        foreach (var component in addressComponents.EnumerateArray()) {
            var types = component.GetProperty("types").EnumerateArray()
                                 .Select(type => type.GetString());

            if (types.Any(type => type == "locality")) {
                geocode.City = component.GetProperty("long_name").GetString();
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
            
            //we check whether the requested location actually lies within the viewport of the city
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

    // public async Task<List<Gym>?> GetGyms(double latitude, double longitude, int rad = 10000) {
    //     var placesUrl =
    //         $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={latitude},{longitude}&radius={rad}&type=gym&keyword=gym&key={_apiKey}";
    //     var placesDetailUrl =
    //         $"https://maps.googleapis.com/maps/api/place/details/json?fields=user_ratings_total,utc_offset,website,wheelchair_accessible_entrance,rating,name,opening_hours,formatted_address,formatted_phone_number,geometry&key={_apiKey}";
    //
    //     HttpResponseMessage response = await _client.GetAsync(placesUrl);
    //
    //     if (response.IsSuccessStatusCode) {
    //         string json = await response.Content.ReadAsStringAsync();
    //         var jsonDoc = JsonDocument.Parse(json);
    //         var result = jsonDoc.RootElement.GetProperty("results").EnumerateArray();
    //
    //         var returnResult = new List<Gym>();
    //         foreach (var oneGym in result) {
    //             Gym gym = new Gym();
    //             gym.ExternalPlaceId = oneGym.GetProperty("place_id").GetString();
    //
    //             HttpResponseMessage responseDet =
    //                 await _client.GetAsync($"{placesDetailUrl}&place_id={gym.ExternalPlaceId}");
    //             gym.Name = oneGym.GetProperty("name").GetString();
    //             gym.ExternalRating = oneGym.GetProperty("rating").GetDecimal();
    //             gym.ExternalRatingNumber = oneGym.GetProperty("user_ratings_total").GetInt32();
    //
    //             var location = oneGym.GetProperty("geometry").GetProperty("location");
    //             gym.Latitude = location.GetProperty("lat").GetDouble();
    //             gym.Longitude = location.GetProperty("lng").GetDouble();
    //
    //             returnResult.Add(gym);
    //         }
    //
    //         return returnResult;
    //     } else {
    //         return null;
    //     }
    // }
}
using System.Text.Json;
using backend.DTO;
using backend.Models;
using backend.Utilities;

namespace backend.Services;

public class GeoService
{
    private readonly GoogleApiService _googleApiService;

    // Inject the GoogleApiService (or IGeoClient if you have an interface)
    public GeoService(GoogleApiService googleApiService)
    {
        _googleApiService = googleApiService;
    }

    public GymFilteredTravelInfoDto CalculateTravelingTimeAndPrice(
        GymsFilteredDto gyms,
        double userLatitude,
        double userLongitude)
    {
        List<GymTravelInfoDto> mainGyms = CalculateTravelingTimeAndPrice(gyms.MainGyms, userLatitude, userLongitude);
        List<GymTravelInfoDto> auxGyms = CalculateTravelingTimeAndPrice(gyms.AuxGyms, userLatitude, userLongitude);
        return new GymFilteredTravelInfoDto
        {
            MainGyms = mainGyms,
            AuxGyms = auxGyms,
        };
    }

    private List<GymTravelInfoDto> CalculateTravelingTimeAndPrice(
        List<Gym> gyms,
        double userLatitude,
        double userLongitude)
    {
        // Call our GoogleApiService to get time/distance info.
        // Because GetDistanceMatrixAsync is async, we need to wait for it here
        // (if we cannot change this method signature to async).
        var distanceMatrixResponse =
            _googleApiService.GetDistanceMatrixAsync(userLatitude, userLongitude, gyms)
                .GetAwaiter()
                .GetResult();

        if (!distanceMatrixResponse.Success)
        {
            // Handle error as needed; throw, log, or return an empty list.
            throw new Exception($"Google Distance Matrix API error: {distanceMatrixResponse.Value}");
        }

        // The returned value is the JsonElement root we received from Google
        var root = (JsonElement)distanceMatrixResponse.Value;

        //  Distance Matrix returns:
        // {
        //   "destination_addresses" : [ ... ],
        //   "origin_addresses" : [ ... ],
        //   "rows" : [
        //     {
        //       "elements": [
        //         {
        //           "distance": { "text": "...", "value": X },
        //           "duration": { "text": "...", "value": Y },
        //           "status": "OK"
        //         },
        //         ...
        //       ]
        //     }
        //   ],
        //   "status" : "OK"
        // }

        var result = new List<GymTravelInfoDto>();

        // Safely parse the array of rows (should be only 1 row if there's 1 origin).
        if (root.TryGetProperty("rows", out var rows) && rows.GetArrayLength() > 0)
        {
            var elements = rows[0].GetProperty("elements");

            // 'elements' should have the same number of items as the number of destinations (gyms)
            for (int i = 0; i < gyms.Count; i++)
            {
                var gym = gyms[i];
                var element = elements[i];

                // Check if the element has a valid distance/duration
                var statusProperty = element.GetProperty("status").GetString();
                if (statusProperty != "OK")
                {
                    // TODO: If the status is not OK, we can decide how to handle it
                    result.Add(new GymTravelInfoDto
                    {
                        Gym = gym,
                        TravelTime = 0,
                        // TravelPrice = 0
                    });
                    continue;
                }

                // Distance is typically in meters
                double distanceInMeters = element
                    .GetProperty("distance")
                    .GetProperty("value")
                    .GetDouble();

                // Duration is typically in seconds
                double timeInSeconds = element
                    .GetProperty("duration")
                    .GetProperty("value")
                    .GetDouble();

                // Convert
                double distanceInKm = distanceInMeters / 1000.0;
                double travelTimeInMinutes = timeInSeconds / 60.0;

                //// TODO: travelPrice depends on the way of transportation and is not available for cars, skip for now
                double travelPrice = 0;

                result.Add(new GymTravelInfoDto
                {
                    Gym = gym,
                    TravelTime = travelTimeInMinutes,
                });
            }
        }

        return result;
    }
}
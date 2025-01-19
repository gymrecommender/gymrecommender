using System.Text.Json;
using backend.DTO;
using backend.Models;
using backend.Utilities;

namespace backend.Services;

public class GeoService
{
    private readonly GoogleApiService _googleApiService;

    // Constructor injection for GoogleApiService
    public GeoService(GoogleApiService googleApiService)
    {
        _googleApiService = googleApiService;
    }

    /// <summary>
    /// Calculates traveling time and price for both main and auxiliary gyms.
    /// If departureTime is null or invalid, it defaults to 5 PM UTC today or next day if past 5 PM.
    /// </summary>
    /// <param name="gyms">GymsFilteredDto containing main and auxiliary gyms.</param>
    /// <param name="userLatitude">User's latitude.</param>
    /// <param name="userLongitude">User's longitude.</param>
    /// <param name="departureTime">Desired departure time as a string. If null or invalid, defaults to 5 PM UTC.</param>
    /// <returns>GymFilteredTravelInfoDto containing travel info for main and auxiliary gyms.</returns>
    public GymFilteredTravelInfoDto CalculateTravelingTimeAndPrice(
        GymsFilteredDto gyms,
        double userLatitude,
        double userLongitude,
        string? departureTime)
    {
        long unixDepartureTime = ParseDepartureTime(departureTime);

        List<GymTravelInfoDto> mainGyms =
            CalculateTravelingTimeAndPrice(gyms.MainGyms, userLatitude, userLongitude, unixDepartureTime);
        List<GymTravelInfoDto> auxGyms =
            CalculateTravelingTimeAndPrice(gyms.AuxGyms, userLatitude, userLongitude, unixDepartureTime);

        return new GymFilteredTravelInfoDto
        {
            MainGyms = mainGyms,
            AuxGyms = auxGyms,
        };
    }

    /// <summary>
    /// Parses the departure time string into a Unix timestamp.
    /// If the input is null or invalid, defaults to 5 PM UTC today or next day if past 5 PM.
    /// </summary>
    /// <param name="departureTimeStr">Departure time as a string.</param>
    /// <returns>Unix timestamp as long?, or null if unable to set a valid departure time.</returns>
    private long ParseDepartureTime(string? departureTimeStr)
    {
        if (!string.IsNullOrWhiteSpace(departureTimeStr))
        {
            // Attempt to parse the departure time string
            if (DateTime.TryParse(departureTimeStr, out DateTime parsedTime))
            {
                // Convert to UTC if the parsed time has a kind of Local or Unspecified
                if (parsedTime.Kind == DateTimeKind.Unspecified)
                {
                    parsedTime = DateTime.SpecifyKind(parsedTime, DateTimeKind.Utc);
                }
                else
                {
                    parsedTime = parsedTime.ToUniversalTime();
                }

                // Convert to Unix timestamp
                return new DateTimeOffset(parsedTime).ToUnixTimeSeconds();
            }
            else
            {
                // Log warning about invalid format if logging is set up
                //_logger.LogWarning("Invalid departureTime format. Defaulting to 5 PM UTC.");
            }
        }

        // If departureTimeStr is null or invalid, set to 5 PM UTC
        var now = DateTime.UtcNow;
        var fivePmToday = new DateTime(now.Year, now.Month, now.Day, 17, 0, 0, DateTimeKind.Utc);

        // If current time is after 5 PM UTC, set to 5 PM next day
        if (now > fivePmToday)
        {
            fivePmToday = fivePmToday.AddDays(1);
        }

        // Convert to Unix timestamp
        return new DateTimeOffset(fivePmToday).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Calculates traveling time and price for a list of gyms using Google Distance Matrix API in transit mode.
    /// </summary>
    /// <param name="gyms">List of Gym objects.</param>
    /// <param name="userLatitude">User's latitude.</param>
    /// <param name="userLongitude">User's longitude.</param>
    /// <param name="departureTime">Desired departure time as Unix timestamp.</param>
    /// <returns>List of GymTravelInfoDto containing travel info for each gym.</returns>
    private List<GymTravelInfoDto> CalculateTravelingTimeAndPrice(
        List<Gym> gyms,
        double userLatitude,
        double userLongitude,
        long departureTime)
    {
        // Call GoogleApiService to get distance matrix in transit mode
        var distanceMatrixResponse = _googleApiService.GetDistanceMatrixAsync(
            originLat: userLatitude,
            originLng: userLongitude,
            gyms: gyms,
            mode: "transit",
            departureTime: departureTime
        ).GetAwaiter().GetResult();

        if (!distanceMatrixResponse.Success)
        {
            // Handle error as needed; throw, log, or return an empty list.
            throw new Exception($"Google Distance Matrix API error: {distanceMatrixResponse.Value}");
        }

        // The returned value is the JsonElement root we received from Google
        var root = (JsonElement)distanceMatrixResponse.Value;

        var result = new List<GymTravelInfoDto>();

        // Safely parse the array of rows (should be only 1 row if there's 1 origin).
        if (root.TryGetProperty("rows", out var rows) && rows.GetArrayLength() > 0)
        {
            var elements = rows[0].GetProperty("elements");

            // Iterate through each gym's corresponding element
            for (int i = 0; i < gyms.Count; i++)
            {
                var gym = gyms[i];
                var element = elements[i];

                // Check individual element status
                var statusProperty = element.GetProperty("status").GetString();
                if (statusProperty != "OK")
                {
                    // If status is not OK, set TravelTime and TravelPrice to null
                    result.Add(new GymTravelInfoDto
                    {
                        Gym = gym,
                        TravelTime = null,
                        TravelPrice = null,
                        Currency = "USD"
                    });
                    continue;
                }

                // Extract distance in meters
                double distanceInMeters = element
                    .GetProperty("distance")
                    .GetProperty("value")
                    .GetDouble();

                // Extract duration in seconds
                double timeInSeconds = element
                    .GetProperty("duration")
                    .GetProperty("value")
                    .GetDouble();

                // Initialize fare variables
                double? fareValue = null;
                string fareCurrency = "USD"; // Default currency

                // Extract fare if available
                if (element.TryGetProperty("fare", out var fare))
                {
                    fareValue = fare.GetProperty("value").GetDouble();
                    fareCurrency = fare.GetProperty("currency").GetString();
                }

                // Convert duration to minutes
                double travelTimeInMinutes = timeInSeconds / 60.0;

                // Add the travel info to the result list
                result.Add(new GymTravelInfoDto
                {
                    Gym = gym,
                    TravelTime = travelTimeInMinutes,
                    TravelPrice = fareValue,
                    Currency = fareCurrency
                });
            }
        }

        return result;
    }
}
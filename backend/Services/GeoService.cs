using System.Text.Json;
using backend.DTO;
using backend.Models;
using backend.Utilities;

// Make sure to include this namespace

namespace backend.Services;

public class GeoService {
    private readonly GoogleApiService _googleApiService;
    private readonly ILogger<GeoService> _logger;

    // Constructor injection for GoogleApiService and ILogger
    public GeoService(GoogleApiService googleApiService, ILogger<GeoService> logger) {
        _googleApiService = googleApiService;
        _logger = logger;
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
        string? departureTime) {
        _logger.LogInformation("Calculating traveling time and price for gyms.");

        long unixDepartureTime = ParseDepartureTime(departureTime);

        _logger.LogDebug("Parsed departure time Unix timestamp: {UnixDepartureTime}", unixDepartureTime);

        List<GymTravelInfoDto> mainGyms =
            CalculateTravelingTimeAndPrice(gyms.MainGyms, userLatitude, userLongitude, unixDepartureTime);
        List<GymTravelInfoDto> auxGyms =
            CalculateTravelingTimeAndPrice(gyms.AuxGyms, userLatitude, userLongitude, unixDepartureTime);

        _logger.LogInformation("Completed calculation of traveling time and price for gyms.");

        return new GymFilteredTravelInfoDto {
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
    private long ParseDepartureTime(string? departureTimeStr) {
        if (!string.IsNullOrWhiteSpace(departureTimeStr)) {
            _logger.LogDebug("Attempting to parse departure time string: {DepartureTimeStr}", departureTimeStr);

            // Attempt to parse the departure time string
            if (DateTime.TryParse(departureTimeStr, out DateTime parsedTime)) {
                _logger.LogDebug("Successfully parsed departure time: {ParsedTime}", parsedTime);

                // Convert to UTC if the parsed time has a kind of Local or Unspecified
                if (parsedTime.Kind == DateTimeKind.Unspecified) {
                    parsedTime = DateTime.SpecifyKind(parsedTime, DateTimeKind.Utc);
                    _logger.LogDebug("Specified DateTimeKind as UTC: {ParsedTime}", parsedTime);
                } else {
                    parsedTime = parsedTime.ToUniversalTime();
                    _logger.LogDebug("Converted parsed time to UTC: {ParsedTime}", parsedTime);
                }

                // Convert to Unix timestamp
                long unixTime = new DateTimeOffset(parsedTime).ToUnixTimeSeconds();
                _logger.LogInformation("Using provided departure time Unix timestamp: {UnixTime}", unixTime);
                return unixTime;
            } else {
                _logger.LogWarning("Invalid departureTime format: {DepartureTimeStr}. Defaulting to 5 PM UTC.",
                    departureTimeStr);
            }
        } else {
            _logger.LogInformation("No departureTime provided. Defaulting to 5 PM UTC.");
        }

        // If departureTimeStr is null or invalid, set to 5 PM UTC

        // I think, it makes more sense to set it for the current time as if the user were to go to the gym right now
        // It is always possible to specify the departure time explicitly, and setting the default one to something static
        // is quite limiting, I think
        // But generally, since it is possible to specify the departure time explicitly, I do not think that the default value is really important
        // So this is just nitpicking and a matter of taste)
        var now = DateTime.UtcNow;
        var fivePmToday = new DateTime(now.Year, now.Month, now.Day, 17, 0, 0, DateTimeKind.Utc);

        // If current time is after 5 PM UTC, set to 5 PM next day
        if (now > fivePmToday) {
            fivePmToday = fivePmToday.AddDays(1);
            _logger.LogInformation(
                "Current time is after 5 PM UTC. Setting departure time to 5 PM UTC next day: {FivePmToday}",
                fivePmToday);
        } else {
            _logger.LogInformation("Setting departure time to 5 PM UTC today: {FivePmToday}", fivePmToday);
        }

        // Convert to Unix timestamp
        long defaultUnixTime = new DateTimeOffset(fivePmToday).ToUnixTimeSeconds();
        _logger.LogDebug("Default departure time Unix timestamp: {DefaultUnixTime}", defaultUnixTime);
        return defaultUnixTime;
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
        long departureTime) {
        _logger.LogInformation("Calculating traveling time and price for {GymCount} gyms.", gyms.Count);

        // Call GoogleApiService to get distance matrix in transit mode
        _logger.LogDebug(
            "Calling Google Distance Matrix API with origin ({UserLatitude}, {UserLongitude}) and departureTime {DepartureTime}.",
            userLatitude, userLongitude, departureTime);

        var distanceMatrixResponse = _googleApiService.GetDistanceMatrixAsync(
            originLat: userLatitude,
            originLng: userLongitude,
            gyms: gyms,
            mode: "transit",
            departureTime: departureTime
        ).GetAwaiter().GetResult();

        if (!distanceMatrixResponse.Success) {
            _logger.LogError("Google Distance Matrix API error: {Error}", distanceMatrixResponse.Value);
            // Handle error as needed; throw, log, or return an empty list.
            throw new Exception($"Google Distance Matrix API error: {distanceMatrixResponse.Value}");
        }

        _logger.LogDebug("Successfully received response from Google Distance Matrix API.");

        // The returned value is the JsonElement root we received from Google
        var root = (JsonElement)distanceMatrixResponse.Value;

        var result = new List<GymTravelInfoDto>();

        // Safely parse the array of rows (should be only 1 row if there's 1 origin).
        if (root.TryGetProperty("rows", out var rows) && rows.GetArrayLength() > 0) {
            _logger.LogDebug("Parsing distance matrix response rows.");

            var elements = rows[0].GetProperty("elements");

            // Iterate through each gym's corresponding element
            for (int i = 0; i < gyms.Count; i++) {
                var gym = gyms[i];
                var element = elements[i];

                // Check individual element status
                var statusProperty = element.GetProperty("status").GetString();
                if (statusProperty != "OK") {
                    _logger.LogWarning(
                        "Distance Matrix element status for Gym ID {GymId} is {Status}. Setting TravelTime and TravelPrice to null.",
                        gym.Id, statusProperty);
                    // If status is not OK, set TravelTime and TravelPrice to null
                    result.Add(new GymTravelInfoDto {
                        Gym = gym,
                        TravelTime = null,
                        TravelPrice = null,
                        Currency = "EUR"
                    });
                    continue;
                }

                try {
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

                    _logger.LogDebug(
                        "Gym ID {GymId}: Distance = {DistanceInMeters} meters, Duration = {TimeInSeconds} seconds.",
                        gym.Id, distanceInMeters, timeInSeconds);

                    // Initialize fare variables
                    double? fareValue = null;
                    string fareCurrency = "EUR"; // Default currency

                    // Extract fare if available
                    if (element.TryGetProperty("fare", out var fare)) {
                        fareValue = fare.GetProperty("value").GetDouble();
                        fareCurrency = fare.GetProperty("currency").GetString() ?? "EUR";
                        _logger.LogDebug("Gym ID {GymId}: Fare = {FareValue} {FareCurrency}.", gym.Id, fareValue,
                            fareCurrency);
                    }

                    // Convert duration to minutes
                    double travelTimeInMinutes = timeInSeconds / 60.0;

                    // Add the travel info to the result list
                    result.Add(new GymTravelInfoDto {
                        Gym = gym,
                        TravelTime = travelTimeInMinutes,
                        TravelPrice = fareValue,
                        Currency = fareCurrency
                    });
                } catch (Exception ex) {
                    _logger.LogError(ex,
                        "Error processing distance matrix element for Gym ID {GymId}. Setting TravelTime and TravelPrice to null.",
                        gym.Id);
                    result.Add(new GymTravelInfoDto {
                        Gym = gym,
                        TravelTime = null,
                        TravelPrice = null,
                        Currency = "EUR"
                    });
                }
            }
        } else {
            _logger.LogWarning("No rows found in distance matrix response.");
        }

        _logger.LogInformation("Completed processing traveling time and price for gyms.");

        return result;
    }
}
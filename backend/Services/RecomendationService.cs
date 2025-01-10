using System.Collections;
using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class RecomendationService
{
    private readonly GymrecommenderContext _dbContext;
    private readonly GeoService _geoService;

    public RecomendationService(GymrecommenderContext context, GeoService geoService)
    {
        _dbContext = context;
        _geoService = geoService;
    }

    private const string MONTHLY_PRICE_WEIGHT = "MonthlyPrice";
    private const string EXTERNAL_RATING_WEIGHT = "ExternalRating";
    private const string CONGESTION_RATING_WEIGHT = "CongestionRating";
    private const string TRAVEL_PRICE_WEIGHT = "TravelPrice";
    private const string TRAVEL_TIME_WEIGHT = "TravelTime";

    protected static readonly Hashtable WEIGHTS_MAP = new Hashtable()
    {
        { MONTHLY_PRICE_WEIGHT, 0.25 },
        { EXTERNAL_RATING_WEIGHT, 0.25 },
        { CONGESTION_RATING_WEIGHT, 0.15 },
        { TRAVEL_PRICE_WEIGHT, 0.20 },
        { TRAVEL_TIME_WEIGHT, 0.15 }
    };

    public async Task<IActionResult> GetRecomendations(GymRecommendationRequestDto gymRecommendationRequest)
    {
        List<Gym> filteredGyms = await GetFilteredGyms(gymRecommendationRequest);
        List<GymTravelInfoDto> filteredGymsWithGeoData = _geoService.CalculateTravelingTimeAndPrice(
            filteredGyms,
            gymRecommendationRequest.Latitude,
            gymRecommendationRequest.Longitude
        );
        List<GymTravelInfoDto> finalFilteredGyms = FilterGymsByTravelData(filteredGymsWithGeoData);

        var recommendations = GetRatings(filteredGymsWithGeoData);
        // TODO: save request and recomndation
        //saveRequest(gymRecomendationRequest);
        //saveRecomensations(recommendations);
        return new OkObjectResult(recommendations);
    }

    /// <summary>
    /// Filters gyms based on the provided parameters: MaxMembershipPrice, MinOverallRating, MinCongestionRating, City.
    /// </summary>
    /// <param name="request">GymRecommendationRequestDto containing filter parameters.</param>
    /// <returns>A list of Gym objects that match the filter criteria.</returns>
    private async Task<List<Gym>> GetFilteredGyms(GymRecommendationRequestDto request)
    {
        // Start with all gyms
        IQueryable<Gym> query = _dbContext.Gyms.AsQueryable();

        // Filter by MaxMembershipPrice if specified
        if (request.MaxMembershipPrice > 0)
        {
            //Todo chenge based on membership length
            query = query.Where(g => g.MonthlyMprice.HasValue && g.MonthlyMprice.Value <= request.MaxMembershipPrice);
        }

        // Filter by MinOverallRating if specified
        if (request.MinOverallRating > 0)
        {
            query = query.Where(g => g.ExternalRating >= request.MinOverallRating);
        }

        // Filter by MinCongestionRating if specified
        if (request.MinCongestionRating > 0)
        {
            query = query.Where(g => g.CongestionRating >= request.MinCongestionRating);
        }

        // Filter by City if specified
        if (!string.IsNullOrEmpty(request.City))
        {
            query = query.Where(g =>
                g.City != null && g.City.Name.Equals(request.City, StringComparison.OrdinalIgnoreCase));
        }

        // Execute the query and return the list
        return await query.ToListAsync();
    }

    /// <summary>
    /// Filters gyms based on TravelPrice and TravelTime thresholds.
    /// </summary>
    /// <param name="gymsWithGeoData">List of GymTravelInfoDto containing travel data.</param>
    /// <returns>Filtered list of GymTravelInfoDto.</returns>
    private List<GymTravelInfoDto> FilterGymsByTravelData(List<GymTravelInfoDto> gymsWithGeoData)
    {
        // Define your thresholds for TravelPrice and TravelTime
        const double MAX_TRAVEL_PRICE = 5.0; // Example threshold
        const double MAX_TRAVEL_TIME = 60.0; // Example threshold in minutes

        // Filter gyms that meet the TravelPrice and TravelTime criteria
        var filtered = gymsWithGeoData
            .Where(g => g.TravelPrice <= MAX_TRAVEL_PRICE && g.TravelTime <= MAX_TRAVEL_TIME)
            .ToList();

        return filtered;
    }

    /// <summary>
    /// Calculates the final ratings for each gym based on multiple criteria.
    /// </summary>
    /// <param name="filteredGyms">List of GymTravelInfoDto after filtering.</param>
    /// <returns>List of GymRecommendationDto with calculated scores.</returns>
    private List<GymRecommendationDto> GetRatings(List<GymTravelInfoDto> filteredGyms)
    {
        if (filteredGyms == null || !filteredGyms.Any())
        {
            return new List<GymRecommendationDto>();
        }

        // Extract the lists for each criterion
        List<double?> membershipPrices = filteredGyms
            .Select(g => g.Gym.MonthlyMprice.HasValue ? (double?)g.Gym.MonthlyMprice.Value : null)
            .ToList();

        List<double?> externalRatings = filteredGyms
            .Select(g => (double?)g.Gym.ExternalRating)
            .ToList(); // Assuming ExternalRating is non-nullable

        List<double?> congestionRatings = filteredGyms
            .Select(g => (double?)g.Gym.CongestionRating)
            .ToList(); // Assuming CongestionRating is non-nullable

        List<double?> travelPrices = filteredGyms
            .Select(g => (double?)g.TravelPrice)
            .ToList();

        List<double?> travelTimes = filteredGyms
            .Select(g => (double?)g.TravelTime)
            .ToList();

        // Normalize each criterion using the helper method
        List<double> normalizedMembershipPrices = NormalizeWithZScoreAndMinMax(membershipPrices);
        List<double> normalizedExternalRatings = NormalizeWithZScoreAndMinMax(externalRatings);
        List<double> normalizedCongestionRatings = NormalizeWithZScoreAndMinMax(congestionRatings);
        List<double> normalizedTravelPrices = NormalizeWithZScoreAndMinMax(travelPrices);
        List<double> normalizedTravelTimes = NormalizeWithZScoreAndMinMax(travelTimes);

        // Assign weights and ensure they sum to 1
        double weightMembershipPrice = WEIGHTS_MAP.ContainsKey(MONTHLY_PRICE_WEIGHT)
            ? Convert.ToDouble(WEIGHTS_MAP[MONTHLY_PRICE_WEIGHT])
            : 0.25;

        double weightExternalRating = WEIGHTS_MAP.ContainsKey(EXTERNAL_RATING_WEIGHT)
            ? Convert.ToDouble(WEIGHTS_MAP[EXTERNAL_RATING_WEIGHT])
            : 0.25;

        double weightCongestionRating = WEIGHTS_MAP.ContainsKey(CONGESTION_RATING_WEIGHT)
            ? Convert.ToDouble(WEIGHTS_MAP[CONGESTION_RATING_WEIGHT])
            : 0.15;

        double weightTravelPrice = WEIGHTS_MAP.ContainsKey(TRAVEL_PRICE_WEIGHT)
            ? Convert.ToDouble(WEIGHTS_MAP[TRAVEL_PRICE_WEIGHT])
            : 0.20;

        double weightTravelTime = WEIGHTS_MAP.ContainsKey(TRAVEL_TIME_WEIGHT)
            ? Convert.ToDouble(WEIGHTS_MAP[TRAVEL_TIME_WEIGHT])
            : 0.15;

        double totalWeight = weightMembershipPrice + weightExternalRating + weightCongestionRating + weightTravelPrice +
                             weightTravelTime;

        // Normalize weights to ensure they sum up to 1
        weightMembershipPrice /= totalWeight;
        weightExternalRating /= totalWeight;
        weightCongestionRating /= totalWeight;
        weightTravelPrice /= totalWeight;
        weightTravelTime /= totalWeight;

        // Calculate Final Score for each gym
        List<GymRecommendationDto> recommendations = new List<GymRecommendationDto>();

        for (int i = 0; i < filteredGyms.Count; i++)
        {
            var gym = filteredGyms[i].Gym;
            var recommendation = new GymRecommendationDto(gym)
            {
                NormalizedMembershipPrice = normalizedMembershipPrices[i],
                NormalizedOverallRating = normalizedExternalRatings[i],
                NormalizedCongestionRating = normalizedCongestionRatings[i],
                NormalizedTravelPrice = normalizedTravelPrices[i],
                NormalizedTravelTime = normalizedTravelTimes[i],
                FinalScore =
                    (normalizedMembershipPrices[i] * weightMembershipPrice) +
                    (normalizedExternalRatings[i] * weightExternalRating) +
                    (normalizedCongestionRatings[i] * weightCongestionRating) +
                    (normalizedTravelPrices[i] * weightTravelPrice) +
                    (normalizedTravelTimes[i] * weightTravelTime)
            };
            recommendations.Add(recommendation);
        }

        // Sort the recommendations in descending order of FinalScore
        recommendations = recommendations.OrderByDescending(r => r.FinalScore).ToList();

        return recommendations;
    }

    /// <summary>
    /// Normalizes a list of nullable doubles using Z-score normalization followed by Min-Max normalization.
    /// Missing values (nulls) are assigned a Z-score of 0, representing the mean.
    /// </summary>
    /// <param name="values">List of nullable double values to normalize.</param>
    /// <returns>List of normalized double values.</returns>
    private List<double> NormalizeWithZScoreAndMinMax(List<double?> values)
    {
        // Extract non-null values for calculations
        var nonNullValues = values.Where(v => v.HasValue).Select(v => v.Value).ToList();

        if (!nonNullValues.Any())
        {
            // If all values are null, return a list of 0.5 (midpoint of normalization)
            return values.Select(v => 0.5).ToList();
        }

        // Calculate mean and standard deviation for Z-score
        double mean = nonNullValues.Average();
        double stdDev = CalculateStandardDeviation(nonNullValues, mean);

        // Handle case where stdDev is zero to avoid division by zero
        if (stdDev == 0)
        {
            stdDev = 1;
        }

        // Compute Z-scores, assign 0 for missing values
        List<double> zScores = values.Select(v =>
            v.HasValue ? (v.Value - mean) / stdDev : 0
        ).ToList();

        // Perform Min-Max normalization on Z-scores
        var validZScores = zScores.Where(z => !double.IsNaN(z)).ToList();

        double minZ = validZScores.Min();
        double maxZ = validZScores.Max();

        double rangeZ = maxZ - minZ;
        if (rangeZ == 0)
        {
            rangeZ = 1;
        }

        List<double> normalizedValues = zScores.Select(z =>
            (z - minZ) / rangeZ
        ).ToList();

        return normalizedValues;
    }

    /// <summary>
    /// Calculates the standard deviation of a list of doubles given their mean.
    /// </summary>
    /// <param name="values">List of double values.</param>
    /// <param name="mean">Mean of the values.</param>
    /// <returns>Standard deviation.</returns>
    private double CalculateStandardDeviation(List<double> values, double mean)
    {
        if (values.Count == 0) return 0;
        double variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Count;
        return Math.Sqrt(variance);
    }
}
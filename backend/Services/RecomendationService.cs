using System.Collections;
using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services;

public class RecomendationService
{
    private const string MONTHLY_PRICE_WEIGHT = "TimePriority";
    private const string EXTERNAL_RATING_WEIGHT = "PricePriority";
    private const string CONGESTION_RATING_WEIGHT = "PricePriority";

    protected static readonly Hashtable WEIGHTS_MAP = new Hashtable()
    {
        { MONTHLY_PRICE_WEIGHT, 0.6 },
        { EXTERNAL_RATING_WEIGHT, 0.2 },
        { CONGESTION_RATING_WEIGHT, 0.2 }
    };

    public async Task<IActionResult> GetRecomendations(GymRecommendationRequestDto gymRecommendationRequestRequest)
    {
        List<Gym> filetredGyms = new List<Gym>(); //TODO: Get gyms by parameters in gymRecomendationRequest
        var recommendations = GetRatings(filetredGyms);
        //TODO: save request and recomndation
        //saveRequest(gymRecomendationRequest);
        //saveRecomensations(recommendations);
        return new OkObjectResult(recommendations);
    }

    public List<GymRecommendationDto> GetRatings(List<Gym> filteredGyms)
    {
        if (filteredGyms == null || !filteredGyms.Any())
        {
            return [];
        }

        // TODO: add time traveling and traveling price

        // Extract the lists for each criterion
        List<double?> monthlyPrices = filteredGyms
            .Select(g => g.MonthlyMprice.HasValue ? (double?)g.MonthlyMprice.Value : null)
            .ToList();

        List<double?> externalRatings = filteredGyms
            .Select(g => (double?)g.ExternalRating)
            .ToList(); // Assuming ExternalRating is non-nullable

        List<double?> congestionRatings = filteredGyms
            .Select(g => (double?)g.CongestionRating)
            .ToList(); // Assuming CongestionRating is non-nullable


        // Normalize each criterion using the helper method
        List<double> normalizedMonthlyPrices = NormalizeWithZScoreAndMinMax(monthlyPrices);
        List<double> normalizedExternalRatings = NormalizeWithZScoreAndMinMax(externalRatings);
        List<double> normalizedCongestionRatings = NormalizeWithZScoreAndMinMax(congestionRatings);

        // Assign weights
        // Ensure the weights sum to 1
        double weightMonthlyPrice = WEIGHTS_MAP.ContainsKey(MONTHLY_PRICE_WEIGHT)
            ? Convert.ToDouble(WEIGHTS_MAP[MONTHLY_PRICE_WEIGHT])
            : 0.6;

        double weightExternalRating = WEIGHTS_MAP.ContainsKey(EXTERNAL_RATING_WEIGHT)
            ? Convert.ToDouble(WEIGHTS_MAP[EXTERNAL_RATING_WEIGHT])
            : 0.2;

        double weightCongestionRating = WEIGHTS_MAP.ContainsKey(CONGESTION_RATING_WEIGHT)
            ? Convert.ToDouble(WEIGHTS_MAP[CONGESTION_RATING_WEIGHT])
            : 0.2;

        double totalWeight = weightMonthlyPrice + weightExternalRating + weightCongestionRating;

        // Normalize weights to ensure they sum up to 1
        weightMonthlyPrice /= totalWeight;
        weightExternalRating /= totalWeight;
        weightCongestionRating /= totalWeight;

        // Calculate Final Score for each gym
        List<GymRecommendationDto> recommendations = new List<GymRecommendationDto>();

        for (int i = 0; i < filteredGyms.Count; i++)
        {
            var gym = filteredGyms[i];
            var recommendation = new GymRecommendationDto(gym)
            {
                NormalizedMembershipPrice = normalizedMonthlyPrices[i],
                NormalizedOverallRating = normalizedExternalRatings[i],
                NormalizedCongestionRating = normalizedCongestionRatings[i],
                FinalScore =
                    (normalizedMonthlyPrices[i] * weightMonthlyPrice) +
                    (normalizedExternalRatings[i] * weightExternalRating) +
                    (normalizedCongestionRatings[i] * weightCongestionRating)
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

        // Step 2: Calculate mean and standard deviation for Z-score
        double mean = nonNullValues.Average();
        double stdDev = CalculateStandardDeviation(nonNullValues, mean);

        // Handle case where stdDev is zero to avoid division by zero
        if (stdDev == 0)
        {
            stdDev = 1;
        }

        //  Compute Z-scores
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
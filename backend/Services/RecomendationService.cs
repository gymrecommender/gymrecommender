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

    // Define static readonly constants for base weights within each group
    // These represent the distribution within price-related and other-related criteria
    private static readonly double BaseMembershipPriceWeight = 0.5; // Within price-related group
    private static readonly double BaseTravelPriceWeight = 0.5; // Within price-related group

    private static readonly double BaseExternalRatingWeight = 0.4; // Within other-related group
    private static readonly double BaseCongestionRatingWeight = 0.3; // Within other-related group
    private static readonly double BaseTravelTimeWeight = 0.3; // Within other-related group

    /// <summary>
    /// Retrieves gym recommendations based on user preferences.
    /// </summary>
    /// <param name="gymRecommendationRequest">User's gym recommendation request.</param>
    /// <returns>List of recommended gyms with scores.</returns>
    public async Task<IActionResult> GetRecomendations(GymRecommendationRequestDto gymRecommendationRequest)
    {
        List<Gym> filteredGyms = await GetFilteredGyms(gymRecommendationRequest);
        List<GymTravelInfoDto> filteredGymsWithGeoData = _geoService.CalculateTravelingTimeAndPrice(
            filteredGyms,
            gymRecommendationRequest.Latitude,
            gymRecommendationRequest.Longitude
        );

        var recommendations = GetRatings(filteredGymsWithGeoData, gymRecommendationRequest.PriceRatingPriority);
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
            //TODO: Change based on membership length
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
    /// Calculates the final ratings for each gym based on multiple criteria.
    /// Adjusts weights based on PriceRatingPriority to balance price-related and other criteria.
    /// </summary>
    /// <param name="filteredGyms">List of GymTravelInfoDto after filtering.</param>
    /// <param name="priceRatingPriority">User's priority for price (0-100).</param>
    /// <returns>List of GymRecommendationDto with calculated scores.</returns>
    private List<GymRecommendationDto> GetRatings(List<GymTravelInfoDto> filteredGyms, int priceRatingPriority)
    {
        if (filteredGyms == null || !filteredGyms.Any())
        {
            return new List<GymRecommendationDto>();
        }

        // Extract the lists for each criterion
        //TODO: Change to use correct field based on membership length
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
        // Pass 'inverted' = false for criteria to maximize (ExternalRating, CongestionRating)
        // Pass 'inverted' = true for criteria to minimize (MembershipPrice, TravelPrice, TravelTime)
        // Note: MembershipPrice is typically something to minimize, but based on original code, it was not inverted.
        // Here, we assume MembershipPrice should be minimized. Adjust if necessary.
        List<double> normalizedMembershipPrices = NormalizeCriteria(membershipPrices, inverted: true);
        List<double> normalizedExternalRatings = NormalizeCriteria(externalRatings, inverted: false);
        List<double> normalizedCongestionRatings = NormalizeCriteria(congestionRatings, inverted: false);
        List<double> normalizedTravelPrices = NormalizeCriteria(travelPrices, inverted: true);
        List<double> normalizedTravelTimes = NormalizeCriteria(travelTimes, inverted: true);

        // Adjust weights based on PriceRatingPriority
        // PriceRatingPriority (0-100) determines the balance between price-related and other criteria
        // Higher PriceRatingPriority gives more emphasis to price-related criteria

        // Convert PriceRatingPriority to a proportion (0.0 to 1.0)
        double pricePriorityProportion = Math.Clamp(priceRatingPriority / 100.0, 0.0, 1.0);
        double otherPriorityProportion = 1.0 - pricePriorityProportion;

        // Distribute the proportions within their respective groups
        // Price-related group: MembershipPrice and TravelPrice
        double membershipPriceWeight = BaseMembershipPriceWeight * pricePriorityProportion;
        double travelPriceWeight = BaseTravelPriceWeight * pricePriorityProportion;

        // Other-related group: ExternalRating, CongestionRating, TravelTime
        double externalRatingWeight = BaseExternalRatingWeight * otherPriorityProportion;
        double congestionRatingWeight = BaseCongestionRatingWeight * otherPriorityProportion;
        double travelTimeWeight = BaseTravelTimeWeight * otherPriorityProportion;

        // Total weight should now sum to 1.0 if pricePriorityProportion + otherPriorityProportion = 1
        // However, due to Base weights, it might not. So, normalize them.
        double totalWeight = membershipPriceWeight + travelPriceWeight + externalRatingWeight + congestionRatingWeight +
                             travelTimeWeight;


        membershipPriceWeight /= totalWeight;
        travelPriceWeight /= totalWeight;
        externalRatingWeight /= totalWeight;
        congestionRatingWeight /= totalWeight;
        travelTimeWeight /= totalWeight;


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
                    (normalizedMembershipPrices[i] * membershipPriceWeight) +
                    (normalizedExternalRatings[i] * externalRatingWeight) +
                    (normalizedCongestionRatings[i] * congestionRatingWeight) +
                    (normalizedTravelPrices[i] * travelPriceWeight) +
                    (normalizedTravelTimes[i] * travelTimeWeight)
            };
            recommendations.Add(recommendation);
        }

        // Sort the recommendations in descending order of FinalScore
        recommendations = recommendations.OrderByDescending(r => r.FinalScore).ToList();

        return recommendations;
    }

    /// <summary>
    /// Normalizes a list of nullable doubles using Z-score normalization followed by Min-Max normalization.
    /// Optionally inverts the normalized values if the criteria should be minimized.
    /// Missing values (nulls) are assigned a Z-score of 0, representing the mean.
    /// </summary>
    /// <param name="values">List of nullable double values to normalize.</param>
    /// <param name="inverted">Indicates whether to invert the normalized values.</param>
    /// <returns>List of normalized (and possibly inverted) double values.</returns>
    private List<double> NormalizeCriteria(List<double?> values, bool inverted)
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

        // Invert the normalized values if required
        if (inverted)
        {
            normalizedValues = normalizedValues.Select(x => 1 - x).ToList();
        }

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

    // /// <summary>
    // /// Saves the gym recommendation request to the database.
    // /// </summary>
    // /// <param name="requestDto">The gym recommendation request DTO.</param>
    // /// <returns>The ID of the saved request.</returns>
    // private Guid SaveRequest(GymRecommendationRequestDto requestDto)
    // {
    //     // Convert DTO to entity
    //     var requestEntity = new RecommendationRequest
    //     {
    //         Id = Guid.NewGuid(),
    //         TimePriority = requestDto.TimePriority,
    //         MembershipLength = requestDto.MembershipLength,
    //         PreferredDepartureTime = requestDto.PreferredDepartureTime,
    //         PreferredArrivalTime = requestDto.PreferredArrivalTime,
    //         MinMembershipPrice = requestDto.MinMembershipPrice,
    //         MinOverallRating = requestDto.MinOverallRating,
    //         MinCongestionRating = requestDto.MinCongestionRating,
    //         City = requestDto.City,
    //         Latitude = requestDto.Latitude,
    //         Longitude = requestDto.Longitude,
    //         CreatedAt = DateTime.UtcNow
    //     };
    //
    //     // Save to DB
    //     _dbContext.RecommendationRequests.Add(requestEntity);
    //     _dbContext.SaveChanges();
    //
    //     return requestEntity.Id;
    // }
    //
    // /// <summary>
    // /// Saves the gym recommendations to the database, linking them to the request ID.
    // /// </summary>
    // /// <param name="requestId">The ID of the recommendation request.</param>
    // /// <param name="recommendations">The list of gym recommendations.</param>
    // private void SaveRecommendations(Guid requestId, List<GymRecommendationDto> recommendations)
    // {
    //     foreach (var recommendation in recommendations)
    //     {
    //         var recommendationEntity = new Recommendation
    //         {
    //             Id = Guid.NewGuid(),
    //             RequestId = requestId,
    //             GymId = recommendation.Gym.Id,
    //             FinalScore = recommendation.FinalScore,
    //             NormalizedMembershipPrice = recommendation.NormalizedMembershipPrice,
    //             NormalizedOverallRating = recommendation.NormalizedOverallRating,
    //             NormalizedCongestionRating = recommendation.NormalizedCongestionRating,
    //             NormalizedTravelPrice = recommendation.NormalizedTravelPrice,
    //             NormalizedTravelTime = recommendation.NormalizedTravelTime,
    //             CreatedAt = DateTime.UtcNow
    //         };
    //
    //         _dbContext.Recommendations.Add(recommendationEntity);
    //     }
    //
    //     _dbContext.SaveChanges();
    // }
}
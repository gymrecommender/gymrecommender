using backend.DTO;
using backend.Enums;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class RecommendationService
{
    private readonly GymrecommenderContext _dbContext;
    private readonly GeoService _geoService;
    private readonly AuthenticationService _authenticationService;

    public RecommendationService(GymrecommenderContext context, GeoService geoService,
        AuthenticationService authenticationService)
    {
        _dbContext = context;
        _geoService = geoService;
        _authenticationService = authenticationService;
    }

    // Define static readonly constants for base weights within each group
    // These represent the distribution within price-related and other-related criteria
    private static readonly double BaseMembershipPriceWeight = 0.8; // Within price-related group
    private static readonly double BaseTravelPriceWeight = 0.2; // Within price-related group

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
        // Start a transaction to ensure atomicity
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Get filtered gyms
            List<Gym> filteredGyms = await GetFilteredGyms(gymRecommendationRequest);

            // Calculate travel data
            List<GymTravelInfoDto> filteredGymsWithGeoData = _geoService.CalculateTravelingTimeAndPrice(
                filteredGyms,
                gymRecommendationRequest.Latitude,
                gymRecommendationRequest.Longitude
            );

            // Calculate ratings
            var recommendations = GetRatings(filteredGymsWithGeoData, gymRecommendationRequest.PriceRatingPriority);
            // TODO: save request and recommendations
            var requestEntity = await SaveRecommendationRequestAsync(gymRecommendationRequest);
            await SaveRecommendationsAsync(requestEntity.Id, recommendations);

            await transaction.CommitAsync();
            return new OkObjectResult(recommendations);
        }
        catch (Exception ex)
        {
            // Rollback the transaction in case of error
            await transaction.RollbackAsync();

            // Log the exception (implement logging as needed)
            // _logger.LogError(ex, "Error while processing recommendations.");

            return new StatusCodeResult(500); // Internal Server Error
        }
    }

    /// <summary>
    /// Retrieves all requests associated with a specific user email. throws KeyNotFoundException if email doe not exists
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <returns>A list of RequestDto objects.</returns>
    public async Task<List<Request>> GetRequestsByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email must be provided.", nameof(email));
        }

        // Fetch the user from the database
        var user = await _dbContext.Accounts
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with email '{email}' not found.");
        }

        // Retrieve requests associated with the user's ID
        var requests = await _dbContext.Requests
            .AsNoTracking()
            .Include(r => r.Recommendations)
            .Where(r => r.UserId == user.Id)
            .ToListAsync();


        // Map Request entities to RequestDto
        return requests;
    }

    /// <summary>
    /// Filters gyms based on the provided parameters: MaxMembershipPrice, MinOverallRating, MinCongestionRating, City.
    /// </summary>
    /// <param name="request">GymRecommendationRequestDto containing filter parameters.</param>
    /// <returns>A list of Gym objects that match the filter criteria.</returns>
    private async Task<List<Gym>> GetFilteredGyms(GymRecommendationRequestDto request)
    {
        // Start with all gyms and include the City navigation property
        IQueryable<Gym> query = _dbContext.Gyms.Include(g => g.City).AsQueryable();

        // Filter by MaxMembershipPrice if specified
        if (request.MaxMembershipPrice > 0)
        {
            // TODO: Change to use correct field based on membership length
            // TODO: Convert currencies
            query = query.Where(
                g => g.MonthlyMprice.HasValue && g.MonthlyMprice.Value <= request.MaxMembershipPrice);
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
                g.City != null && EF.Functions.ILike(g.City.Name, request.City));
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
    public List<GymRecommendationDto> GetRatings(List<GymTravelInfoDto> filteredGyms, int priceRatingPriority)
    {
        if (filteredGyms == null || !filteredGyms.Any())
        {
            return new List<GymRecommendationDto>();
        }

        // Extract the lists for each criterion
        //TODO: Change to use correct field based on membership length
        //TODO: Convert currencies
        List<double?> membershipPrices = filteredGyms
            .Select(g => g.Gym.MonthlyMprice.HasValue ? (double?)g.Gym.MonthlyMprice.Value : null)
            .ToList();

        List<double?> externalRatings = filteredGyms
            .Select(g => (double?)g.Gym.InternalRating)
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
        double totalWeight = membershipPriceWeight + travelPriceWeight + externalRatingWeight +
                             congestionRatingWeight +
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

    /// <summary>
    /// Saves the gym recommendation request to the database.
    /// </summary>
    /// <param name="requestDto">The gym recommendation request DTO.</param>
    /// <returns>The saved Request entity.</returns>
    private async Task<Request> SaveRecommendationRequestAsync(GymRecommendationRequestDto requestDto)
    {
        // Map DTO to entity
        var requestEntity = new Request
        {
            Id = Guid.NewGuid(),
            RequestedAt = DateTime.UtcNow,
            OriginLatitude = requestDto.Latitude,
            OriginLongitude = requestDto.Longitude,
            //TODO: Rename field in db to PriceRatingPriority
            TimePriority = requestDto.PriceRatingPriority,
            TotalCostPriority = requestDto.PriceRatingPriority,
            MinCongestionRating = requestDto.MinCongestionRating,
            MinRating = requestDto.MinOverallRating,
            MinMembershipPrice = (int)requestDto.MaxMembershipPrice,
            //TODO: What is initial purpose if this field? Rename to City if we want to save city name, Name remove request_user_id_name_key constraint from db
            Name = requestDto.City +
                   Guid.NewGuid(), // TODO: remove Guid.NewGuid(), it is currently required to work around request_user_id_name_key
            UserId = await _authenticationService.GetCurrentUserIdAsync(),
            // Initialize other properties if needed
        };

        // Add to DbContext
        _dbContext.Requests.Add(requestEntity);
        await _dbContext.SaveChangesAsync();

        return requestEntity;
    }

    /// <summary>
    /// Saves the gym recommendations to the database, linking them to the request ID.
    /// </summary>
    /// <param name="requestId">The ID of the recommendation request.</param>
    /// <param name="recommendations">The list of gym recommendations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SaveRecommendationsAsync(Guid requestId, List<GymRecommendationDto> recommendations)
    {
        if (recommendations == null || !recommendations.Any())
            return;

        foreach (var recommendation in recommendations)
        {
            var recommendationEntity = new Recommendation
            {
                Id = Guid.NewGuid(),
                GymId = recommendation.Gym.Id, // Ensure GymId is included in RecommendationDto
                RequestId = requestId,
                //TODO: Add separate field for NormalizedTravelPrice, priority is low as we likely do not need to save this in DB.
                Tcost = new decimal(recommendation.NormalizedMembershipPrice), // Adjust mapping as necessary
                Time = TimeOnly.FromTimeSpan(TimeSpan.Parse("00:00")), // Adjust based on actual data
                TimeScore = new decimal(recommendation.NormalizedTravelTime),
                TcostScore = new decimal(recommendation.NormalizedTravelPrice),
                CongestionScore = new decimal(recommendation.NormalizedCongestionRating),
                RatingScore = new decimal(recommendation.NormalizedOverallRating),
                TotalScore = new decimal(recommendation.FinalScore),
                // TODO: Implement alternative recommendations
                Type = RecommendationType.main,
                // TODO: Why do we need currency in this table? Should we move it to the Request table
                CurrencyId = recommendation.Gym.CurrencyId,
            };

            _dbContext.Recommendations.Add(recommendationEntity);
        }

        await _dbContext.SaveChangesAsync();
    }
}
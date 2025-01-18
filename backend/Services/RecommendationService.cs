using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.ViewModels;
using backend.ViewModels.WorkingHour;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class RecommendationService {
    private readonly GymrecommenderContext _dbContext;
    private readonly GeoService _geoService;
    private readonly AuthenticationService _authenticationService;
    private readonly GymRetrievalService _gymRetrievalService;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(GymrecommenderContext context, GeoService geoService,
                                 GymRetrievalService gymRetrievalService, ILogger<RecommendationService> logger,
                                 AuthenticationService authenticationService) {
        _dbContext = context;
        _geoService = geoService;
        _authenticationService = authenticationService;
        _logger = logger;
        _gymRetrievalService = gymRetrievalService;
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
    public async Task<IActionResult> GetRecommendations(GymRecommendationRequestDto gymRecommendationRequest,
                                                        Account? account) {
        // Start a transaction to ensure atomicity
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try {
            // Get filtered gyms
            var gyms = await GetFilteredGyms(gymRecommendationRequest);

            // Calculate travel data
            GymFilteredTravelInfoDto gymsWithGeoData = _geoService.CalculateTravelingTimeAndPrice(
                gyms,
                gymRecommendationRequest.Latitude,
                gymRecommendationRequest.Longitude
            );

            // Calculate ratings
            var recommendations = GetRatings(gymsWithGeoData, gymRecommendationRequest.PriceRatingPriority,
                gymRecommendationRequest.MembershipLength);

            Request? requestEntity = null;
            //Users do not have to be authenticated in order to get the recommendations. In this case we do not save any requested data
            if (account != null) {
                requestEntity = await SaveRecommendationRequestAsync(gymRecommendationRequest, account.Id);
                await SaveRecommendationsAsync(requestEntity.Id, recommendations);
                await transaction.CommitAsync();
            }
            
            return new OkObjectResult(new GymRecommendationResponceDto(requestEntity?.Id, recommendations["MainGyms"], recommendations["AuxGyms"]));
        } catch (Exception ex) {
            // Rollback the transaction in case of error
            await transaction.RollbackAsync();

            // Log the exception (implement logging as needed)
            _logger.LogError(ex, "Error while processing recommendations.");

            return new StatusCodeResult(500); // Internal Server Error
        }
    }

    /// <summary>
    /// Retrieves all requests associated with a specific user email. throws KeyNotFoundException if email doe not exists
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <returns>A list of RequestDto objects.</returns>
    public async Task<List<Request>> GetRequestsByUsernameAsync(string firebaseUid) {
        // Fetch the user from the database
        var user = await _dbContext.Accounts
                                   .AsNoTracking()
                                   .SingleOrDefaultAsync(u => u.OuterUid == firebaseUid && u.Type == AccountType.user);

        if (user == null) {
            throw new KeyNotFoundException($"User has not been found.");
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
    private async Task<GymsFilteredDto> GetFilteredGyms(GymRecommendationRequestDto request) {
        // Start with all gyms
        IQueryable<Gym> query = _dbContext.Gyms
                                          .Include(g => g.GymWorkingHours).ThenInclude(gwh => gwh.WorkingHours)
                                          .Include(g => g.Currency)
                                          .Include(g => g.City).ThenInclude(c => c.Country)
                                          .AsQueryable();

        //Retrieve the city for the specified location
        var cityRes = await _gymRetrievalService.GetCity(request.Latitude, request.Longitude);
        if (!cityRes.Success) throw new Exception(cityRes.Error);

        City city = (City)cityRes.Value;
        query = query.Where(g => g.CityId == city.Id);

        // TODO: Convert currencies
        if (request.MaxMembershipPrice > 0) {
            // Implement conditional filtering based on MembershipLength
            switch (request.MembershipLength) {
                case MembershipLength.Month:
                    query = query.Where(g =>
                        g.MonthlyMprice.HasValue && g.MonthlyMprice.Value <= request.MaxMembershipPrice);
                    break;

                case MembershipLength.HalfYear:
                    query = query.Where(g =>
                        g.SixMonthsMprice.HasValue && g.SixMonthsMprice.Value <= request.MaxMembershipPrice);
                    break;

                case MembershipLength.Year:
                    query = query.Where(g =>
                        g.YearlyMprice.HasValue && g.YearlyMprice.Value <= request.MaxMembershipPrice);
                    break;

                default:
                    query = query.Where(g =>
                        g.MonthlyMprice.HasValue && g.MonthlyMprice.Value <= request.MaxMembershipPrice);
                    break;
            }
        }

        // Filter by MinOverallRating if specified
        if (request.MinOverallRating > 0) query = query.Where(g => g.ExternalRating >= request.MinOverallRating);
        // Filter by MinCongestionRating if specified
        if (request.MinCongestionRating > 0)
            query = query.Where(g => g.CongestionRating >= request.MinCongestionRating);

        var gyms = query.ToList();

        //Separate the gyms into the ones for the main rating and the ones for the auxiliary ratings
        //based on the presence of useful data in all the criteria used in the formation of recommendation ratings
        var mainGymsQuery = gyms.Where(g => g.ExternalRating != 0 && g.CongestionRating != 0);
        switch (request.MembershipLength) {
            case MembershipLength.Month:
                mainGymsQuery = mainGymsQuery.Where(g => g.MonthlyMprice.HasValue);
                break;
            case MembershipLength.HalfYear:
                mainGymsQuery = mainGymsQuery.Where(g => g.SixMonthsMprice.HasValue);
                break;
            case MembershipLength.Year:
                mainGymsQuery = mainGymsQuery.Where(g => g.YearlyMprice.HasValue);
                break;
            default:
                mainGymsQuery = mainGymsQuery.Where(g => g.MonthlyMprice.HasValue);
                break;
        }

        var mainGyms = mainGymsQuery.ToList();
        var auxGyms = gyms.Except(mainGyms).ToList();

        return new GymsFilteredDto {
            MainGyms = mainGyms,
            AuxGyms = auxGyms,
        };
    }

    private double ScaleToRange(double normalizedValue, double minRange, double maxRange)
    {
        return Math.Round(minRange + (normalizedValue * (maxRange - minRange)), 2);
    }

    public List<GymRecommendationDto> FormRatings(List<GymTravelInfoDto> filteredGyms,
                                                  int priceRatingPriority,
                                                  MembershipLength membershipLength,
                                                  int maxSize
    ) {
        List<double?> membershipPrices = filteredGyms
                                         .Select(g =>
                                             g.Gym.GetPrice(membershipLength).HasValue
                                                 ? (double?)g.Gym.GetPrice(membershipLength).Value
                                                 : null)
                                         .ToList();
        
        List<double> travelPrices = filteredGyms
                                     .Select(g => g.TravelPrice)
                                     .ToList();
        
        List<double?> totalPrices = membershipPrices
                                    .Zip(travelPrices, (membershipPrice, travelPrice) =>
                                        membershipPrice.HasValue
                                            ? (double?)(membershipPrice.Value + travelPrice)
                                            : null)
                                    .ToList();

        List<double?> externalRatings = filteredGyms
                                        .Select(g => (double?)(g.Gym.ExternalRating * g.Gym.ExternalRatingNumber + g.Gym.InternalRating * g.Gym.InternalRatingNumber) /
                                                     (g.Gym.ExternalRatingNumber + g.Gym.InternalRatingNumber))
                                        .ToList(); // Assuming ExternalRating is non-nullable

        List<double?> congestionRatings = filteredGyms
                                          .Select(g => (double?)g.Gym.CongestionRating)
                                          .ToList(); // Assuming CongestionRating is non-nullable

        List<double> travelTimes = filteredGyms
                                    .Select(g => g.TravelTime)
                                    .ToList();

        // Normalize each criterion using the helper method
        // Pass 'inverted' = false for criteria to maximize (ExternalRating, CongestionRating)
        // Pass 'inverted' = true for criteria to minimize (MembershipPrice, TravelPrice, TravelTime)
        // Note: MembershipPrice is typically something to minimize, but based on original code, it was not inverted.
        // Here, we assume MembershipPrice should be minimized. Adjust if necessary.
        List<double> scaledTotalPrices = NormalizeCriteria(totalPrices, inverted: true).Select(value => ScaleToRange(value, 1, 10)).ToList();
        List<double> scaledExternalRatings = NormalizeCriteria(externalRatings, inverted: false).Select(value => ScaleToRange(value, 1, 10)).ToList();
        List<double> scaledCongestionRatings = NormalizeCriteria(congestionRatings, inverted: false).Select(value => ScaleToRange(value, 1, 10)).ToList();
        List<double> scaledTravelTimes = NormalizeCriteria(travelTimes.Select(x => (double?)x).ToList(), inverted: true).Select(value => ScaleToRange(value, 1, 10)).ToList();
        
        // Adjust weights based on PriceRatingPriority
        // PriceRatingPriority (0-100) determines the balance between price-related and other criteria
        // Higher PriceRatingPriority gives more emphasis to price-related criteria
        // Convert PriceRatingPriority to a proportion (0.0 to 1.0)
        double pricePriorityProportion = Math.Clamp(priceRatingPriority / 100.0, 0.0, 1.0);
        double otherPriorityProportion = 1.0 - pricePriorityProportion;

        // Distribute the proportions within their respective groups
        // Price-related group: MembershipPrice and TravelPrice
        double totalPriceWeight = BaseMembershipPriceWeight * pricePriorityProportion + BaseTravelPriceWeight * pricePriorityProportion;

        // Other-related group: ExternalRating, CongestionRating, TravelTime
        double externalRatingWeight = BaseExternalRatingWeight * otherPriorityProportion;
        double congestionRatingWeight = BaseCongestionRatingWeight * otherPriorityProportion;
        double travelTimeWeight = BaseTravelTimeWeight * otherPriorityProportion;

        // Total weight should now sum to 1.0 if pricePriorityProportion + otherPriorityProportion = 1
        // However, due to Base weights, it might not. So, normalize them.
        double totalWeight = totalPriceWeight + externalRatingWeight +
                             congestionRatingWeight +
                             travelTimeWeight;


        totalPriceWeight /= totalWeight;
        externalRatingWeight /= totalWeight;
        congestionRatingWeight /= totalWeight;
        travelTimeWeight /= totalWeight;


        // Calculate Final Score for each gym
        List<GymRecommendationDto> recommendations = new List<GymRecommendationDto>();

        for (int i = 0; i < filteredGyms.Count; i++) {
            int hourPart = (int)(travelTimes[i] / 60); // Extract hours
            int minutePart = (int)(travelTimes[i] % 60); // Extract minutes
            
            var gym = filteredGyms[i].Gym;
            var gymView = new GymViewModel {
                Id = gym.Id,
                Name = gym.Name,
                Country = gym.City.Country.Name,
                City = gym.City.Name,
                Address = gym.Address,
                IsOwned = gym.OwnedBy.HasValue,
                Latitude = gym.Latitude,
                Longitude = gym.Longitude,
                IsWheelchairAccessible = gym.IsWheelchairAccessible,
                Currency = gym.Currency.Code,
                PhoneNumber = gym.PhoneNumber,
                MonthlyMprice = gym.MonthlyMprice,
                SixMonthsMprice = gym.SixMonthsMprice,
                YearlyMprice = gym.YearlyMprice,
                Website = gym.Website,
                CurrencyId = gym.CurrencyId,
                WorkingHours = gym.GymWorkingHours.Select(w => new GymWorkingHoursViewModel {
                    Weekday = w.Weekday,
                    OpenFrom = w.WorkingHours.OpenFrom,
                    OpenUntil = w.WorkingHours.OpenUntil,
                }).ToList()
            };
            var recommendation = new GymRecommendationDto(gymView) {
                OverallRating = Math.Round(
                    (scaledTotalPrices[i] * totalPriceWeight) +
                    (scaledExternalRatings[i] * externalRatingWeight) +
                    (scaledCongestionRatings[i] * congestionRatingWeight) +
                    (scaledTravelTimes[i] * travelTimeWeight), 2),
                TimeRating = scaledTravelTimes[i],
                CostRating = scaledTotalPrices[i],
                TravellingTime = new TimeOnly(hourPart, minutePart),
                TotalCost = totalPrices[i] ?? -1,
                CongestionRating = congestionRatings[i] ?? -1,
                RegularRating = externalRatings[i] ?? -1
            };
            recommendations.Add(recommendation);
        }

        // Sort the recommendations in descending order of FinalScore
        return recommendations.OrderByDescending(r => r.OverallRating).Take(maxSize).ToList();
    }

    /// <summary>
    /// Calculates the final ratings for each gym based on multiple criteria.
    /// Adjusts weights based on PriceRatingPriority to balance price-related and other criteria.
    /// </summary>
    /// <param name="filteredGyms">List of GymTravelInfoDto after filtering.</param>
    /// <param name="priceRatingPriority">User's priority for price (0-100).</param>
    /// <returns>List of GymRecommendationDto with calculated scores.</returns>
    public Dictionary<string, List<GymRecommendationDto>> GetRatings(GymFilteredTravelInfoDto filteredGyms,
                                                                     int priceRatingPriority,
                                                                     MembershipLength membershipLength) {
        Dictionary<string, List<GymRecommendationDto>> result = new Dictionary<string, List<GymRecommendationDto>> {
            { "MainGyms", new List<GymRecommendationDto>() },
            { "AuxGyms", new List<GymRecommendationDto>() },
        };

        if (filteredGyms.MainGyms.Any())
            result["MainGyms"] = FormRatings(filteredGyms.MainGyms, priceRatingPriority, membershipLength, 5);
        if (filteredGyms.AuxGyms.Any())
            result["AuxGyms"] = FormRatings(filteredGyms.AuxGyms, priceRatingPriority, membershipLength, 3);

        return result;
    }

    /// <summary>
    /// Normalizes a list of nullable doubles using Z-score normalization followed by Min-Max normalization.
    /// Optionally inverts the normalized values if the criteria should be minimized.
    /// Missing values (nulls) are assigned a Z-score of 0, representing the mean.
    /// </summary>
    /// <param name="values">List of nullable double values to normalize.</param>
    /// <param name="inverted">Indicates whether to invert the normalized values.</param>
    /// <returns>List of normalized (and possibly inverted) double values.</returns>
    private List<double> NormalizeCriteria(List<double?> values, bool inverted) {
        // Extract non-null values for calculations
        var nonNullValues = values.Where(v => v.HasValue).Select(v => v.Value).ToList();

        if (!nonNullValues.Any()) {
            // If all values are null, return a list of 0.5 (midpoint of normalization)
            return values.Select(v => 0.5).ToList();
        }

        // Calculate mean and standard deviation for Z-score
        double mean = nonNullValues.Average();
        double stdDev = CalculateStandardDeviation(nonNullValues, mean);

        // Handle case where stdDev is zero to avoid division by zero
        if (stdDev == 0) {
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
        if (rangeZ == 0) {
            rangeZ = 1;
        }

        List<double> normalizedValues = zScores.Select(z =>
            (z - minZ) / rangeZ
        ).ToList();

        // Invert the normalized values if required
        if (inverted) {
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
    private double CalculateStandardDeviation(List<double> values, double mean) {
        if (values.Count == 0) return 0;
        double variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Count;
        return Math.Sqrt(variance);
    }

    /// <summary>
    /// Saves the gym recommendation request to the database.
    /// </summary>
    /// <param name="requestDto">The gym recommendation request DTO.</param>
    /// <returns>The saved Request entity.</returns>
    private async Task<Request> SaveRecommendationRequestAsync(GymRecommendationRequestDto requestDto, Guid userId) {
        // Map DTO to entity
        var requestEntity = new Request {
            OriginLatitude = requestDto.Latitude,
            OriginLongitude = requestDto.Longitude,
            //TODO: Rename field in db to PriceRatingPriority
            TimePriority = 100 - requestDto.PriceRatingPriority,
            TotalCostPriority = requestDto.PriceRatingPriority,
            MinCongestionRating = requestDto.MinCongestionRating >= 1 ? requestDto.MinCongestionRating : 1,
            MinRating = requestDto.MinOverallRating >= 1 ? requestDto.MinOverallRating : 1,
            MinMembershipPrice = (int)requestDto.MaxMembershipPrice,
            //TODO: What is initial purpose if this field? Rename to City if we want to save city name, Name remove request_user_id_name_key constraint from db
            UserId = userId
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
    private async Task SaveRecommendationsAsync(Guid requestId,
                                                Dictionary<string, List<GymRecommendationDto>> recommendations) {
        var recommendationEntities = new List<Recommendation>();

        foreach (var type in new[] { "MainGyms", "AuxGyms" }) {
            if (!recommendations.ContainsKey(type))
                continue;

            var recommendationType = type == "MainGyms"
                ? RecommendationType.main
                : RecommendationType.alternative;

            recommendationEntities.AddRange(recommendations[type].Select(recommendation => new Recommendation {
                Id = Guid.NewGuid(),
                GymId = recommendation.Gym.Id,
                RequestId = requestId,
                // TODO: Add separate field for NormalizedTravelPrice if needed
                Tcost = new decimal(recommendation.TotalCost), // Adjust mapping as necessary
                Time = recommendation.TravellingTime, // Adjust based on actual data
                TimeScore = new decimal(recommendation.TimeRating),
                TcostScore = new decimal(recommendation.CostRating),
                CongestionScore = new decimal(recommendation.CongestionRating),
                RatingScore = new decimal(recommendation.RegularRating),
                TotalScore = new decimal(recommendation.OverallRating),
                Type = recommendationType,
                // TODO: Evaluate if CurrencyId should be moved to the Request table
                CurrencyId = recommendation.Gym.CurrencyId,
            }));
        }

        if (recommendationEntities.Any()) {
            _dbContext.Recommendations.AddRange(recommendationEntities);
            await _dbContext.SaveChangesAsync();
        }
    }
}
using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.ViewModels;
using backend.ViewModels.WorkingHour;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

    /// <summary>
    /// Retrieves gym recommendations based on user preferences.
    /// </summary>
    /// <param name="gymRecommendationRequest">User's gym recommendation request.</param>
    /// <returns>List of recommended gyms with scores.</returns>
    public async Task<IActionResult> GetRecommendations(GymRecommendationRequestDto gymRecommendationRequest,
                                                        Account? account) {
        _logger.LogInformation(
            "GetRecommendations called with Request: {@GymRecommendationRequest}, Account: {@Account}",
            gymRecommendationRequest, account);

        // Start a transaction to ensure atomicity
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        _logger.LogInformation("Database transaction started.");

        try {
            // This will retrieve the gyms if there are none in the city of the current location
            var retrievalCheck = await _gymRetrievalService.RetrieveGyms(gymRecommendationRequest.Latitude,
                gymRecommendationRequest.Longitude);
            if (!retrievalCheck.Success) {
                return new StatusCodeResult(500);
            }

            // Get filtered gyms
            var gyms = await GetFilteredGyms(gymRecommendationRequest);
            _logger.LogInformation("Filtered gyms retrieved: {Count}", gyms.MainGyms.Count + gyms.AuxGyms.Count);

            // Calculate travel data
            GymFilteredTravelInfoDto gymsWithGeoData = _geoService.CalculateTravelingTimeAndPrice(
                gyms,
                gymRecommendationRequest.Latitude,
                gymRecommendationRequest.Longitude,
                gymRecommendationRequest.PreferredDepartureTime
            );
            _logger.LogInformation("Traveling time and price calculated for gyms.");

            // Calculate ratings
            var recommendations = GetRatings(gymsWithGeoData, gymRecommendationRequest.PriceRatingPriority,
                gymRecommendationRequest.MembershipLength);
            _logger.LogInformation("Gym ratings calculated. MainGyms: {MainGymsCount}, AuxGyms: {AuxGymsCount}",
                recommendations["MainGyms"].Count, recommendations["AuxGyms"].Count);

            Request? requestEntity = null;
            //Users do not have to be authenticated in order to get the recommendations. In this case we do not save any requested data
            if (account != null && account.Type == AccountType.user) {
                requestEntity = await SaveRecommendationRequestAsync(gymRecommendationRequest, account.Id);
                _logger.LogInformation("Recommendation request saved with RequestId: {RequestId}", requestEntity.Id);

                await SaveRecommendationsAsync(requestEntity.Id, recommendations);
                _logger.LogInformation("Recommendations saved for RequestId: {RequestId}", requestEntity.Id);

                await transaction.CommitAsync();
                _logger.LogInformation("Database transaction committed.");
            } else {
                _logger.LogInformation("No account provided. Skipping saving recommendation request.");
            }

            return new OkObjectResult(new GymRecommendationResponceDto(requestEntity?.Id,
                gymRecommendationRequest.Latitude, gymRecommendationRequest.Longitude, recommendations["MainGyms"],
                recommendations["AuxGyms"]));
        } catch (Exception ex) {
            // Rollback the transaction in case of error
            await transaction.RollbackAsync();

            // Log the exception (implement logging as needed)
            _logger.LogError(ex, "Error while processing recommendations.");

            throw ex; // Internal Server Error
        }
    }

    /// <summary>
    /// Retrieves all requests associated with a specific user email. throws KeyNotFoundException if email doe not exists
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <returns>A list of RequestDto objects.</returns>
    public async Task<List<Request>> GetRequestsByUsernameAsync(string firebaseUid) {
        _logger.LogInformation("GetRequestsByUsernameAsync called with FirebaseUid: {FirebaseUid}", firebaseUid);

        // Fetch the user from the database
        var user = await _dbContext.Accounts
                                   .AsNoTracking()
                                   .SingleOrDefaultAsync(u => u.OuterUid == firebaseUid && u.Type == AccountType.user);

        if (user == null) {
            _logger.LogWarning("User not found for FirebaseUid: {FirebaseUid}", firebaseUid);
            throw new KeyNotFoundException($"User has not been found.");
        }

        _logger.LogInformation("User found with Id: {UserId}", user.Id);

        // Retrieve requests associated with the user's ID
        var requests = await _dbContext.Requests
                                       .AsNoTracking()
                                       .Include(r => r.Recommendations)
                                       .Where(r => r.UserId == user.Id)
                                       .ToListAsync();

        _logger.LogInformation("Retrieved {RequestCount} requests for UserId: {UserId}", requests.Count, user.Id);

        // Map Request entities to RequestDto
        return requests;
    }

    /// <summary>
    /// Filters gyms based on the provided parameters: MaxMembershipPrice, MinOverallRating, MinCongestionRating, City.
    /// </summary>
    /// <param name="request">GymRecommendationRequestDto containing filter parameters.</param>
    /// <returns>A list of Gym objects that match the filter criteria.</returns>
    private async Task<GymsFilteredDto> GetFilteredGyms(GymRecommendationRequestDto request) {
        _logger.LogInformation("GetFilteredGyms called with Request: {@Request}", request);

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
        if (request.MaxMembershipPrice < 1000) {
            // TODO: Convert currencies, why is it set to 100?
            _logger.LogInformation("Applying MaxMembershipPrice filter: {MaxMembershipPrice}",
                request.MaxMembershipPrice);
            // Implement conditional filtering based on MembershipLength
            switch (request.MembershipLength) {
                case MembershipLength.Month:
                    query = query.Where(g =>
                        g.MonthlyMprice.HasValue && g.MonthlyMprice.Value <= request.MaxMembershipPrice);
                    break;

                case MembershipLength.Halfyear:
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
        if (request.MinOverallRating > 1) query = query.Where(g => g.ExternalRating >= request.MinOverallRating);
        // Filter by MinCongestionRating if specified
        if (request.MinCongestionRating > 1)
            query = query.Where(g => g.CongestionRating >= request.MinCongestionRating);

        var gyms = query.ToList();

        //Separate the gyms into the ones for the main rating and the ones for the auxiliary ratings
        //based on the presence of useful data in all the criteria used in the formation of recommendation ratings
        var mainGymsQuery = gyms.Where(g => g.ExternalRating != 0 && g.CongestionRating != 0);
        switch (request.MembershipLength) {
            case MembershipLength.Month:
                mainGymsQuery = mainGymsQuery.Where(g => g.MonthlyMprice.HasValue);
                break;
            case MembershipLength.Halfyear:
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
        _logger.LogInformation("MainGyms count: {MainGymsCount}, AuxGyms count: {AuxGymsCount}", mainGyms.Count,
            auxGyms.Count);

        return new GymsFilteredDto {
            MainGyms = mainGyms,
            AuxGyms = auxGyms,
        };
    }

    private double ScaleToRange(double normalizedValue, double minRange, double maxRange) {
        return Math.Round(minRange + (normalizedValue * (maxRange - minRange)), 2);
    }

    public List<GymRecommendationDto> FormRatings(List<GymTravelInfoDto> filteredGyms,
                                                  int priceRatingPriority,
                                                  MembershipLength membershipLength,
                                                  int maxSize
    ) {
        _logger.LogInformation(
            "FormRatings called with PriceRatingPriority: {PriceRatingPriority}, MembershipLength: {MembershipLength}, MaxSize: {MaxSize}",
            priceRatingPriority, membershipLength, maxSize);

        List<double?> membershipPrices = filteredGyms
                                         .Select(g =>
                                             g.Gym.GetPrice(membershipLength).HasValue
                                                 ? (double?)g.Gym.GetPrice(membershipLength).Value
                                                 : null)
                                         .ToList();

        List<double?> travelPrices = filteredGyms
                                     .Select(g => g.TravelPrice)
                                     .ToList();

        List<double?> totalPrices = membershipPrices
                                    .Zip(travelPrices, (membershipPrice, travelPrice) => {
                                        if (!membershipPrice.HasValue) return (double?)null;
                                        double? sum = membershipPrice.Value;

                                        //It does affect the rating and total price but not enough to discard the whole membership when there was no traver time retrieved
                                        if (travelPrice.HasValue) sum += travelPrice.Value;
                                        return sum;
                                    })
                                    .ToList();

        List<double?> externalRatings = filteredGyms
                                        .Select(g =>
                                            (double?)(g.Gym.ExternalRating * g.Gym.ExternalRatingNumber +
                                                      g.Gym.InternalRating * g.Gym.InternalRatingNumber) /
                                            (g.Gym.ExternalRatingNumber + g.Gym.InternalRatingNumber))
                                        .ToList(); // Assuming ExternalRating is non-nullable

        List<double?> congestionRatings = filteredGyms
                                          .Select(g => (double?)g.Gym.CongestionRating)
                                          .ToList(); // Assuming CongestionRating is non-nullable

        List<double?> travelTimes = filteredGyms
                                    .Select(g => g.TravelTime)
                                    .ToList();

        _logger.LogInformation("Criteria lists prepared for normalization.");

        // Normalize each criterion using the helper method
        // Pass 'inverted' = false for criteria to maximize (ExternalRating, CongestionRating)
        // Pass 'inverted' = true for criteria to minimize (MembershipPrice, TravelPrice, TravelTime)
        // Note: MembershipPrice is typically something to minimize, but based on original code, it was not inverted.
        // Here, we assume MembershipPrice should be minimized. Adjust if necessary.
        List<double> scaledTotalPrices = NormalizeCriteria(totalPrices, inverted: true)
                                         .Select(value => ScaleToRange(value, 1, 10)).ToList();
        List<double> scaledExternalRatings = NormalizeCriteria(externalRatings, inverted: false)
                                             .Select(value => ScaleToRange(value, 1, 10)).ToList();
        List<double> scaledCongestionRatings = NormalizeCriteria(congestionRatings, inverted: false)
                                               .Select(value => ScaleToRange(value, 1, 10)).ToList();
        List<double> scaledTravelTimes = NormalizeCriteria(travelTimes.Select(x => (double?)x).ToList(), inverted: true)
                                         .Select(value => ScaleToRange(value, 1, 10)).ToList();

        _logger.LogInformation("Normalization of criteria completed.");
        // Adjust weights based on PriceRatingPriority
        // PriceRatingPriority (0-100) determines the balance between price-related and other criteria
        // Higher PriceRatingPriority gives more emphasis to price-related criteria
        // Convert PriceRatingPriority to a proportion (0.0 to 1.0)
        double pricePriorityProportion = Math.Clamp(priceRatingPriority / 100.0, 0.0, 1.0);
        double otherPriorityProportion = 1.0 - pricePriorityProportion;

        _logger.LogInformation(
            "PricePriorityProportion: {PricePriorityProportion}, OtherPriorityProportion: {OtherPriorityProportion}",
            pricePriorityProportion, otherPriorityProportion);

        // Distribute the proportions within their respective groups
        // Price-related group: MembershipPrice and TravelPrice
        double totalPriceWeight = pricePriorityProportion * 0.68;
        double travelTimeWeight = otherPriorityProportion * 0.68;

        // Other-related group: ExternalRating, CongestionRating, TravelTime
        double regularRatingWeight = 0.2;
        double congestionRatingWeight = 0.12;

        // Total weight should now sum to 1.0 if pricePriorityProportion + otherPriorityProportion = 1
        // However, due to Base weights, it might not. So, normalize them.
        double totalWeight = totalPriceWeight + regularRatingWeight +
                             congestionRatingWeight +
                             travelTimeWeight;

        totalPriceWeight /= totalWeight;
        regularRatingWeight /= totalWeight;
        congestionRatingWeight /= totalWeight;
        travelTimeWeight /= totalWeight;

        _logger.LogInformation(
            "Weights calculated - TotalPriceWeight: {TotalPriceWeight}, RegularRatingWeight: {RegularRatingWeight}, CongestionRatingWeight: {CongestionRatingWeight}, TravelTimeWeight: {TravelTimeWeight}",
            totalPriceWeight, regularRatingWeight, congestionRatingWeight, travelTimeWeight);

        // Calculate Final Score for each gym
        List<GymRecommendationDto> recommendations = new List<GymRecommendationDto>();
        for (int i = 0; i < filteredGyms.Count; i++) {
            int hourPart = 0;
            int minutePart = 0;
            if (travelTimes[i] != null) {
                hourPart = (int)(travelTimes[i] / 60); // Extract hours
                minutePart = (int)(travelTimes[i] % 60); // Extract minutes
            }

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
                CongestionRating = gym.CongestionRating,
                Rating = (gym.ExternalRatingNumber + gym.InternalRatingNumber) > 0
                    ? (decimal)Math.Round(
                        ((double)gym.ExternalRating * gym.ExternalRatingNumber +
                         (double)gym.InternalRating * gym.InternalRatingNumber) /
                        (gym.ExternalRatingNumber + gym.InternalRatingNumber), 2)
                    : 0,
                WorkingHours = gym.GymWorkingHours.Select(w => new GymWorkingHoursViewModel {
                    Weekday = w.Weekday,
                    OpenFrom = w.WorkingHours.OpenFrom,
                    OpenUntil = w.WorkingHours.OpenUntil,
                }).ToList()
            };
            var recommendation = new GymRecommendationDto(gymView) {
                OverallRating = Math.Round(
                    (scaledTotalPrices[i] * totalPriceWeight) +
                    (scaledExternalRatings[i] * regularRatingWeight) +
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

        _logger.LogInformation("Final recommendations formed with count: {RecommendationCount}", recommendations.Count);

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
        _logger.LogInformation(
            "GetRatings called with PriceRatingPriority: {PriceRatingPriority}, MembershipLength: {MembershipLength}",
            priceRatingPriority, membershipLength);

        Dictionary<string, List<GymRecommendationDto>> result = new Dictionary<string, List<GymRecommendationDto>> {
            { "MainGyms", new List<GymRecommendationDto>() },
            { "AuxGyms", new List<GymRecommendationDto>() },
        };

        if (filteredGyms.MainGyms.Any()) {
            _logger.LogInformation("Processing MainGyms.");
            result["MainGyms"] = FormRatings(filteredGyms.MainGyms, priceRatingPriority, membershipLength, 5);
            _logger.LogInformation("MainGyms processed with count: {MainGymsCount}", result["MainGyms"].Count);
        }

        if (filteredGyms.AuxGyms.Any()) {
            _logger.LogInformation("Processing AuxGyms.");
            result["AuxGyms"] = FormRatings(filteredGyms.AuxGyms, priceRatingPriority, membershipLength, 3);
            _logger.LogInformation("AuxGyms processed with count: {AuxGymsCount}", result["AuxGyms"].Count);
        }

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
        _logger.LogInformation("NormalizeCriteria called with Inverted: {Inverted}", inverted);

        // Extract non-null values for calculations
        var nonNullValues = values.Where(v => v.HasValue).Select(v => v.Value).ToList();

        if (!nonNullValues.Any()) {
            _logger.LogWarning("All values are null. Assigning default normalized value of 0.5 to all entries.");
            // If all values are null, return a list of 0.5 (midpoint of normalization)
            return values.Select(v => 0.5).ToList();
        }

        // Calculate mean and standard deviation for Z-score
        double mean = nonNullValues.Average();
        double stdDev = CalculateStandardDeviation(nonNullValues, mean);
        _logger.LogInformation("Normalization stats - Mean: {Mean}, StdDev: {StdDev}", mean, stdDev);

        // Handle case where stdDev is zero to avoid division by zero
        if (stdDev == 0) {
            stdDev = 1;
            _logger.LogWarning("Standard deviation is zero. Set to 1 to avoid division by zero.");
        }

        // Compute Z-scores, assign 0 for missing values
        List<double> zScores = values.Select(v =>
            v.HasValue ? (v.Value - mean) / stdDev : 0
        ).ToList();

        _logger.LogInformation("Z-scores computed.");

        // Perform Min-Max normalization on Z-scores
        var validZScores = zScores.Where(z => !double.IsNaN(z)).ToList();

        double minZ = validZScores.Min();
        double maxZ = validZScores.Max();

        _logger.LogInformation("Z-scores range - Min: {MinZ}, Max: {MaxZ}", minZ, maxZ);

        double rangeZ = maxZ - minZ;
        if (rangeZ == 0) {
            rangeZ = 1;
            _logger.LogWarning("Z-score range is zero. Set to 1 to avoid division by zero.");
        }

        List<double> normalizedValues = zScores.Select(z =>
            (z - minZ) / rangeZ
        ).ToList();

        _logger.LogInformation("Min-Max normalization completed.");

        // Invert the normalized values if required
        if (inverted) {
            normalizedValues = normalizedValues.Select(x => 1 - x).ToList();
            _logger.LogInformation("Normalized values inverted.");
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
        _logger.LogInformation("Saving recommendation request for UserId: {UserId}", userId);

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
            MembType = requestDto.MembershipLength,
            UserId = userId,
            ArrivalTime = !requestDto.PreferredArrivalTime.IsNullOrEmpty()
                ? TimeOnly.Parse(requestDto.PreferredArrivalTime)
                : null,
            DepartureTime = !requestDto.PreferredDepartureTime.IsNullOrEmpty()
                ? TimeOnly.Parse(requestDto.PreferredDepartureTime)
                : null,
            // Initialize other properties if needed
        };

        // Add to DbContext
        _dbContext.Requests.Add(requestEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Recommendation request saved with RequestId: {RequestId}", requestEntity.Id);

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
        _logger.LogInformation("Saving recommendations for RequestId: {RequestId}", requestId);

        var recommendationEntities = new List<Recommendation>();

        foreach (var type in new[] { "MainGyms", "AuxGyms" }) {
            if (!recommendations.ContainsKey(type)) {
                _logger.LogWarning("Recommendation type {Type} not found in recommendations dictionary.", type);
                continue;
            }

            var recommendationType = type == "MainGyms"
                ? RecommendationType.main
                : RecommendationType.alternative;

            _logger.LogInformation("Processing {Type} with RecommendationType: {RecommendationType}", type,
                recommendationType);

            recommendationEntities.AddRange(recommendations[type].Select(recommendation => new Recommendation {
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


            _logger.LogInformation("Total recommendations to save: {Count}", recommendationEntities.Count);
        }

        if (recommendationEntities.Any()) {
            _dbContext.Recommendations.AddRange(recommendationEntities);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Recommendations saved successfully for RequestId: {RequestId}", requestId);
        } else {
            _logger.LogWarning("No recommendations to save for RequestId: {RequestId}", requestId);
        }
    }
}
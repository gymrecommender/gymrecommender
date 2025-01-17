using backend.Enums;

namespace backend.DTO;

public class GymRecommendationRequestDto
{
    // TODO: add field validation
    public int PriceRatingPriority { get; set; } // 0 to 100 scale
    public MembershipLength MembershipLength { get; set; } // e.g., "1 month"
    public string? PreferredDepartureTime { get; set; } // Optional, format "hh:mm"
    public string? PreferredArrivalTime { get; set; } // Optional, format "hh:mm"
    public int MaxMembershipPrice { get; set; } // 0 to 10000 scale
    public int MinOverallRating { get; set; } // 1 to 5 scale
    public int MinCongestionRating { get; set; } // 1 to 5 scale
    public double Latitude { get; set; } // Latitude of the location
    public double Longitude { get; set; } // Longitude of the location
}
using backend.Models;
using backend.ViewModels;

namespace backend.DTO;

public class GymRecommendationDto
{
    public GymViewModel Gym { get; set; }

    public double CostRating { get; set; }
    public double OverallRating { get; set; }
    public double TimeRating { get; set; }
    public TimeOnly TravellingTime { get; set; }
    public double TotalCost { get; set; }
    public double CongestionRating { get; set; }
    public double RegularRating { get; set; }

    public GymRecommendationDto(GymViewModel gym)
    {
        Gym = gym;
    }
}
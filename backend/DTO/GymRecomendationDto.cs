using backend.Models;

namespace backend.DTO;

public class GymRecommendationDto
{
    public Gym Gym { get; set; }

    public double NormalizedMembershipPrice { get; set; }

    public double NormalizedOverallRating { get; set; }

    public double NormalizedCongestionRating { get; set; }

    public double FinalScore { get; set; }

    public GymRecommendationDto(Gym gym)
    {
        Gym = gym;
    }
}
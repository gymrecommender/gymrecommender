using backend.Models;

namespace backend.DTO;

public class GymTravelInfoDto
{
    public Gym Gym { get; set; }
    public double TravelPrice { get; set; }
    public double TravelTime { get; set; }
}
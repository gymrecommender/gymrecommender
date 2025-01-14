using backend.DTO;
using backend.Models;

namespace backend.Services;

public class GeoService
{
    public List<GymTravelInfoDto> CalculateTravelingTimeAndPrice(
        List<Gym> gyms,
        double userLatitude,
        double userLongitude)
    {
        var travelDataList = new List<GymTravelInfoDto>();

        foreach (var gym in gyms)
        {
            // TODO: Replace mocked values with actual Google Direction API calls in the future
            travelDataList.Add(new GymTravelInfoDto
            {
                Gym = gym,
                TravelPrice = 2.0,
                TravelTime = 30.0
            });
        }

        return travelDataList;
    }
}
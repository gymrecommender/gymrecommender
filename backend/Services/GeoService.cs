using backend.DTO;
using backend.Models;

namespace backend.Services;

public class GeoService
{
    public GymFilteredTravelInfoDto CalculateTravelingTimeAndPrice(
        GymsFilteredDto gyms,
        double userLatitude,
        double userLongitude)
    {
        var travelDataList = new GymFilteredTravelInfoDto();
        
        foreach (var gym in gyms.MainGyms)
        {
            // TODO: Replace mocked values with actual Google Direction API calls in the future
            travelDataList.MainGyms.Add(new GymTravelInfoDto
            {
                Gym = gym,
                TravelPrice = 2.0,
                TravelTime = 30.0
            });
        }
        
        foreach (var gym in gyms.AuxGyms)
        {
            // TODO: Replace mocked values with actual Google Direction API calls in the future
            travelDataList.AuxGyms.Add(new GymTravelInfoDto
            {
                Gym = gym,
                TravelPrice = 2.0,
                TravelTime = 30.0
            });
        }

        return travelDataList;
    }
}
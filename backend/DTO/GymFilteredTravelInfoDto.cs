namespace backend.DTO;

public class GymFilteredTravelInfoDto {
    public List<GymTravelInfoDto> MainGyms { get; set; } = new List<GymTravelInfoDto>();
    public List<GymTravelInfoDto> AuxGyms { get; set; } = new List<GymTravelInfoDto>();
}
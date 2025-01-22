using backend.Models;

namespace backend.DTO;

public class GymsFilteredDto {
    public List<Gym> MainGyms { get; set; }
    public List<Gym> AuxGyms { get; set; }
}
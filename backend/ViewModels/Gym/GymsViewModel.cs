using backend.Models;

namespace backend.ViewModels;

public class GymsViewModel {
    public IEnumerable<GymViewModel> Gyms { get; set; } = new List<GymViewModel>();

    public PagingInfo PagingInfo { get; set; } = new PagingInfo();
}
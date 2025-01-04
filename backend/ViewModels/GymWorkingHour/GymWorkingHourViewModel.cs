namespace backend.ViewModels.WorkingHour;

public class GymWorkingHourViewModel {
    public int Weekday { get; set; }
    
    public TimeOnly OpenFrom { get; set; }

    public TimeOnly OpenUntil { get; set; }
}
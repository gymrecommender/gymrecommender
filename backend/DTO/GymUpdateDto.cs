using backend.Models;

namespace backend.DTO;

public class GymUpdateDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public decimal? MonthlyMprice { get; set; }
    public decimal? YearlyMprice { get; set; }
    public decimal? SixMonthsMprice { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public bool? isWheelchairAccessible { get; set; }
    
    public GymWorkingHour[]? WorkingHours { get; set; } 
}

public partial class WorkingHourDto
{
    public int Weekday { get; set; } // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
    public TimeOnly OpenFrom { get; set; }
    public TimeOnly OpenUntil { get; set; }
}
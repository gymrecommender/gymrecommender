public class GymUpdateDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public decimal? MonthlyMprice { get; set; }
    public decimal? YearlyMprice { get; set; }
    public decimal? SixMonthsMprice { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public bool? IsWheelchairAccessible { get; set; }
    public string? Currency { get; set; }
    public List<GymWorkingHourUpdateDto>? WorkingHours { get; set; }
}

public class GymWorkingHourUpdateDto
{
    public int Weekday { get; set; }
    public TimeOnly? OpenFrom { get; set; }
    public TimeOnly? OpenUntil { get; set; }
}
using backend.ViewModels.WorkingHour;

namespace backend.ViewModels;

public class GymViewModel {
    public Guid Id { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string Name { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string Address { get; set; } = null!;

    public string? Website { get; set; }

    public bool IsWheelchairAccessible { get; set; }

    public decimal? MonthlyMprice { get; set; }

    public decimal? YearlyMprice { get; set; }

    public decimal? SixMonthsMprice { get; set; }

    public string Currency { get; set; }

    public string City { get; set; }
    
    public string Country { get; set; }
    
    public bool IsOwned { get; set; }
    
    public Guid CurrencyId { get; set; }
    
    public decimal CongestionRating { get; set; }
    
    public decimal Rating { get; set; }
    
    public List<GymWorkingHoursViewModel> WorkingHours { get; set; } = null!;
}
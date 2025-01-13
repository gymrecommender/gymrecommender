using System.Text.Json.Serialization;
using backend.Enums;

namespace backend.Models;

public partial class Gym
{
    public Guid Id { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string Name { get; set; } = null!;

    public string ExternalPlaceId { get; set; } = null!;

    public decimal ExternalRating { get; set; }

    public int ExternalRatingNumber { get; set; }

    public string? PhoneNumber { get; set; }

    public string Address { get; set; } = null!;

    public string? Website { get; set; }

    public bool IsWheelchairAccessible { get; set; }

    public decimal? MonthlyMprice { get; set; }

    public decimal? YearlyMprice { get; set; }

    public decimal? SixMonthsMprice { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PriceChangedAt { get; set; }

    public DateTime? ChangedAt { get; set; }

    public decimal InternalRating { get; set; }

    public int InternalRatingNumber { get; set; }

    public decimal CongestionRating { get; set; }

    public int CongestionRatingNumber { get; set; }

    public Guid? OwnedBy { get; set; }

    public Guid CurrencyId { get; set; }

    public Guid CityId { get; set; }

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual City City { get; set; } = null!;

    public virtual ICollection<CongestionRating> CongestionRatings { get; set; } = new List<CongestionRating>();

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<GymWorkingHour> GymWorkingHours { get; set; } = new List<GymWorkingHour>();
    [JsonIgnore] public virtual Account? OwnedByNavigation { get; set; }
    [JsonIgnore] public virtual ICollection<Ownership> Ownerships { get; set; } = new List<Ownership>();
    [JsonIgnore] public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    [JsonIgnore] public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

    public decimal? GetPrice(MembershipLength membershipLength)
    {
        return membershipLength switch
        {
            MembershipLength.Month => MonthlyMprice,
            MembershipLength.HalfYear => SixMonthsMprice,
            MembershipLength.Year => YearlyMprice,
            _ => MonthlyMprice
        };
    }
}
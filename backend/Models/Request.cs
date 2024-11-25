using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Request
{
    public Guid Id { get; set; }

    public DateTime RequestedAt { get; set; }

    public double OriginLatitude { get; set; }

    public double OriginLongitude { get; set; }

    public int TimePriority { get; set; }

    public int TotalCostPriority { get; set; }

    public decimal MinCongestionRating { get; set; }

    public decimal MinRating { get; set; }

    public int MinMembershipPrice { get; set; }

    public string? Name { get; set; }

    public Guid UserId { get; set; }

    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

    public virtual ICollection<RequestPeriod> RequestPeriods { get; set; } = new List<RequestPeriod>();

    public virtual Account User { get; set; } = null!;
}

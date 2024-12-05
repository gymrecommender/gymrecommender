using System;
using System.Collections.Generic;
using backend.Enums;

namespace backend.Models;

public partial class Recommendation
{
    public Guid Id { get; set; }

    public decimal Tcost { get; set; }

    public TimeOnly Time { get; set; }

    public decimal TimeScore { get; set; }

    public decimal TcostScore { get; set; }

    public decimal? CongestionScore { get; set; }

    public decimal? RatingScore { get; set; }

    public decimal TotalScore { get; set; }
    
    public RecommendationType Type { get; set; }

    public Guid GymId { get; set; }

    public Guid RequestId { get; set; }

    public Guid CurrencyId { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual Gym Gym { get; set; } = null!;

    public virtual Request Request { get; set; } = null!;
}

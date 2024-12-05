using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class CongestionRating
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ChangedAt { get; set; }

    public TimeOnly VisitTime { get; set; }

    public int Weekday { get; set; }

    public int AvgWaitingTime { get; set; }

    public int Crowdedness { get; set; }

    public Guid GymId { get; set; }

    public Guid UserId { get; set; }

    public virtual Gym Gym { get; set; } = null!;

    public virtual Account User { get; set; } = null!;
}

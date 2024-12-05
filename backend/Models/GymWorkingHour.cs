using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class GymWorkingHour
{
    public Guid Id { get; set; }

    public int Weekday { get; set; }

    public DateTime? ChangedAt { get; set; }

    public Guid GymId { get; set; }

    public Guid WorkingHoursId { get; set; }

    public virtual Gym Gym { get; set; } = null!;

    public virtual WorkingHour WorkingHours { get; set; } = null!;
}

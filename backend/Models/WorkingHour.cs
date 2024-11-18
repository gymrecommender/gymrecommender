using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class WorkingHour
{
    public Guid Id { get; set; }

    public TimeOnly OpenFrom { get; set; }

    public TimeOnly OpenUntil { get; set; }

    public virtual ICollection<GymWorkingHour> GymWorkingHours { get; set; } = new List<GymWorkingHour>();
}

using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class RequestPeriod
{
    public Guid Id { get; set; }

    public Guid RequestId { get; set; }

    public int Weekday { get; set; }

    public TimeOnly? ArrivalTime { get; set; }

    public TimeOnly? DepartureTime { get; set; }

    public virtual Request Request { get; set; } = null!;
}

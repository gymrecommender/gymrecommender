using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Availability
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? ChangedAt { get; set; }

    public Guid GymId { get; set; }

    public Guid MarkedBy { get; set; }
}

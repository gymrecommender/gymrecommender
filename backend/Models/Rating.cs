using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Rating
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ChangedAt { get; set; }

    public int Rating1 { get; set; }

    public Guid UserId { get; set; }

    public Guid GymId { get; set; }

    public virtual Gym Gym { get; set; } = null!;

    public virtual Account User { get; set; } = null!;
}

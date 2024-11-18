using System;
using System.Collections.Generic;
using backend.Enums;

namespace backend.Models;

public partial class Ownership
{
    public Guid Id { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? RespondedAt { get; set; }

    public string? Message { get; set; }
    
    public OwnershipDecision Decision { get; set; }

    public Guid? RespondedBy { get; set; }

    public Guid RequestedBy { get; set; }

    public Guid GymId { get; set; }

    public virtual Gym Gym { get; set; } = null!;

    public virtual Account RequestedByNavigation { get; set; } = null!;

    public virtual Account? RespondedByNavigation { get; set; }
}

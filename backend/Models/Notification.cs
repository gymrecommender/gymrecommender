using System;
using System.Collections.Generic;
using backend.Enums;

namespace backend.Models;

public partial class Notification
{
    public Guid Id { get; set; }

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }
    
    public NotificationType Type { get; set; }

    public Guid UserId { get; set; }

    public virtual Account User { get; set; } = null!;
}

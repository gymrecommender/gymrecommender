using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class RequestPause
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public byte[]? Ip { get; set; }

    public DateTime StartedAt { get; set; }

    public virtual Account? User { get; set; }
}

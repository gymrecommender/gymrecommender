using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Currency
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public virtual ICollection<Gym> Gyms { get; set; } = new List<Gym>();

    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
}

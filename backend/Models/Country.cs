using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Country
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}

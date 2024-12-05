using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class City
{
    public Guid Id { get; set; }

    public double Nelatitude { get; set; }

    public double Nelongitude { get; set; }

    public double Swlatitude { get; set; }

    public double Swlongitude { get; set; }

    public string Name { get; set; } = null!;

    public Guid CountryId { get; set; }

    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<Gym> Gyms { get; set; } = new List<Gym>();
}

namespace backend.Views;

public class Geocode {
    public string City { get; set; } = null!;
    
    public string Country { get; set; } = null!;
    
    public double Nelatitude { get; set; }

    public double Nelongitude { get; set; }

    public double Swlatitude { get; set; }

    public double Swlongitude { get; set; }
}
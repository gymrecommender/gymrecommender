using System.Text.Json;
using backend.Views;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace backend.Utilities;

using DotNetEnv;

public class GoogleApi {
    private static string apiKey;

    public static void setApiKey() {
        Env.Load();
        apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
    }
    
    public static async Task<Geocode?> GetCity(double latitude, double longitude) {
        var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={apiKey}";

        using (HttpClient client = new HttpClient()) {
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode) {
                string json = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(json);
                var result = jsonDoc.RootElement.GetProperty("results").EnumerateArray().First();

                var addressComponents = result.GetProperty("address_components");
                var returnResult = new Geocode();

                foreach (var component in addressComponents.EnumerateArray()) {
                    var types = component.GetProperty("types").EnumerateArray()
                                         .Select(type => type.GetString());

                    if (types.Any(type => type == "locality")) {
                        returnResult.City = component.GetProperty("long_name").GetString();
                    } else if (types.Any(type => type == "country")) {
                        returnResult.Country = component.GetProperty("long_name").GetString();
                    }
                }

                var viewport = result.GetProperty("geometry").GetProperty("viewport");
                var northeast = viewport.GetProperty("northeast");
                returnResult.Nelatitude = northeast.GetProperty("lat").GetDouble();
                returnResult.Nelongitude = northeast.GetProperty("lng").GetDouble();

                var southwest = viewport.GetProperty("southwest");
                returnResult.Swlatitude = southwest.GetProperty("lat").GetDouble();
                returnResult.Swlongitude = southwest.GetProperty("lng").GetDouble();

                return returnResult;
            } else {
                return null;
            }
        }
    }
}
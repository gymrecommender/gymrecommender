using System.Globalization;
using backend.Models;
using backend.Utilities;
using backend.ViewModels;
using backend.ViewModels.WorkingHour;
using backend.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class GymRetrievalService {
    private readonly GymrecommenderContext _context;
    private readonly GoogleApiService _googleApiService;

    public GymRetrievalService(GymrecommenderContext context, GoogleApiService googleApiService) {
        _context = context;
        _googleApiService = googleApiService;
    }

    public int CalculateCityRadius(City city) {
        double ToRadians(double angle) => Math.PI * angle / 180.0;
        double earthRad = 6371 * 1e3;

        double lat1 = ToRadians(city.Nelatitude);
        double lng1 = ToRadians(city.Nelongitude);
        double lat2 = ToRadians(city.Swlatitude);
        double lng2 = ToRadians(city.Swlongitude);
        double deltaLat = lat2 - lat1;
        double deltaLng = lng2 - lng1;

        //Generally, we want to calculate the approximate radius of the city in order to retrieve as much gyms
        //as possible from different districts of the city whenever that is possible
        //Haversine formula is helping us with that
        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return int.Min(Convert.ToInt32(earthRad * c / 2), 50000);
    }

    public async Task<Response> RetrieveGymsByCity(string country, string city) {
        try {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            var reqCity = _context.Cities.AsNoTracking()
                                  .Include(c => c.Country)
                                  .Where(c => c.Name == textInfo.ToTitleCase(city))
                                  .Where(c => c.Country.Name == textInfo.ToTitleCase(country))
                                  .FirstOrDefault();

            if (reqCity == null) {
                return new Response(
                    ErrorMessage.ErrorMessages["Location error"],
                    "Location error",
                    true
                );
            }

            var gyms = _context.Gyms.AsNoTracking()
                               .Include(g => g.City).ThenInclude(c => c.Country)
                               .Include(g => g.Currency)
                               .Include(g => g.GymWorkingHours).ThenInclude(w => w.WorkingHours)
                               .Where(g => g.CityId == reqCity.Id)
                               .OrderBy(g => g.CreatedAt)
                               .Select(g => new GymViewModel {
                                   Id = g.Id,
                                   Name = g.Name,
                                   Address = g.Address,
                                   City = g.City.Name,
                                   Country = g.City.Country.Name,
                                   Currency = g.Currency.Code,
                                   Latitude = g.Latitude,
                                   Longitude = g.Longitude,
                                   MonthlyMprice = g.MonthlyMprice,
                                   IsWheelchairAccessible = g.IsWheelchairAccessible,
                                   PhoneNumber = g.PhoneNumber,
                                   YearlyMprice = g.YearlyMprice,
                                   SixMonthsMprice = g.SixMonthsMprice,
                                   Website = g.Website,
                                   IsOwned = g.OwnedBy.HasValue,
                                   CurrencyId = g.CurrencyId,
                                   CongestionRating = g.CongestionRating,
                                   Rating = Math.Round(
                                       (g.ExternalRating * g.ExternalRatingNumber +
                                        g.InternalRating * g.InternalRatingNumber) /
                                       (g.ExternalRatingNumber + g.InternalRatingNumber), 2),
                                   WorkingHours = g.GymWorkingHours.Select(w => new GymWorkingHoursViewModel {
                                       Weekday = w.Weekday,
                                       OpenFrom = w.WorkingHours.OpenFrom,
                                       OpenUntil = w.WorkingHours.OpenUntil,
                                   }).ToList()
                               })
                               .AsSplitQuery()
                               .ToList();

            return new Response(gyms);
        } catch (Exception e) {
            return new Response(
                ErrorMessage.ErrorMessages["Internal error"],
                "Internal error",
                true
            );
        }
    }

    public async Task<Response> GetCity(double lat, double lng) {
        Response response = await _googleApiService.GetCity(lat, lng);
        if (!response.Success) return response;

        Geocode newCity = (Geocode)response.Value;
        var cityCheck = _context.Cities.AsNoTracking()
                                .Include(c => c.Country)
                                .Where(c => c.Name == newCity.City && c.Country.Name == newCity.Country)
                                .Where(c => c.Swlatitude <= lat && lat <= c.Nelatitude)
                                .Where(c => c.Swlongitude <= lng && lng <= c.Nelongitude)
                                .FirstOrDefault();

        if (cityCheck != null) {
            return new Response(cityCheck);
        }

        var country = _context.Countries.AsNoTracking()
                              .FirstOrDefault(c => c.Name == newCity.Country);

        if (country == null) {
            country = new Country {
                Name = newCity.Country
            };

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();
        } else {
            _context.Attach(country);
        }

        var city = new City {
            Name = newCity.City,
            Country = country,
            Nelongitude = newCity.Nelongitude,
            Nelatitude = newCity.Nelatitude,
            Swlongitude = newCity.Swlongitude,
            Swlatitude = newCity.Swlatitude,
        };
        _context.Cities.Add(city);
        await _context.SaveChangesAsync();

        return new Response(city);
    }
}
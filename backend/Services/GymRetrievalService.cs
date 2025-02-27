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

    public Response ParseError(Response gymsRes) {
        if (gymsRes.ErrorCode == "Internal error") {
            return new Response(
                gymsRes.Error,
                "500",
                true
            );
        }

        return new Response(
            gymsRes.Error,
            "400",
            true
        );
    }

    public async Task<Response> RetrieveGyms(double lat, double lng, bool useGoogleAPI = true) {
        try {
            var cityRes = await GetCity(lat, lng);
            if (!cityRes.Success) {
                return new Response(
                    cityRes.Error,
                    cityRes.ErrorCode,
                    true
                );
            }

            City city = (City)cityRes.Value;

            var gymsRes = await RetrieveGymsByCity(city.Country.Name, city.Name);
            if (!gymsRes.Success) return ParseError(gymsRes);

            //If we already have at least one gym for the area, considering that we do not have functionality
            //of deleting a gym from the table for any account type, we can conclude that we have already
            //retrieved the gyms for the current city and it is enough to just return these gyms
            if (((List<GymViewModel>)gymsRes.Value).Count() != 0) return new Response(gymsRes);

            // Since we are limited by the number of allowed free requests to the Google API
            // We will retrieve the gyms for the city in general, and for the specific location of the user
            // This way the results will be less personalized and optimal, but we will avoid paying for the services
            var cityMiddleLat = (city.Nelatitude + city.Swlatitude) / 2;
            var cityMiddleLng = (city.Nelongitude + city.Swlongitude) / 2;

            //Retrieving the gyms for the current city via the Google API
            var result = await _googleApiService.GetGyms(cityMiddleLat, cityMiddleLng,
                CalculateCityRadius(city), city);
            if (!result.Success) return ParseError(result);

            //Saving retrieved gyms, working hours and their relations to the database
            var save = await SaveNewGyms((List<Tuple<Gym, List<GymWorkingHoursViewModel>>>)result.Value);
            if (!save.Success) return ParseError(save);

            //Retrieving the gyms for the city from the database
            gymsRes = await RetrieveGymsByCity(city.Country.Name, city.Name);
            if (!gymsRes.Success) return ParseError(gymsRes);

            return new Response(gymsRes);
        } catch (Exception e) {
            return new Response(
                e.Message,
                "500",
                true
            );
        }
    }

    [NonAction]
    private async Task<Response> SaveNewGyms(List<Tuple<Gym, List<GymWorkingHoursViewModel>>> data) {
        try {
            var existingWorkingHours = await _context.WorkingHours.AsNoTracking().ToListAsync();
            var dbWorkingHours = existingWorkingHours
                                 .Select(wh => new WorkingHour {
                                     Id = wh.Id,
                                     OpenFrom = wh.OpenFrom,
                                     OpenUntil = wh.OpenUntil
                                 })
                                 .ToList();
            
            //TODO currency should be determined in some other way
            var currency = _context.Currencies.AsNoTracking().Where(c => c.Code == "EUR").ToList().First();

            foreach (var dataItem in data) {
                var gym = dataItem.Item1;
                gym.CurrencyId = currency.Id;
                
                var gymCheck = _context.Gyms.FirstOrDefault(g => g.ExternalPlaceId == gym.ExternalPlaceId);
                if (gymCheck != null) continue;
                _context.Gyms.Add(gym);

                var workingHours = dataItem.Item2;
                foreach (var wHour in workingHours) {
                    var match = existingWorkingHours.FirstOrDefault(wh =>
                        wh.OpenFrom == wHour.OpenFrom && wh.OpenUntil == wHour.OpenUntil);

                    //We are checking whether the working hours for the current gym has already been stored in the table
                    if (match == null) {
                        match = new WorkingHour {
                            OpenFrom = wHour.OpenFrom,
                            OpenUntil = wHour.OpenUntil,
                        };
                        _context.WorkingHours.Add(match);
                        existingWorkingHours.Add(match);
                    } else {
                        //We need to track the already existing entries in the WorkingHour table in order for dotnet to
                        //not try to create another instance of it upon saving
                        var matchDb = dbWorkingHours.FirstOrDefault(wh =>
                            wh.OpenFrom == match.OpenFrom && wh.OpenUntil == match.OpenUntil);

                        if (matchDb != null) _context.Attach(match);
                    }

                    //Binding working hours with the working hours of the current gym
                    _context.GymWorkingHours.Add(new GymWorkingHour {
                        Gym = gym,
                        Weekday = wHour.Weekday,
                        WorkingHours = match
                    });
                }
            }

            await _context.SaveChangesAsync();
            return new Response("");
        } catch (Exception e) {
            return new Response(
                e.Message,
                e.HResult.ToString(),
                true
            );
        }
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

            var gyms = _context.Gyms
                               .Include(g => g.City).ThenInclude(c => c.Country)
                               .Include(g => g.Currency)
                               .Include(g => g.GymWorkingHours).ThenInclude(w => w.WorkingHours)
                               .Where(g => g.CityId == reqCity.Id)
                               .OrderBy(g => g.CreatedAt)
                               .AsSplitQuery()
                               .ToList();

            var gymViewModels = gyms.Select(g => new GymViewModel {
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
                Rating = g.ExternalRatingNumber + g.InternalRatingNumber > 0
                    ? (decimal)Math.Round(
                        ((double)g.ExternalRating * g.ExternalRatingNumber +
                         (double)g.InternalRating * g.InternalRatingNumber) /
                        (g.ExternalRatingNumber + g.InternalRatingNumber), 2)
                    : 0,
                WorkingHours = g.GymWorkingHours.Select(w => new GymWorkingHoursViewModel {
                    Weekday = w.Weekday,
                    OpenFrom = w.WorkingHours.OpenFrom,
                    OpenUntil = w.WorkingHours.OpenUntil,
                }).ToList()
            }).ToList();

            return new Response(gymViewModels);
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
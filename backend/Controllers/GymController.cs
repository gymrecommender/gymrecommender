using System.Globalization;
using backend.Enums;
using backend.Models;
using backend.Utilities;
using backend.ViewModels;
using backend.ViewModels.WorkingHour;
using backend.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class GymController : Controller {
    private readonly GymrecommenderContext _context;
    private readonly AppSettings _appData;
    private readonly GoogleApi _googleApi;

    public GymController(GymrecommenderContext context, IOptions<AppSettings> appSettings, GoogleApi googleApi) {
        _context = context;
        _appData = appSettings.Value;
        _googleApi = googleApi;
    }

    [HttpGet("{country}/{city}")]
    public async Task<IActionResult> GetGymsByCity(string country, string city) {
        try {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            var reqCity = _context.Cities.AsNoTracking()
                                 .Include(c => c.Country)
                                 .Where(c => c.Name == textInfo.ToTitleCase(city))
                                 .Where(c => c.Country.Name == textInfo.ToTitleCase(country))
                                 .FirstOrDefault();

            if (reqCity == null) {
                return NotFound(new {
                    success = false, error = new {
                        code = "Location error",
                        message = ErrorMessage.ErrorMessages["Location error"]
                    }
                });
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
                                  WorkingHours = g.GymWorkingHours.Select(w => new GymWorkingHourViewModel {
                                      Weekday = w.Weekday,
                                      OpenFrom = w.WorkingHours.OpenFrom,
                                      OpenUntil = w.WorkingHours.OpenUntil,
                                  }).ToList()
                              })
                              .AsSplitQuery()
                              .ToList();

            return Ok(gyms);
        } catch (Exception e) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    code = "Internal error",
                    message = ErrorMessage.ErrorMessages["Internal error"],
                }
            });
        }
    }

    [NonAction]
    private async Task<Response> AddCity(double lat, double lng) {
        Response response = await _googleApi.GetCity(lat, lng);
        if (!response.Success) return response;

        Geocode newCity = (Geocode)response.Value;
            
        var country = _context.Countries.AsNoTracking()
                             .FirstOrDefault(c => c.Name == newCity.Country);

        if (country == null) {
            country = new Country {
                Name = newCity.Country
            };
                
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();
        }

        var city = new City {
            Name = newCity.City,
            CountryId = country.Id,
            Nelongitude = newCity.Nelongitude,
            Nelatitude = newCity.Nelatitude,
            Swlongitude = newCity.Swlongitude,
            Swlatitude = newCity.Swlatitude,
        };
        _context.Cities.Add(city);
        await _context.SaveChangesAsync();

        return new Response(city);
    }
    
    [NonAction]
    private async Task<Response> RetrieveGyms(double lat, double lng) {
        try {
            var cityRes = await AddCity(lat, lng);
            if (!cityRes.Success) return cityRes;
            
            return cityRes;
        } catch (Exception e) {
            return new Response(
                e.Message,
                e.HResult.ToString(),
                true
            );
        }
    }

    [HttpGet("location")]
    public async Task<IActionResult> GetGymsForLocation([FromQuery] double lat, [FromQuery] double lng) {
        try {
            var city = _context.Cities.AsNoTracking()
                              .Include(c => c.Country)
                              .Where(c => c.Swlatitude <= lat && lat <= c.Nelatitude)
                              .Where(c => c.Swlongitude <= lng && lng <= c.Nelongitude)
                              .ToList();

            if (city.Count == 0) {
                var res = await RetrieveGyms(lat, lng);
                if (!res.Success) {
                    return StatusCode(Convert.ToInt32(res.ErrorCode), new {
                        success = false,
                        error = new {
                            code = res.ErrorCode,
                            message = res.Error,
                        }
                    });
                }

                return Ok(res.Value);
            } else if (city.Count > 1) {
                //TODO do something when there is more than one city that satisfies these criteria
            }
            
            return await GetGymsByCity(city[0].Country.Name, city[0].Name);
        } catch (Exception e) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    code = "Internal error",
                    message = ErrorMessage.ErrorMessages["Internal error"],
                }
            });
        }
    }
}
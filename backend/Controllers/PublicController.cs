using System.Globalization;
using backend.Enums;
using backend.Models;
using backend.Utilities;
using backend.ViewModels;
using backend.ViewModels.WorkingHour;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class PublicController : Controller {
    protected readonly GymrecommenderContext context;
    protected readonly AppSettings appData;

    public PublicController(GymrecommenderContext context, IOptions<AppSettings> appSettings) {
        this.context = context;
        appData = appSettings.Value;
    }

    [HttpGet("{country}/{city}")]
    public async Task<IActionResult> GetGymsByCity(string country, string city) {
        try {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            var reqCity = context.Cities.AsNoTracking()
                .Include(c => c.Country)
                .Where(c => c.Name == textInfo.ToTitleCase(city))
                .Where(c => c.Country.Name == textInfo.ToTitleCase(country))
                .FirstOrDefault();
            
            if (reqCity == null) {
                return NotFound(new{ success = false, error = new
                {
                    code = "Location error",
                    message = ErrorMessage.ErrorMessages["Location error"]
                }});
            }

            var gyms = context.Gyms.AsNoTracking()
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
        }
        catch (Exception e) {
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
using System.Globalization;
using backend.Enums;
using backend.Models;
using backend.Services;
using backend.Utilities;
using backend.ViewModels;
using backend.ViewModels.WorkingHour;
using backend.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sprache;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class GymController : Controller {
    private readonly GymrecommenderContext _context;
    private readonly AppSettings _appData;
    private readonly GoogleApiService _googleApiService;
    private readonly GymRetrievalService _gymRetrievalService;

    public GymController(GymrecommenderContext context, IOptions<AppSettings> appSettings, GoogleApiService googleApiService, GymRetrievalService gymRetrievalService) {
        _context = context;
        _appData = appSettings.Value;
        _googleApiService = googleApiService;
        _gymRetrievalService = gymRetrievalService;
    }


    [HttpGet("currencies")]
    public async Task<IActionResult> GetCurrencies() {
        try {
            var currencies = await _context.Currencies
                                           .AsNoTracking()
                                           .Select(c => new {
                                               c.Id,
                                               c.Code,
                                               c.Name,
                                           })
                                           .ToListAsync();

            return Ok(currencies);
        } catch (Exception ex) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    code = "InternalError",
                    message = ex.Message
                }
            });
        }
    }


    [HttpGet("{country}/{city}")]
    public async Task<IActionResult> GetGymsByCity(string country, string city) {
        var gymsRes = await _gymRetrievalService.RetrieveGymsByCity(country, city);
        if (!gymsRes.Success) return ParseError(gymsRes);

        return Ok(gymsRes.Value);
    }

    [HttpGet("location")]
    public async Task<IActionResult> GetGymsForLocation([FromQuery] double lat, [FromQuery] double lng) {
        try {
            var cityRes = await _gymRetrievalService.GetCity(lat, lng);
            if (!cityRes.Success) {
                return StatusCode(Convert.ToInt32(cityRes.ErrorCode), new {
                    success = false,
                    error = new {
                        code = cityRes.ErrorCode,
                        message = cityRes.Error,
                    }
                });
            }
            City city = (City)cityRes.Value;

            var gymsRes = await _gymRetrievalService.RetrieveGymsByCity(city.Country.Name, city.Name);
            if (!gymsRes.Success) return ParseError(gymsRes);

            //If we already have at least one gym for the area, considering that we do not have functionality
            //of deleting a gym from the table for any account type, we can conclude that we have already
            //retrieved the gyms for the current city and it is enough to just return these gyms
            if (((List<GymViewModel>)gymsRes.Value).Count() != 0) return Ok(gymsRes);

            // Since we are limited by the number of allowed free requests to the Google API
            // We will retrieve the gyms for the city in general, and for the specific location of the user
            // This way the results will be less personalized and optimal, but we will avoid paying for the services
            var cityMiddleLat = (city.Nelatitude + city.Swlatitude) / 2;
            var cityMiddleLng = (city.Nelongitude + city.Swlongitude) / 2;
            
            //Retrieving the gyms for the current city via the Google API
            var result = await _googleApiService.GetGyms(cityMiddleLat, cityMiddleLng, _gymRetrievalService.CalculateCityRadius(city), city);
            if (!result.Success) return ParseError(result);
            
            //Saving retrieved gyms, working hours and their relations to the database
            var save = await SaveNewGyms((List<Tuple<Gym, List<GymWorkingHoursViewModel>>>)result.Value);
            if (!save.Success) return ParseError(save);
            
            //Retrieving the gyms for the city from the database
            gymsRes = await _gymRetrievalService.RetrieveGymsByCity(city.Country.Name, city.Name);
            if (!gymsRes.Success) return ParseError(gymsRes);
            
            return Ok(gymsRes);
        } catch (Exception e) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    code = "Internal error",
                    message = e.Message,
                }
            });
        }
    }

    [NonAction]
    public IActionResult ParseError(Response gymsRes) {
        if (gymsRes.ErrorCode == "Internal error") {
            return StatusCode(500, new {
                success = false,
                error = new {
                    code = gymsRes.ErrorCode,
                    message = gymsRes.Error,
                }
            });
        }

        return BadRequest(new {
            success = false,
            error = new {
                code = gymsRes.ErrorCode,
                message = gymsRes.Error,
            }
        });
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

[HttpGet("api/gym/getpausebyip")]
public async Task<IActionResult> GetPauseByIp([FromQuery] string? ip = null)
{
    try
    {
        // Use the provided IP address or fall back to the client's IP address
        string clientIp = ip ?? HttpContext.Connection.RemoteIpAddress?.ToString();

        // Validation: Ensure the IP address is available
        if (string.IsNullOrEmpty(clientIp))
        {
            return BadRequest(new
            {
                success = false,
                error = new
                {
                    code = "InvalidRequest",
                    message = "Could not retrieve the IP address."
                }
            });
        }

        // Convert the IP to a byte array
        byte[] clientIpBytes = clientIp.Contains(":")
            ? System.Net.IPAddress.Parse(clientIp).GetAddressBytes() // IPv6
            : System.Net.IPAddress.Parse(clientIp).GetAddressBytes(); // IPv4

        // Query the database for a matching pause entry
        var pause = await _context.RequestPauses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Ip != null && p.Ip.SequenceEqual(clientIpBytes));

        if (pause == null)
        {
            return NotFound(new
            {
                success = false,
                message = "No pause found for the given IP address."
            });
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                pause.Id,
                Ip = string.Join(".", pause.Ip.Select(b => b.ToString())), // Convert IP bytes back to string
                pause.StartedAt
            }
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            success = false,
            error = new
            {
                code = "InternalError",
                message = ex.Message
            }
        });
    }
}

  [Authorize(Policy = "UserOnly")]
[HttpGet("pause-by-user/{userId}")]
public async Task<IActionResult> GetPauseByUserId(Guid userId)
{
    try
    {
        // Query the database for a matching pause entry by user_id
        var pause = await _context.RequestPauses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (pause == null)
        {
            return NotFound(new
            {
                success = false,
                message = "No pause found for the given user ID."
            });
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                pause.Id,
                pause.UserId,
                pause.Ip,
                pause.StartedAt
            }
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            success = false,
            error = new
            {
                code = "InternalError",
                message = ex.Message
            }
        });
    }
}
    
}
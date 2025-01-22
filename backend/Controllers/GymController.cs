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
using backend.DTOs;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class GymController : Controller {
    private readonly GymrecommenderContext _context;
    private readonly AppSettings _appData;
    private readonly GoogleApiService _googleApiService;
    private readonly GymRetrievalService _gymRetrievalService;

    public GymController(GymrecommenderContext context, IOptions<AppSettings> appSettings,
                         GoogleApiService googleApiService, GymRetrievalService gymRetrievalService) {
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
        if (!gymsRes.Success) {
            var error = _gymRetrievalService.ParseError(gymsRes);
            return StatusCode(int.Parse(error.ErrorCode), new {
                message = error.Error,
            });
        }

        return Ok(gymsRes.Value);
    }

    [HttpGet("location")]
    public async Task<IActionResult> GetGymsForLocation([FromQuery] double lat, [FromQuery] double lng,
                                                        [FromQuery] bool gApi = true) {
        var result = await _gymRetrievalService.RetrieveGyms(lat, lng, gApi);
        if (!result.Success) {
            return StatusCode(int.Parse(result.ErrorCode), new {
                message = result.Error,
            });
        }

        return Ok(result.Value);
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

    [HttpGet("pause")]
    public async Task<IActionResult> GetPauseByIp() {
        try {
            string? clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(clientIp)) {
                return BadRequest(new {
                    success = false,
                    error = new {
                        code = "InvalidRequest",
                        message = "Could not retrieve the IP address."
                    }
                });
            }

            // Convert the IP to a byte array
            byte[] clientIpBytes = System.Net.IPAddress.Parse(clientIp).GetAddressBytes();

            // Query the database for a matching pause entry
            var pauses = await _context.RequestPauses
                                       .AsNoTracking()
                                       .Where(p => p.Ip != null)
                                       .ToListAsync();

            var pause = pauses.FirstOrDefault(p => p.Ip.SequenceEqual(clientIpBytes));
            var timeRemaining = pause == null
                ? TimeSpan.Zero
                : TimeSpan.FromMinutes(2) - (DateTime.UtcNow - pause.StartedAt);
            var timeToDisplay = timeRemaining < TimeSpan.Zero ? TimeSpan.Zero : timeRemaining;
            
            return Ok(new {
                TimeRemaining = timeToDisplay == TimeSpan.Zero ? TimeOnly.MinValue :
                    new TimeOnly(timeToDisplay.Hours, timeToDisplay.Minutes, timeToDisplay.Seconds),
            });
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

    [HttpPost("pause")]
    public async Task<IActionResult> AddPause() {
        try {
            string? clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(clientIp)) {
                return BadRequest(new {
                    success = false,
                    error = new {
                        code = "InvalidRequest",
                        message = "Could not retrieve the IP address."
                    }
                });
            }

            // Convert the IP to a byte array
            byte[] clientIpBytes = System.Net.IPAddress.Parse(clientIp).GetAddressBytes();
            var pauses = _context.RequestPauses.AsTracking().Where(p => p.Ip != null).ToList();
            var pause = pauses.FirstOrDefault(p => p.Ip.SequenceEqual(clientIpBytes));

            if (pause != null) {
                pause.StartedAt = DateTime.UtcNow;
            } else {
                pause = new RequestPause {
                    StartedAt = DateTime.UtcNow,
                    Ip = clientIpBytes
                };
                _context.RequestPauses.Add(pause);
            }

            await _context.SaveChangesAsync();

            var timeToDisplay = TimeSpan.FromMinutes(2) - (DateTime.UtcNow - pause.StartedAt);
            return Ok(new {
                TimeRemaining = timeToDisplay == TimeSpan.Zero ? TimeOnly.MinValue :
                    new TimeOnly(timeToDisplay.Hours, timeToDisplay.Minutes, timeToDisplay.Seconds),
            });
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
}
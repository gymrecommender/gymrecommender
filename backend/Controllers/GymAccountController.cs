using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.Utilities;
using backend.ViewModels;
using backend.ViewModels.WorkingHour;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class GymAccountController : AccountControllerTemplate {
    public GymAccountController(GymrecommenderContext context, IOptions<AppSettings> appSettings) :
        base(context, appSettings) {
        _accountType = AccountType.gym;
    }

    [HttpGet("{username}")]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> GetGymByUsername(string username, AccountType? accountType) {
        return await base.GetByUsername(username, _accountType);
    }

    [HttpPut]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> UpdateAccount(AccountPutDto accountPutDto) {
        return await base.UpdateAccount(accountPutDto, _accountType);
    }

    [HttpDelete]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> DeleteAccount() {
        return await base.DeleteAccount(_accountType);
    }

    [HttpPost("login")]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> Login() {
        return await base.Login(_accountType);
    }

    [HttpDelete("logout")]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> Logout() {
        return await base.Logout(_accountType);
    }

    [HttpGet("owned")]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> GetOwnedGyms() {
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;
        
        var gyms = await _context.Gyms
                                 .Include(g => g.GymWorkingHours).ThenInclude(wh => wh.WorkingHours)
                                 .Include(g => g.Currency)
                                 .Include(g => g.City).ThenInclude(c => c.Country)
                                 .Include(g => g.OwnedByNavigation)
                                 .Where(g => g.OwnedByNavigation != null && g.OwnedByNavigation.OuterUid == firebaseUid)
                                 .Select(g => new GymViewModel {
                                     Id = g.Id,
                                     Name = g.Name,
                                     Longitude = g.Longitude,
                                     Latitude = g.Latitude,
                                     Address = g.Address,
                                     PhoneNumber = g.PhoneNumber,
                                     IsWheelchairAccessible = g.IsWheelchairAccessible,
                                     Website = g.Website,
                                     MonthlyMprice = g.MonthlyMprice,
                                     YearlyMprice = g.YearlyMprice,
                                     SixMonthsMprice = g.SixMonthsMprice,
                                     Currency = g.Currency.Name,
                                     City = g.City.Name,
                                     Country = g.City.Country.Name,
                                     WorkingHours = g.GymWorkingHours.Select(wh => new GymWorkingHoursViewModel {
                                         Weekday = wh.Weekday,
                                         OpenFrom = wh.WorkingHours.OpenFrom,
                                         OpenUntil = wh.WorkingHours.OpenUntil
                                     }).ToList()
                                 })
                                 .ToListAsync();
        return Ok(gyms);
    }


    [HttpPut("gym/{gymId}")]
    [Authorize(Policy = "GymOnly")]
    //TODO add working hours fields to the GymUpdateDto (it should be an array) ^
    //TODO save non-existing working hours ^
    //TODO connect new working hours with the gym in the GymWorkingHoursTable ^
    //TODO a GymViewModel should be returned ^
    public async Task<IActionResult> UpdateGym(Guid gymId, [FromBody] GymUpdateDto gymUpdateDto) {
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

        var gym = _context.Gyms.AsTracking()
                          .Include(g => g.OwnedByNavigation)
                          .Include(g => g.GymWorkingHours).ThenInclude(gwh => gwh.WorkingHours)
                          .FirstOrDefault(g =>
                              g.Id == gymId && g.OwnedByNavigation != null && g.OwnedByNavigation.OuterUid == firebaseUid);

        if (gym == null) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = ErrorMessage.ErrorMessages["OwnedGymError"]
                }
            });
        }

        if (gymUpdateDto.Name != null) gym.Name = gymUpdateDto.Name;
        if (gymUpdateDto.Address != null) gym.Address = gymUpdateDto.Address;
        if (gymUpdateDto.MonthlyMprice.HasValue) gym.MonthlyMprice = gymUpdateDto.MonthlyMprice.Value;
        if (gymUpdateDto.YearlyMprice.HasValue) gym.YearlyMprice = gymUpdateDto.YearlyMprice.Value;
        if (gymUpdateDto.SixMonthsMprice.HasValue) gym.SixMonthsMprice = gymUpdateDto.SixMonthsMprice.Value;
        if (gymUpdateDto.PhoneNumber != null) gym.PhoneNumber = gymUpdateDto.PhoneNumber;
        if (gymUpdateDto.Website != null) gym.Website = gymUpdateDto.Website;
        if (gymUpdateDto.IsWheelchairAccessible.HasValue)
            gym.IsWheelchairAccessible = gymUpdateDto.IsWheelchairAccessible.Value;

        if (gymUpdateDto.WorkingHours != null && gymUpdateDto.WorkingHours.Any()) {
            foreach (var workingHourDto in gymUpdateDto.WorkingHours) {
                var existingWorkingHour = await _context.WorkingHours
                                                        .FirstOrDefaultAsync(wh =>
                                                            wh.OpenFrom == workingHourDto.WorkingHours.OpenFrom &&
                                                            wh.OpenUntil == workingHourDto.WorkingHours.OpenUntil);

                if (existingWorkingHour == null) {
                    existingWorkingHour = new WorkingHour {
                        OpenFrom = workingHourDto.WorkingHours.OpenFrom,
                        OpenUntil = workingHourDto.WorkingHours.OpenUntil,
                    };
                    _context.WorkingHours.Add(existingWorkingHour);
                    await _context.SaveChangesAsync();
                }

                var existingRelation = gym.GymWorkingHours
                                          .FirstOrDefault(gwh =>
                                              gwh.Weekday == workingHourDto.Weekday &&
                                              gwh.WorkingHoursId == existingWorkingHour.Id);

                if (existingRelation == null) {
                    gym.GymWorkingHours.Add(new GymWorkingHour {
                        GymId = gym.Id,
                        WorkingHoursId = existingWorkingHour.Id,
                        Weekday = workingHourDto.Weekday
                    });
                }
            }
        }

        await _context.SaveChangesAsync();

        var updatedGym = new GymViewModel {
            Id = gym.Id,
            Name = gym.Name,
            Longitude = gym.Longitude,
            Latitude = gym.Latitude,
            Address = gym.Address,
            PhoneNumber = gym.PhoneNumber,
            IsWheelchairAccessible = gym.IsWheelchairAccessible,
            Website = gym.Website,
            MonthlyMprice = gym.MonthlyMprice,
            YearlyMprice = gym.YearlyMprice,
            SixMonthsMprice = gym.SixMonthsMprice,
            WorkingHours = gym.GymWorkingHours.Select(gwh => new GymWorkingHoursViewModel {
                Weekday = gwh.Weekday,
                OpenFrom = gwh.WorkingHours.OpenFrom,
                OpenUntil = gwh.WorkingHours.OpenUntil
            }).ToList()
        };
        return Ok(updatedGym);
    }

    [HttpGet("ownership")]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> GetRequests(string? type = null) {
        //TODO we need an id of the gym account ^
        //TODO add a get parameter that show whether we need to retrieve all requests, only answered or only unanswered ^
        //TODO retrieve requests only for the provided gym account ^
        //TODO Create a view model (not necessary, but desirable) to return the result ^
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

        if (type != null && type != "answered" && type != "unanswered") type = null;
        var query = _context.Ownerships.AsNoTracking()
                            .Include(o => o.RequestedByNavigation)
                            .Include(o => o.Gym)
                            .Where(o => o.RequestedByNavigation.OuterUid == firebaseUid);

        if (type == "answered") query = query.Where(o => o.Decision != null);
        else if (type == "unanswered") query = query.Where(o => o.Decision == null);

        var requests = query.Select(o => new {
            id = o.Id,
            requestedAt = o.RequestedAt,
            respondedAt = o.RespondedAt,
            decision = o.Decision.HasValue ? o.Decision.ToString() : null,
            message = o.Message,
            gym = new {
                id = o.Gym.Id,
                name = o.Gym.Name,
                address = o.Gym.Address,
                latitude = o.Gym.Latitude,
                longitude = o.Gym.Longitude
            }
        }).ToList();

        return Ok(requests);
    }

    [HttpPost("ownership/{gymId}")]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> AddOwnershipRequest(Guid gymId) {
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

        var gym = await _context.Gyms
                                .Include(g => g.OwnedByNavigation)
                                .FirstOrDefaultAsync(g => g.Id == gymId && g.OwnedBy == null);
        if (gym == null) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = ErrorMessage.ErrorMessages["ManagedGymError"]
                }
            });
        }

        var existingRequest = await _context.Ownerships
                                            .Include(o => o.RequestedByNavigation)
                                            .Where(o => o.GymId == gymId &&
                                                        o.RequestedByNavigation.OuterUid == firebaseUid &&
                                                        o.Decision == null)
                                            .FirstOrDefaultAsync();

        if (existingRequest != null) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = ErrorMessage.ErrorMessages["RequestPending"]
                }
            });
        }

        var user = _context.Accounts.First(a => a.OuterUid == firebaseUid);
        var newOwnershipRequest = new Ownership {
            GymId = gymId,
            RequestedByNavigation = user
        };

        _context.Ownerships.Add(newOwnershipRequest);
        await _context.SaveChangesAsync();

        var ownRequestWithGym = _context.Ownerships.AsNoTracking().Include(o => o.Gym)
                                        .First(o => o.Id == newOwnershipRequest.Id);

        return Ok(new {
            id = ownRequestWithGym.Id,
            requestedAt = ownRequestWithGym.RequestedAt,
            respondedAt = ownRequestWithGym.RespondedAt,
            decision = ownRequestWithGym.Decision?.ToString(),
            message = ownRequestWithGym.Message,
            gym = new {
                id = ownRequestWithGym.Gym.Id,
                name = ownRequestWithGym.Gym.Name,
                address = ownRequestWithGym.Gym.Address,
                latitude = ownRequestWithGym.Gym.Latitude,
                longitude = ownRequestWithGym.Gym.Longitude
            }
        });
    }


    [HttpPut("ownership/{gymId}")]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> DetachGym(Guid gymId) {
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

        var gym = _context.Gyms.AsTracking()
                                .Include(g => g.OwnedByNavigation)
                                .FirstOrDefault(g => g.Id == gymId && g.OwnedByNavigation != null && g.OwnedByNavigation.OuterUid == firebaseUid);
        if (gym == null) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = ErrorMessage.ErrorMessages["OwnedGymError"]
                }
            });
        }

        gym.OwnedBy = null;
        await _context.SaveChangesAsync();

        return StatusCode(204);
    }

    [HttpDelete("ownership/{requestId}")]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> DeleteOwnershipRequest(Guid requestId) {
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;
        
        var ownershipRequest = await _context.Ownerships.AsNoTracking()
                                             .Include(o => o.RequestedByNavigation)
                                             .FirstOrDefaultAsync(o =>
                                                 o.Id == requestId && o.RequestedByNavigation.OuterUid == firebaseUid);
        if (ownershipRequest == null) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = ErrorMessage.ErrorMessages["RequestError"]
                }
            });
        }

        _context.Ownerships.Remove(ownershipRequest);
        await _context.SaveChangesAsync();

        return StatusCode(204);
    }
}
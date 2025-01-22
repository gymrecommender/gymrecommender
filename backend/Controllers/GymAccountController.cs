using System.Text.Json;
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
    public GymAccountController(GymrecommenderContext context, HttpClient httpClient, IOptions<AppSettings> appSettings) :
        base(context, httpClient, appSettings) {
        _accountType = AccountType.gym;
    }

    [HttpPut]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> UpdateAccount(AccountPutDto accountPutDto) {
        return await base.UpdateAccount(accountPutDto, _accountType);
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
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var gyms = await _context.Gyms
                                     .Include(g => g.GymWorkingHours).ThenInclude(wh => wh.WorkingHours)
                                     .Include(g => g.Currency)
                                     .Include(g => g.City).ThenInclude(c => c.Country)
                                     .Include(g => g.OwnedByNavigation)
                                     .Where(g => g.OwnedByNavigation != null &&
                                                 g.OwnedByNavigation.OuterUid == firebaseUid)
                                     .ToListAsync();

            var gymsViewModels = gyms.Select(g => new GymViewModel {
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
                Currency = g.Currency.Code,
                City = g.City.Name,
                Country = g.City.Country.Name,
                CurrencyId = g.CurrencyId,
                CongestionRating = g.CongestionRating,
                Rating = (g.ExternalRatingNumber + g.InternalRatingNumber) > 0
                    ? (decimal)Math.Round(
                        ((double)g.ExternalRating * g.ExternalRatingNumber +
                         (double)g.InternalRating * g.InternalRatingNumber) /
                        (g.ExternalRatingNumber + g.InternalRatingNumber), 2)
                    : 0,
                WorkingHours = g.GymWorkingHours.Select(wh => new GymWorkingHoursViewModel {
                    Weekday = wh.Weekday,
                    OpenFrom = wh.WorkingHours.OpenFrom,
                    OpenUntil = wh.WorkingHours.OpenUntil
                }).ToList()
            }).ToList();
            
            return Ok(gymsViewModels);
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while retrieving owned gyms" }
            });
        }
    }


    [HttpPut("gym/{gymId}")]
    [Authorize(Policy = "GymOnly")]
    //TODO add working hours fields to the GymUpdateDto (it should be an array) ^
    //TODO save non-existing working hours ^
    //TODO connect new working hours with the gym in the GymWorkingHoursTable ^
    //TODO a GymViewModel should be returned ^
    public async Task<IActionResult> UpdateGym(Guid gymId, [FromBody] GymUpdateDto gymUpdateDto) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var gym = _context.Gyms.Include(x => x.OwnedByNavigation)
                              .Include(x => x.GymWorkingHours).ThenInclude(x => x.WorkingHours)
                              .Include(x => x.Currency)
                              .FirstOrDefault(x =>
                                  x.OwnedByNavigation != null && x.OwnedByNavigation.OuterUid == firebaseUid &&
                                  x.Id == gymId);

            if (gym == null) {
                return StatusCode(404, new {
                    success = false,
                    error = new {
                        message = "The gym has not been found or the user does not have the right to manage the gym"
                    }
                });
            }

            // Update properties
            var currency = _context.Currencies.FirstOrDefault(c => c.Code == gymUpdateDto.Currency);
            if (currency != null) gym.CurrencyId = currency.Id;

            if (gymUpdateDto.Name != null) gym.Name = gymUpdateDto.Name;
            if (gymUpdateDto.Address != null) gym.Address = gymUpdateDto.Address;
            if (gymUpdateDto.PhoneNumber != null) gym.PhoneNumber = gymUpdateDto.PhoneNumber;
            if (gymUpdateDto.Website != null) gym.Website = gymUpdateDto.Website;

            if (gymUpdateDto.MonthlyMprice.HasValue) gym.MonthlyMprice = gymUpdateDto.MonthlyMprice.Value;
            if (gymUpdateDto.YearlyMprice.HasValue) gym.YearlyMprice = gymUpdateDto.YearlyMprice.Value;
            if (gymUpdateDto.SixMonthsMprice.HasValue) gym.SixMonthsMprice = gymUpdateDto.SixMonthsMprice.Value;
            if (gymUpdateDto.IsWheelchairAccessible.HasValue)
                gym.IsWheelchairAccessible = gymUpdateDto.IsWheelchairAccessible.Value;

            // Update working hours
            if (gymUpdateDto.WorkingHours != null && gymUpdateDto.WorkingHours.Any()) {
                foreach (var workingHourDto in gymUpdateDto.WorkingHours) {
                    var existingWorkingHour = await _context.WorkingHours
                                                            .FirstOrDefaultAsync(wh =>
                                                                wh.OpenFrom == workingHourDto.OpenFrom &&
                                                                wh.OpenUntil == workingHourDto.OpenUntil);
                    
                    bool isDelete = workingHourDto.OpenFrom == null && workingHourDto.OpenUntil == null;
                    if ((workingHourDto.OpenFrom == null && workingHourDto.OpenUntil != null) ||
                        (workingHourDto.OpenFrom != null && workingHourDto.OpenUntil == null)) {
                        return BadRequest("Both OpenFrom and OpenUntil must be either null or have valid values");
                    }
                    
                    if (existingWorkingHour == null && !isDelete) {
                        existingWorkingHour = new WorkingHour {
                            OpenFrom = (TimeOnly)workingHourDto.OpenFrom,
                            OpenUntil = (TimeOnly)workingHourDto.OpenUntil,
                        };
                        _context.WorkingHours.Add(existingWorkingHour);
                        await _context.SaveChangesAsync();
                    }

                    var existingRelation = gym.GymWorkingHours
                                              .FirstOrDefault(gwh => gwh.Weekday == workingHourDto.Weekday);

                    bool isSame = existingRelation?.WorkingHours.OpenUntil == workingHourDto.OpenUntil &&
                                  existingRelation?.WorkingHours.OpenFrom == workingHourDto.OpenFrom;

                    if (isDelete) {
                        if (existingRelation != null) {
                            _context.GymWorkingHours.Remove(existingRelation);
                            await _context.SaveChangesAsync();
                        }
                    } else {
                        if (existingRelation == null) {
                            gym.GymWorkingHours.Add(new GymWorkingHour {
                                GymId = gym.Id,
                                WorkingHoursId = existingWorkingHour.Id,
                                Weekday = workingHourDto.Weekday
                            });
                        } else if (!isSame) {
                            existingRelation.WorkingHoursId = existingWorkingHour.Id;
                            existingRelation.ChangedAt = DateTime.UtcNow;
                            _context.GymWorkingHours.Update(existingRelation);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            
            var bookmarks = _context.Bookmarks.Where(b => b.GymId == gym.Id).Select(b => new {
                UserId = b.UserId,
            }).ToList();

            foreach (var bookmark in bookmarks) {
                _context.Notifications.Add(new Notification {
                    Message = $"The bookmarked gym '{gym.Name}' has been updated",
                    Type = NotificationType.message,
                    UserId = bookmark.UserId,
                });
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
                Currency = gym.Currency.Code,
                CurrencyId = gym.CurrencyId,
                CongestionRating = gym.CongestionRating,
                Rating = (gym.ExternalRatingNumber + gym.InternalRatingNumber) > 0
                    ? (decimal)Math.Round(
                        ((double)gym.ExternalRating * gym.ExternalRatingNumber +
                         (double)gym.InternalRating * gym.InternalRatingNumber) /
                        (gym.ExternalRatingNumber + gym.InternalRatingNumber), 2)
                    : 0,
                WorkingHours = gym.GymWorkingHours.Select(gwh => new GymWorkingHoursViewModel {
                    Weekday = gwh.Weekday,
                    OpenFrom = gwh.WorkingHours.OpenFrom,
                    OpenUntil = gwh.WorkingHours.OpenUntil
                }).ToList()
            };

            return Ok(updatedGym);
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while updating the gym" }
            });
        }
    }

    [HttpGet("ownership")]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> GetRequests(string? type = null) {
        //TODO we need an id of the gym account ^
        //TODO add a get parameter that show whether we need to retrieve all requests, only answered or only unanswered ^
        //TODO retrieve requests only for the provided gym account ^
        //TODO Create a view model (not necessary, but desirable) to return the result ^
        try {
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
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while getting ownership requests" }
            });
        }
    }

    [HttpPost("ownership/{gymId}")]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> AddOwnershipRequest(Guid gymId) {
        try {
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
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while adding ownership requests" }
            });
        }
    }


    [HttpPut("ownership/{gymId}")]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> DetachGym(Guid gymId) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var gym = _context.Gyms.AsTracking()
                              .Include(g => g.OwnedByNavigation)
                              .FirstOrDefault(g =>
                                  g.Id == gymId && g.OwnedByNavigation != null &&
                                  g.OwnedByNavigation.OuterUid == firebaseUid);
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
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while detaching the gym" }
            });
        }
    }

    [HttpDelete("ownership/{requestId}")]
    [Authorize(Policy = "GymOnly")]
    public async Task<IActionResult> DeleteOwnershipRequest(Guid requestId) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var ownershipRequest = await _context.Ownerships
                                                 .Include(o => o.RequestedByNavigation)
                                                 .FirstOrDefaultAsync(o =>
                                                     o.Id == requestId &&
                                                     o.RequestedByNavigation.OuterUid == firebaseUid);
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
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while deleting the ownership request" }
            });
        }
    }
}
using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.Services;
using backend.Utilities;
using backend.ViewModels;
using backend.ViewModels.WorkingHour;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UserAccountController : AccountControllerTemplate {
    private readonly RecommendationService _recommendationService;

    public UserAccountController(GymrecommenderContext context, IOptions<AppSettings> appSettings,
                                 RecommendationService recommendationService) :
        base(context, appSettings) {
        _accountType = AccountType.user;
        _recommendationService = recommendationService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SignUp(AccountDto accountDto, AccountType type, Guid? createdBy = null) {
        if (ModelState.IsValid) {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;
            if (firebaseUid == null) {
                return Forbid("Unauthorized user");
            }

            try {
                var errors = new Dictionary<string, string[]> { };
                if (!Enum.TryParse<ProviderType>(accountDto.Provider, out var provider)) {
                    errors["Provider"] = new[] { $"Provider {accountDto.Provider} is not supported" };
                } //not sure if this can be changed to a smarter way

                if (errors.Count > 0) {
                    return BadRequest(new {
                        success = false,
                        error = new {
                            code = "ValidationError",
                            message = "Some fields contain invalid data",
                            details = errors
                        }
                    });
                }

                var account = new Account {
                    Username = accountDto.Username,
                    Email = accountDto.Email,
                    FirstName = accountDto.FirstName,
                    LastName = accountDto.LastName,
                    Type = type,
                    Provider = provider,
                    OuterUid = accountDto.OuterUid,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(accountDto.Password),
                    IsEmailVerified = accountDto.IsEmailVerified,
                    CreatedBy = createdBy
                };
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                var role = account.Type.ToString();
                var response = new AuthResponse() {
                    Username = account.Username,
                    Email = account.Email,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    Role = role,
                };

                return Ok(response);
            } catch (Exception e) {
                return StatusCode(500, new {
                    success = false,
                    error = new {
                        code = "SignupError",
                        message = ErrorMessage.ErrorMessages["SignUpError"]
                    }
                });
            }
        }

        return BadRequest(new {
            success = false,
            error = new {
                code = "ValidationError",
                message = ErrorMessage.ErrorMessages["ValidationError"],
            }
        });
    }

    [HttpPut]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> UpdateAccount(AccountPutDto accountPutDto) {
        return await base.UpdateAccount(accountPutDto, _accountType);
    }

    [HttpDelete]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> DeleteAccount() {
        return await base.DeleteAccount(_accountType);
    }

    [HttpPost("login")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> Login() {
        return await base.Login(_accountType);
    }

    [HttpDelete("logout")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> Logout() {
        return await base.Logout(_accountType);
    }

    /// <summary>
    /// Retrieves all requests associated with a specific user.
    /// Accessible only by accounts with the User role.
    /// </summary>
    /// <returns>A list of RequestDto objects.</returns>
    [HttpGet("history")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GetRequestsHistory() {
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

        try {
            // Retrieve requests using the recommendation service
            var requests = await _recommendationService.GetRequestsByUsernameAsync(firebaseUid);

            return Ok(requests);
        } catch (KeyNotFoundException knfEx) {
            return NotFound(new { message = knfEx.Message });
        }
    }

    [HttpGet("requests")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GetRequests() {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var request = _context.Requests
                                  .Include(r => r.User)
                                  .Where(r => r.User.OuterUid == firebaseUid)
                                  .Select(r => new {
                                      Id = r.Id,
                                      RequestedAt = r.RequestedAt,
                                      Name = r.Name,
                                      Preferences = new {
                                          MinPrice = r.MinMembershipPrice,
                                          MinRating = r.MinRating,
                                          MinCongestion = r.MinCongestionRating,
                                          PriceTimeRatio = r.TotalCostPriority,
                                          MembershipLength = r.MembType.ToString(),
                                          DepartureTime = r.DepartureTime,
                                          ArrivalTime = r.ArrivalTime,
                                      }
                                  })
                                  .OrderByDescending(r => r.RequestedAt)
                                  .ToList();

            return Ok(request);
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while getting the requests" }
            });
        }
    }

    [HttpPut("requests/{requestId}")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> UpdateRequest(Guid requestId, UpdateRequestDto updateRequestDto) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            // Fetch the request by ID and ensure it belongs to the user
            var request = await _context.Requests
                                        .Include(r => r.User)
                                        .FirstOrDefaultAsync(r => r.Id == requestId && r.User.OuterUid == firebaseUid);
            if (request == null) {
                return NotFound(new {message = "Request has not been found or does not belong to the user."});
            }

            if (string.IsNullOrWhiteSpace(updateRequestDto.Name))
                return BadRequest("The 'Name' field can not be empty.");

            // Update only the Name field of the request
            request.Name = updateRequestDto.Name;
            await _context.SaveChangesAsync();

            return Ok(new {
                Id = request.Id,
                RequestedAt = request.RequestedAt,
                Name = request.Name,
                Preferences = new {
                    MinPrice = request.MinMembershipPrice,
                    MinRating = request.MinRating,
                    MinCongestion = request.MinCongestionRating,
                    PriceTimeRatio = request.TotalCostPriority,
                    MembershipLength = request.MembType.ToString(),
                    DepartureTime = request.DepartureTime,
                    ArrivalTime = request.ArrivalTime,
                }
            });
        } catch (Exception _) {
            return StatusCode(500, "An error occurred while updating the request.");
        }
    }

    [HttpGet("requests/{requestId:guid}/recommendations")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GetRecommendationRatingsByRequestId(Guid requestId) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;
            // Retrieve the UserId associated with the given requestId
            var request = await _context.Requests
                                        .Include(r => r.User)
                                        .Where(req => req.Id == requestId && req.User.OuterUid == firebaseUid)
                                        .FirstOrDefaultAsync();

            if (request == null) throw new Exception("The request has not been found or does not belong to the user.");

            var recommendations = _context.Recommendations
                                          .Include(r => r.Gym).ThenInclude(g => g.GymWorkingHours)
                                          .ThenInclude(gwh => gwh.WorkingHours)
                                          .Include(r => r.Request)
                                          .Where(r => r.RequestId == request.Id)
                                          .Select(r => new {
                                              Type = r.Type.ToString(),
                                              Gym = new GymViewModel {
                                                  Id = r.Gym.Id,
                                                  Name = r.Gym.Name,
                                                  Country = r.Gym.City.Country.Name,
                                                  City = r.Gym.City.Name,
                                                  Address = r.Gym.Address,
                                                  IsOwned = r.Gym.OwnedBy.HasValue,
                                                  Latitude = r.Gym.Latitude,
                                                  Longitude = r.Gym.Longitude,
                                                  IsWheelchairAccessible = r.Gym.IsWheelchairAccessible,
                                                  Currency = r.Gym.Currency.Code,
                                                  PhoneNumber = r.Gym.PhoneNumber,
                                                  MonthlyMprice = r.Gym.MonthlyMprice,
                                                  SixMonthsMprice = r.Gym.SixMonthsMprice,
                                                  YearlyMprice = r.Gym.YearlyMprice,
                                                  Website = r.Gym.Website,
                                                  CurrencyId = r.Gym.CurrencyId,
                                                  CongestionRating = r.Gym.CongestionRating,
                                                  Rating = (r.Gym.ExternalRatingNumber + r.Gym.InternalRatingNumber) > 0
                                                      ? (decimal)Math.Round(
                                                          ((double)r.Gym.ExternalRating * r.Gym.ExternalRatingNumber +
                                                           (double)r.Gym.InternalRating * r.Gym.InternalRatingNumber) /
                                                          (r.Gym.ExternalRatingNumber + r.Gym.InternalRatingNumber), 2)
                                                      : 0,
                                                  WorkingHours = r.Gym.GymWorkingHours.Select(w =>
                                                      new GymWorkingHoursViewModel {
                                                          Weekday = w.Weekday,
                                                          OpenFrom = w.WorkingHours.OpenFrom,
                                                          OpenUntil = w.WorkingHours.OpenUntil,
                                                      }).ToList()
                                              },
                                              OverallRating = r.TotalScore,
                                              TimeRating = r.TimeScore,
                                              CostRating = r.TcostScore,
                                              TravellingTime = r.Time,
                                              TotalCost = r.Tcost,
                                              CongestionRating = r.CongestionScore,
                                              RegularRating = r.RatingScore
                                          }).ToList()
                                          .GroupBy(r => r.Type)
                                          .ToDictionary(
                                              group => group.Key switch {
                                                  "main" => "mainRecommendations",
                                                  "alternative" => "additionalRecommendations",
                                              },
                                              group => group
                                                       .Select(r => new {
                                                           r.Gym,
                                                           r.OverallRating,
                                                           r.TimeRating,
                                                           r.CostRating,
                                                           r.TravellingTime,
                                                           r.TotalCost,
                                                           r.CongestionRating,
                                                           r.RegularRating
                                                       })
                                                       .OrderByDescending(r => r.OverallRating)
                                                       .ToList());

            if (recommendations.Count == 0) {
                return Ok(new {
                    mainRecommendations = new List<int>(),
                    additionalRecommendations = new List<int>(),
                });
            }

            return Ok(recommendations);
        } catch (Exception _) {
            return StatusCode(500, new { Message = "An error occurred while processing your request." });
        }
    }

    [HttpDelete("bookmarks/{bookmarkId}")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> DeleteBookmark(Guid bookmarkId) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var bookmark = _context.Bookmarks
                                        .Include(b => b.User)
                                        .FirstOrDefault(b => b.Id == bookmarkId && b.User.OuterUid == firebaseUid);
            if (bookmark == null) {
                return NotFound(new { message = "Bookmark has not been found or does not belong to the user." });
            }
            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();

            return StatusCode(204);
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while deleting the bookmark" }
            });
        }
    }

    [HttpPost("bookmarks")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> AddBookmark(BookmarkAddDto bookmarkAddDto) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var account = _context.Accounts.First(a => a.OuterUid == firebaseUid);
            var bookmark = new Bookmark {
                GymId = bookmarkAddDto.gymId,
                UserId = account.Id,
            };
            _context.Bookmarks.Add(bookmark);
            _context.SaveChanges();

            var gym = _context.Gyms
                              .Include(g => g.Currency)
                              .Include(g => g.City).ThenInclude(c => c.Country)
                              .Include(g => g.GymWorkingHours).ThenInclude(w => w.WorkingHours)
                              .First(g => g.Id == bookmark.GymId);

            return Ok(new {
                Gym = new {
                    Id = gym.Id,
                    Name = gym.Name,
                    City = gym.City.Name,
                    Country = gym.City.Country.Name,
                    Address = gym.Address,
                    Website = gym.Website,
                    MonthlyMprice = gym.MonthlyMprice,
                    YearlyMprice = gym.YearlyMprice,
                    IsWheelchairAccessible = gym.IsWheelchairAccessible,
                    SixMonthsMprice = gym.SixMonthsMprice,
                    Currency = gym.Currency.Code,
                    WorkingHours = gym.GymWorkingHours.Select(wh => new GymWorkingHoursViewModel {
                        Weekday = wh.Weekday,
                        OpenFrom = wh.WorkingHours.OpenFrom,
                        OpenUntil = wh.WorkingHours.OpenUntil
                    }).ToList()
                },
                Id = bookmark.Id,
                CreatedAt = bookmark.CreatedAt,
            });
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while adding the bookmark" }
            });
        }
    }

    [HttpGet("bookmarks")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GetBookmarks() {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var bookmarkedGyms = _context.Bookmarks
                                         .Include(b => b.User)
                                         .Where(b => b.User.OuterUid == firebaseUid)
                                         .Include(b => b.Gym).ThenInclude(g => g.Currency)
                                         .Select(b => new {
                                             Gym = new {
                                                 Id = b.Gym.Id,
                                                 Name = b.Gym.Name,
                                                 City = b.Gym.City.Name,
                                                 Country = b.Gym.City.Country.Name,
                                                 Address = b.Gym.Address,
                                                 Website = b.Gym.Website,
                                                 MonthlyMprice = b.Gym.MonthlyMprice,
                                                 YearlyMprice = b.Gym.YearlyMprice,
                                                 IsWheelchairAccessible = b.Gym.IsWheelchairAccessible,
                                                 SixMonthsMprice = b.Gym.SixMonthsMprice,
                                                 Currency = b.Gym.Currency.Code,
                                                 WorkingHours = b.Gym.GymWorkingHours.Select(wh =>
                                                     new GymWorkingHoursViewModel {
                                                         Weekday = wh.Weekday,
                                                         OpenFrom = wh.WorkingHours.OpenFrom,
                                                         OpenUntil = wh.WorkingHours.OpenUntil
                                                     }).ToList()
                                             },
                                             Id = b.Id,
                                             CreatedAt = b.CreatedAt,
                                         }).ToList();

            return Ok(bookmarkedGyms);
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while retrieving bookmarks" }
            });
        }
    }

    [HttpPost("ratings/{gymId}")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> PostRating(Guid gymId, [FromBody]RatingDto ratingDto) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var gym = _context.Gyms.AsTracking().FirstOrDefault(g => g.Id == gymId);
            if (gym == null) return NotFound(new { message = "The gym is not found" });
            
            var user = _context.Accounts.First(a => a.OuterUid == firebaseUid);
            _context.Ratings.Add(new Rating {
                UserId = user.Id,
                GymId = gym.Id,
                Rating1 = ratingDto.Rating
            });
            _context.CongestionRatings.Add(new CongestionRating {
                UserId = user.Id,
                GymId = gym.Id,
                AvgWaitingTime = ratingDto.WaitingTime,
                VisitTime = ratingDto.VisitTime,
                Crowdedness = ratingDto.Crowdedness,
            });
            _context.SaveChanges();

            //TODO not he best idea if two ratings are added for the same gym at the same time
            var ratings = _context.Ratings.Where(r => r.GymId == gymId);
            gym.InternalRating = (decimal)Math.Round(ratings.Average(r => r.Rating1), 2);
            gym.InternalRatingNumber = ratings.Count();
            
            var conRatings = _context.CongestionRatings.Where(r => r.GymId == gymId);
            gym.CongestionRating = (decimal)Math.Round(0.5 * conRatings.Average(cr => cr.AvgWaitingTime) + 0.5 * conRatings.Average(cr => cr.Crowdedness), 2);
            gym.CongestionRatingNumber = conRatings.Count();
            
            _context.SaveChanges();

            return Ok(new {
                Rating = Math.Round(
                    (gym.InternalRating * gym.InternalRatingNumber + gym.ExternalRating * gym.ExternalRatingNumber) /
                    (gym.ExternalRatingNumber + gym.InternalRatingNumber), 2),
                CongestionRating = gym.CongestionRating,
            });
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while saving ratings" }
            });
        }
    }

    [HttpGet("notifications")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GetNotifications() {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var notifications = _context.Notifications
                                        .Include(n => n.User)
                                        .Where(n => n.User.OuterUid == firebaseUid)
                                        .Select(n => new {
                                            Id = n.Id,
                                            Message = n.Message,
                                            IsRead = n.ReadAt.HasValue
                                        })
                                        .ToList();
            return Ok(notifications);
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while retrieving notifications" }
            });
        }
    }
    
    [HttpPut("notifications/{notificationId}")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> MarkNotificationRead(Guid notificationId) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var notification = _context.Notifications
                                        .Include(n => n.User)
                                        .FirstOrDefault(n => n.User.OuterUid == firebaseUid && n.Id == notificationId);
            
            if (notification == null) return NotFound(new { message = "The notification is not found" });
            notification.ReadAt = DateTime.UtcNow;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
            
            return Ok(new {
                Id = notification.Id,
                Message = notification.Message,
                IsRead = notification.ReadAt.HasValue
            });
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                error = new { message = "An error occurred while retrieving notifications" }
            });
        }
    }
}
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

    public UserAccountController(GymrecommenderContext context, HttpClient httpClient,
                                 IOptions<AppSettings> appSettings,
                                 RecommendationService recommendationService) :
        base(context, httpClient, appSettings) {
        _accountType = AccountType.user;
        _recommendationService = recommendationService;
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(AccountDto accountDto) {
        return await base.SignUp(accountDto, AccountType.user);
    }

    [HttpPut]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> UpdateAccount(AccountPutDto accountPutDto) {
        return await base.UpdateAccount(accountPutDto, _accountType);
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
                message = "An error occurred while getting the requests"
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
                return NotFound(new { message = "Request has not been found or does not belong to the user." });
            }

            if (string.IsNullOrWhiteSpace(updateRequestDto.Name))
                return BadRequest(new {message = "The 'Name' field can not be empty."});

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
            return StatusCode(500, new {message = "An error occurred while updating the request."});
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

            if (request == null) return BadRequest(new {mesage = "The request has not been found or does not belong to the user."});

            var recommendationsData = _context.Recommendations
                                              .Include(r => r.Gym).ThenInclude(g => g.GymWorkingHours)
                                              .ThenInclude(gwh => gwh.WorkingHours)
                                              .Include(r => r.Gym).ThenInclude(g => g.City).ThenInclude(c => c.Country)
                                              .Include(r => r.Gym).ThenInclude(g => g.Currency)
                                              .Include(r => r.Request)
                                              .Where(r => r.RequestId == request.Id)
                                              .AsSplitQuery()
                                              .ToList();

            var recommendations = recommendationsData
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
                                  })
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
                                               .ToList()
                                  );
            var result = new Dictionary<string, object> {
                ["requestId"] = request.Id,
                ["latitude"] = request.OriginLatitude,
                ["longitude"] = request.OriginLongitude,
            };

            foreach (var key in recommendations.Keys) result[key] = recommendations[key];

            return Ok(result);
        } catch (Exception _) {
            return StatusCode(500, new { message = "An error occurred while processing your request." });
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
                message = "An error occurred while deleting the bookmark"
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

            return Ok(new Dictionary<Guid, object> {
                {
                    gym.Id,
                    new {
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
                        CreatedAt = bookmark.CreatedAt
                    }
                }
            });
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                message = "An error occurred while adding the bookmark"
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
                                         }).ToDictionary(
                                             x => x.Gym.Id,
                                             x => new {
                                                 Id = x.Id,
                                                 Gym = x.Gym,
                                                 CreatedAt = x.CreatedAt,
                                             }
                                         );

            return Ok(bookmarkedGyms);
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                message = "An error occurred while retrieving bookmarks"
            });
        }
    }

    [HttpPost("ratings/{gymId}")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> PostRating(Guid gymId, [FromBody] RatingDto ratingDto) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var gym = _context.Gyms.AsTracking().FirstOrDefault(g => g.Id == gymId);
            if (gym == null) return NotFound(new { message = "The gym is not found" });

            var user = _context.Accounts.First(a => a.OuterUid == firebaseUid);
            var rating = new Rating {
                UserId = user.Id,
                GymId = gym.Id,
                Rating1 = ratingDto.Rating
            };
            _context.Ratings.Add(rating);

            var congestionRating = new CongestionRating {
                UserId = user.Id,
                GymId = gym.Id,
                AvgWaitingTime = ratingDto.WaitingTime,
                VisitTime = ratingDto.VisitTime,
                Crowdedness = ratingDto.Crowdedness,
            };
            _context.CongestionRatings.Add(congestionRating);
            _context.SaveChanges();

            //TODO not he best idea if two ratings are added for the same gym at the same time
            var ratings = _context.Ratings.Where(r => r.GymId == gymId);
            gym.InternalRating = (decimal)Math.Round(ratings.Average(r => r.Rating1), 2);
            gym.InternalRatingNumber = ratings.Count();

            var conRatings = _context.CongestionRatings.Where(r => r.GymId == gymId);
            gym.CongestionRating =
                (decimal)Math.Round(
                    0.5 * conRatings.Average(cr => cr.AvgWaitingTime) + 0.5 * conRatings.Average(cr => cr.Crowdedness),
                    2);
            gym.CongestionRatingNumber = conRatings.Count();

            _context.SaveChanges();

            return Ok(new Dictionary<Guid, object>() {
                {
                    rating.GymId,
                    new {
                        Rating = new {
                            Id = rating.Id,
                            Rating = rating.Rating1,
                        },
                        CongestionRating = new {
                            Id = congestionRating.Id,
                            Crowdedness = congestionRating.Crowdedness,
                            AvgWaitingTime = congestionRating.AvgWaitingTime,
                            VisitTime = congestionRating.VisitTime,
                        }
                    }
                }
            });
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                message = "An error occurred while saving ratings"
            });
        }
    }

    [HttpGet("ratings")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GetRatings() {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var ratings = _context.Ratings.Include(r => r.User)
                                  .Where(r => r.User.OuterUid == firebaseUid)
                                  .Select(r => new {
                                      Id = r.Id,
                                      GymId = r.GymId,
                                      Rating = r.Rating1,
                                  })
                                  .OrderBy(r => r.GymId)
                                  .ToList();
            var congestionRatings = _context.CongestionRatings.Include(r => r.User)
                                            .Where(r => r.User.OuterUid == firebaseUid)
                                            .Select(r => new {
                                                Id = r.Id,
                                                GymId = r.GymId,
                                                Crowdedness = r.Crowdedness,
                                                AvgWaitingTime = r.AvgWaitingTime,
                                                VisitTime = r.VisitTime,
                                            })
                                            .OrderBy(r => r.GymId)
                                            .ToList();

            var combinedRatings = ratings.Zip(congestionRatings, (rating, congestionRating) => new {
                GymId = rating.GymId,
                Rating = new {
                    Id = rating.Id,
                    Rating = rating.Rating
                },
                CongestionRating = new {
                    Id = congestionRating.Id,
                    Crowdedness = congestionRating.Crowdedness,
                    VisitTime = congestionRating.VisitTime,
                    AvgWaitingTime = congestionRating.AvgWaitingTime,
                }
            }).ToDictionary(
                x => x.GymId,
                x => new {
                    Rating = x.Rating,
                    CongestionRating = x.CongestionRating
                }
            );

            return Ok(combinedRatings);
        } catch (Exception _) {
            return StatusCode(500, new {
                success = false,
                message = "An error occurred while retrieving ratings"
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
                message = "An error occurred while retrieving notifications"
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
                message = "An error occurred while retrieving notifications"
            });
        }
    }

    [HttpGet("pause")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GetPauseByUserId() {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var pause = await _context.RequestPauses.AsNoTracking()
                                      .Include(p => p.User)
                                      .FirstOrDefaultAsync(p => p.User != null && p.User.OuterUid == firebaseUid);

            var timeRemaining = pause == null
                ? TimeSpan.Zero
                : TimeSpan.FromMinutes(2) - (DateTime.UtcNow - pause.StartedAt);
            var timeToDisplay = timeRemaining < TimeSpan.Zero ? TimeSpan.Zero : timeRemaining;
            
            return Ok(new {
                TimeRemaining = timeToDisplay == TimeSpan.Zero
                    ? TimeOnly.MinValue
                    : new TimeOnly(timeToDisplay.Hours, timeToDisplay.Minutes, timeToDisplay.Seconds),
            });
        } catch (Exception ex) {
            return StatusCode(500, new {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpPost("pause")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> AddPause() {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var user = _context.Accounts.First(a => a.OuterUid == firebaseUid);
            var pause = _context.RequestPauses.FirstOrDefault(p => p.UserId == user.Id);

            if (pause != null) {
                pause.StartedAt = DateTime.UtcNow;
            } else {
                pause = new RequestPause {
                    UserId = user.Id,
                    StartedAt = DateTime.UtcNow,
                };
                _context.RequestPauses.Add(pause);
            }

            await _context.SaveChangesAsync();

            var timeToDisplay = TimeSpan.FromMinutes(2) - (DateTime.UtcNow - pause.StartedAt);
            return Ok(new {
                TimeRemaining = timeToDisplay == TimeSpan.Zero
                    ? TimeOnly.MinValue
                    : new TimeOnly(timeToDisplay.Hours, timeToDisplay.Minutes, timeToDisplay.Seconds),
            });
        } catch (Exception ex) {
            return StatusCode(500, new {
                success = false,
                message = ex.Message
            });
        }
    }
}
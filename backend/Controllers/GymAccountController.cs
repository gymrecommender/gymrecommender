using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.Utilities;
using backend.ViewModels;
using backend.ViewModels.WorkingHour;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class GymAccountController : AccountControllerTemplate {
    private readonly GymrecommenderContext _context;

    public GymAccountController(GymrecommenderContext context, IOptions<AppSettings> appSettings) :
        base(context, appSettings) {
        _context = context;
        _accountType = AccountType.gym;
    }

    [HttpGet]
    public async Task<IActionResult> GetGymData(int page = 1, int sort = 1, bool ascending = true) {
        return await base.GetData(page, sort, ascending, _accountType);
    }

    [HttpPost]
    public async Task<IActionResult> SignUpGym(AccountDto accountDto) {
        return await SignUp(accountDto, _accountType);
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetGymByUsername(string username, AccountType? accountType) {
        return await base.GetByUsername(username, _accountType);
    }

    [HttpPut("{username}")]
    public async Task<IActionResult> UpdateByUsername(string username, AccountPutDto accountPutDto) {
        return await base.UpdateByUsername(username, accountPutDto, _accountType);
    }

    [HttpDelete("{username}")]
    public async Task<IActionResult> DeleteByUsername(string username) {
        return await base.DeleteByUsername(username, _accountType);
    }

    [HttpPost("{username}/login")]
    public async Task<IActionResult> Login(string username) {
        return await base.Login(username, _accountType);
    }

    [HttpDelete("{username}/logout")]
    public async Task<IActionResult> Logout(string username) {
        return await base.Logout(username, _accountType);
    }

    [HttpGet("owned/{gymId}")]
    public async Task<IActionResult> GetOwnedGyms(Guid gymId) {
        var gyms = await _context.Gyms
                                 .Include(g => g.GymWorkingHours).ThenInclude(wh => wh.WorkingHours)
                                 .Include(g => g.Currency)
                                 .Include(g => g.City).ThenInclude(c => c.Country)
                                 .Where(g => g.OwnedBy == gymId)
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


    [HttpPut("{gymAccountId}/{gymId}")]
    public async Task<IActionResult> UpdateGym(Guid gymAccountId, Guid gymId, [FromBody] GymUpdateDto gymUpdateDto) {
        var gym = await _context.Gyms.AsTracking().Where(g => g.Id == gymId && g.OwnedBy == gymAccountId).FirstOrDefaultAsync();
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
        if (gymUpdateDto.isWheelchairAccessible.HasValue) gym.IsWheelchairAccessible = gymUpdateDto.isWheelchairAccessible.Value;

        await _context.SaveChangesAsync();
        //TODO add working hours fields to the GymUpdateDto (it should be an array)
        //TODO save non-existing working hours
        //TODO connect new working hours with the gym in the GymWorkingHoursTable
        //TODO a GymViewModel should be returned
        return Ok(gym);
    }

    [HttpGet("unansweredRequests")]
    public async Task<IActionResult> GetUnansweredRequests() {
        //TODO we need an id of the gym account
        //TODO add a get parameter that show whether we need to retrieve all requests, only answered or only unanswered
        //TODO retrieve requests only for the provided gym account
        //TODO Create a view model (not necessary, but desirable) to return the result
        var requests = await _context.Ownerships
                                     .Where(o => o.RespondedAt == null)
                                     .Include(o => o.Gym)
                                     .Include(o => o.RequestedBy)
                                     .ToListAsync();

        var accountIds = requests.Select(o => o.Id).Distinct();
        var accounts = await _context.Accounts
                                     .Where(a => accountIds.Contains(a.Id))
                                     .ToDictionaryAsync(a => a.Id, a => a.Email); // Map AccountId -> Email


        var groupedRequests = requests
                              .GroupBy(o => o.Gym.Id)
                              .Select(g => new {
                                  GymId = g.Key,
                                  Name = g.First().Gym.Name,
                                  Address = g.First().Gym.Address,
                                  Requests = g.Select(r => new {
                                      AccountId = r.Id.ToString(),
                                      RequestTime = r.RequestedAt.ToString(),
                                      Email = accounts.ContainsKey(r.Id) ? accounts[r.Id] : null,
                                      Message = r.Message
                                  })
                              }).ToDictionary(
                                  g => g.GymId,
                                  g => new {
                                      name = g.Name,
                                      address = g.Address,
                                      requests = g.Requests.ToDictionary(
                                          r => r.AccountId,
                                          r => new {
                                              requestTime = r.RequestTime,
                                              email = r.Email,
                                              message = r.Message
                                          }
                                      )
                                  });


        return Ok(groupedRequests);
    }

    [HttpGet("ownership-requests")]
    public async Task<IActionResult> GetOwnershipRequests() {
        //TODO with the get parameter for the function above this function is not needed
        var ownershipRequests = await _context.Ownerships
                                              .Include(o => o.Gym)
                                              .ToListAsync();

        var response = ownershipRequests.Select(o => new {
            id = o.Id.ToString(),
            requestedAt = o.RequestedAt.ToString("o"),
            respondedAt = o.RespondedAt.HasValue ? o.RespondedAt.Value.ToString("o") : null,
            decision = o.Decision.ToString(),
            message = o.Message,
            gym = new {
                name = o.Gym.Name,
                address = o.Gym.Address,
                latitude = o.Gym.Latitude,
                longitude = o.Gym.Longitude
            }
        }).ToList();

        // Return the result as JSON
        return Ok(response);
    }
}

/*
RequestedAt { get; set; }

   public DateTime? RespondedAt
*/
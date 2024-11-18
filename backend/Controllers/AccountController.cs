using backend.DTO;
using backend.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using backend.Models;
using backend.ViewModels;
using Microsoft.EntityFrameworkCore.Query;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : Controller
{
    private readonly GymrecommenderContext context;
    private readonly AppSettings appData;

    public AccountController(GymrecommenderContext context, IOptions<AppSettings> appSettings)
    {
        this.context = context;
        appData = appSettings.Value;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appData.PageSize;
        var query = context.Accounts.AsNoTracking();
        int count = await query.CountAsync();

        var pagingInfo = new PagingInfo
        {
            CurrentPage = page,
            Sort = sort,
            Ascending = ascending,
            ItemsPerPage = pagesize,
            TotalItems = count
        };

        var accounts = await query
            .Select(p => new AccountViewModel
            {
                Id = p.Id,
                Username = p.Username,
                Email = p.Email,
                FirstName = p.FirstName,
                LastName = p.LastName,
                IsEmailVerified = p.IsEmailVerified,
                OuterUid = p.OuterUid,
                CreatedAt = p.CreatedAt,
                LastSignIn = p.LastSignIn,
                PasswordHash = p.PasswordHash,
                Type = p.Type.ToString(),
                Provider = p.Provider.ToString(),
                // TODO deal with the CreatedBy field
            })
            .OrderBy(b => b.Username)
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .AsSplitQuery()
            .ToListAsync();
        
        return Ok(new { accounts = accounts, paging = pagingInfo });
    }

    [HttpPost]
    public async Task<IActionResult> Post(AccountDto accountDto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var account = new Account
                {
                    Username = accountDto.Username,
                    Email = accountDto.Email,
                    FirstName = accountDto.FirstName,
                    LastName = accountDto.LastName,
                    Type = AccountType.user,
                    Provider = ProviderType.local,
                    OuterUid = accountDto.OuterUid,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(accountDto.Password),
                    IsEmailVerified = accountDto.IsEmailVerified
                };
                context.Add(account);
                await context.SaveChangesAsync();

                var result = new AccountViewModel
                {
                    Id = account.Id,
                    Username = account.Username,
                    Email = account.Email,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    IsEmailVerified = account.IsEmailVerified,
                    Type = account.Type.ToString(),
                    Provider = account.Provider.ToString(),
                    OuterUid = account.OuterUid,
                    CreatedAt = account.CreatedAt,
                    LastSignIn = account.LastSignIn,
                    PasswordHash = account.PasswordHash,
                };

                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = new
                    {
                        message = e.Message
                    }
                });
            }
        }
        
        return BadRequest(new
        {
            success = false,
            error = new
            {
                code = "ValidationError",
                message = "Invalid data",
                details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            }
        });
    }
}
using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Controllers;

public abstract class AccountControllerTemplate : Controller {
    protected AccountType _accountType;
    private readonly GymrecommenderContext context;
    private readonly AppSettings appData;

    public AccountControllerTemplate(GymrecommenderContext context, IOptions<AppSettings> appSettings) {
        this.context = context;
        appData = appSettings.Value;
    }

    public async Task<IActionResult> GetData(int page = 1, int sort = 1, bool ascending = true,
        AccountType? type = null) {
        int pagesize = appData.PageSize;
        var query = context.Accounts.AsNoTracking();
        if (type is not null) {
            query = query.Where(a => a.Type == type);
        }

        int count = await query.CountAsync();

        var pagingInfo = new PagingInfo {
            CurrentPage = page,
            Sort = sort,
            Ascending = ascending,
            ItemsPerPage = pagesize,
            TotalItems = count
        };

        var accounts = await query
            .Select(p => new AccountViewModel {
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
            })
            .OrderBy(b => b.Username)
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .AsSplitQuery()
            .ToListAsync();

        return Ok(new { data = accounts, paging = pagingInfo });
    }

    public async Task<IActionResult> GetByUsername(string username, AccountType? accountType = null) {
        var accountQuery = context.Accounts.AsNoTracking()
            .Where(a => a.Username == username);
        
        if (accountType is not null) {
            accountQuery = accountQuery.Where(a => a.Type == accountType);
        }

        var account = await accountQuery.FirstOrDefaultAsync();

        if (account == null) {
            return NotFound(new { error = $"User {username} is not found" });
        }

        return Ok(new AccountViewModel {
            Id = account.Id,
            Username = account.Username,
            Email = account.Email,
            FirstName = account.FirstName,
            LastName = account.LastName,
            IsEmailVerified = account.IsEmailVerified,
            OuterUid = account.OuterUid,
            CreatedAt = account.CreatedAt,
            LastSignIn = account.LastSignIn,
            PasswordHash = account.PasswordHash,
            Type = account.Type.ToString(),
            Provider = account.Provider.ToString(),
            // TODO deal with the CreatedBy field
        });
    }

    protected async Task<IActionResult> SignUp(AccountDto accountDto, AccountType type, Guid? createdBy = null) {
        if (ModelState.IsValid) {
            try {
                var errors = new Dictionary<string, string[]> { };
                if (!Enum.TryParse<ProviderType>(accountDto.Provider, out var provider)) {
                    errors["Provider"] = new[] { $"Provider {accountDto.Provider} is not supported" };
                }

                if (errors.Count > 0) {
                    return BadRequest(new {
                        success = false,
                        error = errors
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
                context.Add(account);
                await context.SaveChangesAsync();

                var result = new AccountViewModel {
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
            catch (Exception e) {
                return StatusCode(500, new {
                    success = false,
                    error = new {
                        message = e.Message
                    }
                });
            }
        }

        return BadRequest(new {
            success = false,
            error = new {
                code = "ValidationError",
                message = "Invalid data",
                details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            }
        });
    }
}
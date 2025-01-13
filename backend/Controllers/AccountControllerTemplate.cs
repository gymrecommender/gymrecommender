using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.Utilities;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Controllers;

[ApiController]
public abstract class AccountControllerTemplate : Controller {
    protected AccountType _accountType;
    protected readonly GymrecommenderContext context;
    protected readonly AppSettings appData;

    public AccountControllerTemplate(GymrecommenderContext context, IOptions<AppSettings> appSettings) {
        this.context = context;
        appData = appSettings.Value;
    }

    [NonAction]
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
                LastSignIn = p.LastSignIn,
                Type = p.Type.ToString(),
                Provider = p.Provider.ToString(),
            })
            .OrderBy(b => b.Username)
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .AsSplitQuery()
            .ToListAsync();

        return Ok(new { data = accounts, paging = pagingInfo });
    }//ok

    [NonAction]
    public async Task<IActionResult> GetByUsername(string username, AccountType? accountType = null) {
        var accountQuery = context.Accounts.AsNoTracking()
            .Where(a => a.Username == username);

        if (accountType is not null) {
            accountQuery = accountQuery.Where(a => a.Type == accountType);
        }

        var account = await accountQuery.FirstOrDefaultAsync();

        if (account == null) {
            return NotFound(new{ success = false, error = new
            {
                code = "UsernameError",
                message = ErrorMessage.ErrorMessages["UsernameError"]
            }});//new { error = $"User {username} is not found" }
        }

        return Ok(new AccountViewModel {
            Id = account.Id,
            Username = account.Username,
            Email = account.Email,
            FirstName = account.FirstName,
            LastName = account.LastName,
            IsEmailVerified = account.IsEmailVerified,
            LastSignIn = account.LastSignIn,
            Type = account.Type.ToString(),
            Provider = account.Provider.ToString(),
        });
    }//ok

    [NonAction]
    protected async Task<IActionResult> SignUp(AccountDto accountDto, AccountType type, Guid? createdBy = null) {
        if (ModelState.IsValid) {
            try {
                var errors = new Dictionary<string, string[]> { };
                if (!Enum.TryParse<ProviderType>(accountDto.Provider, out var provider)) {
                    errors["Provider"] = new[] { $"Provider {accountDto.Provider} is not supported" };
                }//not sure if this can be changed to a smarter way

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
                context.Accounts.Add(account);
                await context.SaveChangesAsync();
                
                var role = account.Type.ToString();
                var response = new AuthResponse()
                {
                    Username = account.Username,
                    Email = account.Email,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    Role = role,
                };
                
                return Ok(response);
            }
            catch (Exception e) {
                return StatusCode(500, new {
                    success = false,
                    error = new {
                        code = "SignupError",
                        message = ErrorMessage.ErrorMessages["SignUpError"]
                    }
                });
            }
        }

        var modelErrors = ModelState.Values.SelectMany(v => v.Errors)
            .Select(e => "Invalid field data")
            .Distinct()
            .ToArray();

        return BadRequest(new {
            success = false,
            error = new {
                code = "ValidationError",
                message = ErrorMessage.ErrorMessages["ValidationError"],
            }
        });
    }

    [NonAction]
    public async Task<IActionResult> UpdateByUsername(string username, AccountPutDto accountPutDto,
        AccountType? accountType = null) {
        if (ModelState.IsValid) {
            try {
                var accountQuery = context.Accounts.AsTracking()
                    .Where(a => a.Username == username);

                if (accountType is not null) {
                    accountQuery = accountQuery.Where(a => a.Type == accountType);
                }

                var account = await accountQuery.FirstOrDefaultAsync();

                if (account == null) {
                    return NotFound(new{ success = false, error = new
                    {
                        code = "UsernameError",
                        message = ErrorMessage.ErrorMessages["UsernameError"]
                    }});
                }


                if (!string.IsNullOrWhiteSpace(accountPutDto.Password)) {
                    account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(accountPutDto.Password);
                }

                account.FirstName = accountPutDto?.FirstName ?? account.FirstName;
                account.LastName = accountPutDto?.LastName ?? account.LastName;
                account.Username = accountPutDto?.Username ?? account.Username;
                account.OuterUid = accountPutDto?.OuterUid ?? account.OuterUid;
                account.IsEmailVerified = accountPutDto?.IsEmailVerified ?? account.IsEmailVerified;
                account.LastSignIn = accountPutDto?.LastSignIn ?? account.LastSignIn;
                account.Email = accountPutDto?.Email ?? account.Email;

                await context.SaveChangesAsync();

                return Ok(new AccountViewModel {
                    Id = account.Id,
                    Username = account.Username,
                    Email = account.Email,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    IsEmailVerified = account.IsEmailVerified,
                    LastSignIn = account.LastSignIn,
                    Type = account.Type.ToString(),
                    Provider = account.Provider.ToString()
                });
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
                message = ErrorMessage.ErrorMessages["ValidationError"],
                //details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            }
        });
    }

    [NonAction]
    public async Task<IActionResult> DeleteByUsername(string username, AccountType accountType) {
        var account = context.Accounts.AsTracking()
            .Where(a => a.Username == username)
            .Where(a => a.Type == accountType).FirstOrDefault();

        if (account == null) {
            return NotFound(new{ success = false, error = new
            {
                code = "UsernameError",
                message = ErrorMessage.ErrorMessages["UsernameError"]
            }});
        }

        context.Accounts.Remove(account);
        await context.SaveChangesAsync();

        return NoContent();
    }
    
    [NonAction]
    public async Task<IActionResult> GetRoleByUid(string uid) {
        try {
            var account = await context.Accounts.AsNoTracking()
                .Where(a => a.OuterUid == uid)
                .FirstOrDefaultAsync();

            if (account == null) {
                return NotFound(new{ success = false, error = new
                {
                    code = "UIDError",
                    message = ErrorMessage.ErrorMessages["TokenError"] + $"{uid}"//?
                }});
                //return NotFound(new { error = $"User with uid {uid} is not found" });
            }

            return Ok(new AccountRoleModel {
                Role = account.Type.ToString()
            });
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
    
    [NonAction]
    public async Task<IActionResult> Login(string username, AccountType accountType) {
        try {
            var account = await context.Accounts.AsTracking()
                .Where(a => a.Username == username)
                .Where(a => a.Type == accountType)
                .FirstOrDefaultAsync();

            if (account == null) {
                return NotFound(new{ success = false, error = new
                {
                    code = "UsernameError",
                    message = ErrorMessage.ErrorMessages["UsernameError"]
                }});
            }
            account.LastSignIn = DateTime.UtcNow;
            account.IsEmailVerified = true; //TODO this should be handled in a smarter way
            await context.SaveChangesAsync();
            
            var role = account.Type.ToString();
            var response = new AuthResponse()
            {
                Username = account.Username,
                Role = role,
                Email = account.Email,
                FirstName = account.FirstName,
                LastName = account.LastName,
            };

            return Ok(response);
        }
        catch (Exception e) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    code = "LoginError",
                    message = ErrorMessage.ErrorMessages["LoginError"],
                }
            });
        }
    }
    
    [NonAction]
    public async Task<IActionResult> Logout(string username, AccountType accountType) {
        try {
            var account = await context.Accounts.AsTracking()
                .Where(a => a.Username == username)
                .Where(a => a.Type == accountType)
                .FirstOrDefaultAsync();

            if (account == null) {
                return NotFound(new{ success = false, error = new
                {
                    code = "UsernameError",
                    message = ErrorMessage.ErrorMessages["UsernameError"]
                }});
            }

            return NoContent();
        }
        catch (Exception e) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    code = "LogoutError",
                    message = ErrorMessage.ErrorMessages["LogoutError"]
                }
            });
        }
    }
}
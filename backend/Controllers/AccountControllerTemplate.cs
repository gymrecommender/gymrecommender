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
    protected readonly GymrecommenderContext context;
    protected readonly AppSettings appData;

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
            .Select(p => new AccountRegularModel {
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

        return Ok(new AccountRegularModel {
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
                context.Accounts.Add(account);
                await context.SaveChangesAsync();

                var result = new AccountRegularModel {
                    Id = account.Id,
                    Username = account.Username,
                    Email = account.Email,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    IsEmailVerified = account.IsEmailVerified,
                    Type = account.Type.ToString(),
                    Provider = account.Provider.ToString(),
                    LastSignIn = account.LastSignIn,
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
                    return NotFound(new { success = false, error = $"User {username} is not found" });
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

                return Ok(new AccountRegularModel {
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
                message = "Invalid data",
                details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            }
        });
    }

    public async Task<IActionResult> DeleteByUsername(string username, AccountType accountType) {
        var account = context.Accounts.AsTracking()
            .Where(a => a.Username == username)
            .Where(a => a.Type == accountType).FirstOrDefault();

        if (account == null) {
            return NotFound(new { success = false, error = $"User {username} is not found" });
        }

        context.Accounts.Remove(account);
        await context.SaveChangesAsync();

        return NoContent();
    }

    public async Task<IActionResult> GetTokenByUsername(string username, AccountType accountType) {
        try {
            var account = await context.Accounts.AsNoTracking()
                .Where(a => a.Username == username)
                .Where(a => a.Type == accountType)
                .FirstOrDefaultAsync();

            if (account == null) {
                return NotFound(new { error = $"User {username} is not found" });
            }

            var token = await context.UserTokens.AsNoTracking()
                .Where(a => a.UserId == account.Id).FirstOrDefaultAsync();

            if (token == null) {
                return NotFound(new { error = $"The token for {username} is not found" });
            }

            return Ok(new UserTokenViewModel {
                Token = token.OuterToken
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
    
    public async Task<IActionResult> GetRoleByUid(string uid) {
        try {
            var account = await context.Accounts.AsNoTracking()
                .Where(a => a.OuterUid == uid)
                .FirstOrDefaultAsync();

            if (account == null) {
                return NotFound(new { error = $"User with uid {uid} is not found" });
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

    public async Task<IActionResult> UpdateTokenByUsername(string username, AccountTokenDto accountTokenDto,
        AccountType accountType) {
        if (ModelState.IsValid) {
            try {
                var account = await context.Accounts.AsNoTracking()
                    .Where(a => a.Username == username)
                    .Where(a => a.Type == accountType)
                    .FirstOrDefaultAsync();

                if (account == null) {
                    return NotFound(new { error = $"User {username} is not found" });
                }

                var token = await context.UserTokens.AsTracking()
                    .Where(a => a.UserId == account.Id).FirstOrDefaultAsync();

                if (token == null) {
                    return NotFound(new { error = $"The token for {username} is not found" });
                }

                token.OuterToken = accountTokenDto.Token;
                token.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return Ok(new UserTokenViewModel {
                    Token = token.OuterToken
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
                message = "Invalid data",
                details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            }
        });
    }

    public async Task<IActionResult> Login(string username, AccountTokenDto accountTokenDto, AccountType accountType) {
        try {
            var account = await context.Accounts.AsTracking()
                .Where(a => a.Username == username)
                .Where(a => a.Type == accountType)
                .FirstOrDefaultAsync();

            if (account == null) {
                return NotFound(new { error = $"User {username} is not found" });
            }
            account.LastSignIn = DateTime.UtcNow;
            account.IsEmailVerified = true; //TODO this should be handled in a smarter way
            
            var token = new UserToken {
                CreatedAt = DateTime.UtcNow,
                UserId = account.Id,
                OuterToken = accountTokenDto.Token
            };

            context.UserTokens.Add(token);
            await context.SaveChangesAsync();
            //TODO some login logic

            return Ok();
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
    
    public async Task<IActionResult> Logout(string username, AccountType accountType) {
        try {
            var account = await context.Accounts.AsTracking()
                .Where(a => a.Username == username)
                .Where(a => a.Type == accountType)
                .FirstOrDefaultAsync();

            if (account == null) {
                return NotFound(new { error = $"User {username} is not found" });
            }
            
            var token = await context.UserTokens.AsTracking()
                .Where(a => a.UserId == account.Id).FirstOrDefaultAsync();

            if (token == null) {
                return NotFound(new { error = $"The token for {username} is not found" });
            }

            context.UserTokens.Remove(token);
            await context.SaveChangesAsync();
            //TODO some logout logic

            return NoContent();
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
}
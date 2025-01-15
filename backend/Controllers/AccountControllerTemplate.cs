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
    protected readonly GymrecommenderContext _context;
    protected readonly AppSettings _appData;

    public AccountControllerTemplate(GymrecommenderContext context, IOptions<AppSettings> appSettings) {
        _context = context;
        _appData = appSettings.Value;
    }

    [NonAction]
    public async Task<IActionResult> GetByUsername(string username, AccountType? accountType = null) {
        var accountQuery = _context.Accounts.AsNoTracking()
                                   .Where(a => a.Username == username);

        if (accountType is not null) {
            accountQuery = accountQuery.Where(a => a.Type == accountType);
        }

        var account = await accountQuery.FirstOrDefaultAsync();

        if (account == null) {
            return NotFound(new {
                success = false, error = new {
                    code = "UsernameError",
                    message = ErrorMessage.ErrorMessages["UsernameError"]
                }
            }); //new { error = $"User {username} is not found" }
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
    }

    [NonAction]
    public async Task<IActionResult> UpdateAccount(AccountPutDto accountPutDto,
                                                   AccountType? accountType = null) {
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

        if (ModelState.IsValid) {
            try {
                var accountQuery = _context.Accounts.AsTracking()
                                           .Where(a => a.OuterUid == firebaseUid);

                if (accountType is not null) {
                    accountQuery = accountQuery.Where(a => a.Type == accountType);
                }

                var account = await accountQuery.FirstOrDefaultAsync();

                if (account == null) {
                    return NotFound(new {
                        success = false, error = new {
                            code = "UsernameError",
                            message = ErrorMessage.ErrorMessages["UsernameError"]
                        }
                    });
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

                await _context.SaveChangesAsync();

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
            } catch (Exception e) {
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
    public async Task<IActionResult> DeleteAccount(AccountType accountType) {
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

        var account = _context.Accounts.AsTracking()
                              .Where(a => a.OuterUid == firebaseUid)
                              .Where(a => a.Type == accountType).FirstOrDefault();

        if (account == null) {
            return NotFound(new {
                success = false, error = new {
                    code = "UsernameError",
                    message = ErrorMessage.ErrorMessages["UsernameError"]
                }
            });
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [NonAction]
    public async Task<IActionResult> GetRoleByUid(string uid) {
        try {
            var account = await _context.Accounts.AsNoTracking()
                                        .Where(a => a.OuterUid == uid)
                                        .FirstOrDefaultAsync();

            if (account == null) {
                return NotFound(new {
                    success = false, error = new {
                        code = "UIDError",
                        message = ErrorMessage.ErrorMessages["TokenError"] + $"{uid}" //?
                    }
                });
                //return NotFound(new { error = $"User with uid {uid} is not found" });
            }

            return Ok(new AccountRoleModel {
                Role = account.Type.ToString()
            });
        } catch (Exception e) {
            return StatusCode(500, new {
                success = false,
                error = new {
                    message = e.Message
                }
            });
        }
    }

    [NonAction]
    public async Task<IActionResult> Login(AccountType accountType) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var account = await _context.Accounts.AsTracking()
                                        .Where(a => a.OuterUid == firebaseUid)
                                        .Where(a => a.Type == accountType)
                                        .FirstOrDefaultAsync();

            if (account == null) {
                return NotFound(new {
                    success = false, error = new {
                        code = "UsernameError",
                        message = ErrorMessage.ErrorMessages["UsernameError"]
                    }
                });
            }

            account.LastSignIn = DateTime.UtcNow;
            account.IsEmailVerified = true; //TODO this should be handled in a smarter way
            await _context.SaveChangesAsync();

            var role = account.Type.ToString();
            var response = new AuthResponse() {
                Username = account.Username,
                Role = role,
                Email = account.Email,
                FirstName = account.FirstName,
                LastName = account.LastName,
            };

            return Ok(response);
        } catch (Exception e) {
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
    public async Task<IActionResult> Logout(AccountType accountType) {
        try {
            var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;

            var account = await _context.Accounts.AsTracking()
                                        .Where(a => a.OuterUid == firebaseUid)
                                        .Where(a => a.Type == accountType)
                                        .FirstOrDefaultAsync();

            if (account == null) {
                return NotFound(new {
                    success = false, error = new {
                        code = "UsernameError",
                        message = ErrorMessage.ErrorMessages["UsernameError"]
                    }
                });
            }

            return NoContent();
        } catch (Exception e) {
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
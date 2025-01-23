using System.Text;
using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.Utilities;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace backend.Controllers;

[ApiController]
public abstract class AccountControllerTemplate : Controller {
    protected AccountType _accountType;
    protected readonly GymrecommenderContext _context;
    protected readonly AppSettings _appData;
    protected readonly HttpClient _httpClient;

    public AccountControllerTemplate(GymrecommenderContext context, HttpClient httpClient,
                                     IOptions<AppSettings> appSettings) {
        _context = context;
        _appData = appSettings.Value;
        _httpClient = httpClient;
    }

    [NonAction]
    protected async Task<IActionResult> SignUp(AccountDto accountDto, AccountType type, Guid? createdBy = null) {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        if (ModelState.IsValid) {
            try {
                var errors = new Dictionary<string, string[]> { };
                if (!Enum.TryParse<ProviderType>(accountDto.Provider, out var provider)) {
                    errors["Provider"] = new[] { $"Provider {accountDto.Provider} is not supported" };
                } //not sure if this can be changed to a smarter way

                if (errors.Count > 0) {
                    return BadRequest(new {
                        message = "Some fields contain invalid data"
                    });
                }

                var firebaseApiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY")
                                     ?? throw new InvalidOperationException("FIREBASE_API_KEY not set.");
                var firebaseApiUrl =
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={firebaseApiKey}";

                // Prepare request payload for Firebase sign-up
                var payload = new {
                    email = accountDto.Email,
                    password = accountDto.Password,
                    returnSecureToken = true
                };
                
                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                
                var firebaseResponse = await _httpClient.PostAsync(firebaseApiUrl, content);

                if (!firebaseResponse.IsSuccessStatusCode) {
                    var errorResponse = await firebaseResponse.Content.ReadAsStringAsync();
                    return StatusCode(500, new {
                        success = false,
                        message = "Error creating Firebase user"
                    });
                }
                var responseContent = await firebaseResponse.Content.ReadAsStringAsync();
                var signUpResponse = JsonConvert.DeserializeObject<FirebaseSignUpResponse>(responseContent);

                var firebaseUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={firebaseApiKey}";
                var requestBody = new
                {
                    requestType = "VERIFY_EMAIL",
                    email = accountDto.Email,
                    idToken = signUpResponse.idToken
                };

                var contentEmail = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var responseEmail = await _httpClient.PostAsync(firebaseUrl, contentEmail);

                if (!responseEmail.IsSuccessStatusCode)
                {
                    var errorContent = await responseEmail.Content.ReadAsStringAsync();
                    return StatusCode(500, new {
                        success = false,
                        message = "Error creating Firebase user"
                    });
                }
                
                var account = new Account {
                    Username = accountDto.Username,
                    Email = accountDto.Email,
                    FirstName = accountDto.FirstName,
                    LastName = accountDto.LastName,
                    Type = type,
                    Provider = provider,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(accountDto.Password),
                    IsEmailVerified = false,
                    CreatedBy = createdBy,
                    OuterUid = signUpResponse.localId
                };
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                
                var role = account.Type.ToString();
                var response = new AuthResponse() {
                    Username = account.Username,
                    Email = account.Email,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    Role = role,
                };
                return Ok(response);
            } catch (Exception _) {
                await transaction.RollbackAsync();
                
                return StatusCode(500, new {
                    success = false,
                    message = ErrorMessage.ErrorMessages["SignUpError"]
                });
            }
        }

        return BadRequest(new {
            message = ErrorMessage.ErrorMessages["ValidationError"]
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
                        message = ErrorMessage.ErrorMessages["UsernameError"]
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
                    message = e.Message
                });
            }
        }

        return BadRequest(new {
            message = ErrorMessage.ErrorMessages["ValidationError"]
        });
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
                    message = ErrorMessage.ErrorMessages["UsernameError"]
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
                message = ErrorMessage.ErrorMessages["LoginError"]
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
                    message = ErrorMessage.ErrorMessages["UsernameError"]
                });
            }

            return NoContent();
        } catch (Exception e) {
            return StatusCode(500, new {
                success = false,
                message = ErrorMessage.ErrorMessages["LogoutError"]
            });
        }
    }
}
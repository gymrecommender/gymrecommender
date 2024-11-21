using backend.DTO;
using backend.Enums;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UserAccountController : AccountControllerTemplate {
    public UserAccountController(GymrecommenderContext context, IOptions<AppSettings> appSettings) :
        base(context, appSettings) {
        _accountType = AccountType.user;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserData(int page = 1, int sort = 1, bool ascending = true) {
        return await base.GetData(page, sort, ascending, _accountType);
    }
    
    [HttpPost]
    public async Task<IActionResult> SignUpUser(AccountDto accountDto) {
        return await SignUp(accountDto, _accountType);
    }
    
    [HttpGet("{username}")]
    public async Task<IActionResult> GetUserByUsername(string username, AccountType? accountType) {
        return await base.GetByUsername(username, _accountType);
    }
}
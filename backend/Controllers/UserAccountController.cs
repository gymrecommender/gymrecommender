using backend.DTO;
using backend.Enums;
using backend.Models;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<IActionResult> GetData(int page = 1, int sort = 1, bool ascending = true) {
        return await base.GetData(page, sort, ascending, _accountType);
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(AccountDto accountDto) {
        return await SignUp(accountDto, _accountType);
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetByUsername(string username, AccountType? accountType) {
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

    [HttpGet("{username}/token")]
    public async Task<IActionResult> GetTokenByUsername(string username) {
        return await base.GetTokenByUsername(username, _accountType);
    }

    [HttpPut("{username}/token")]
    public async Task<IActionResult> UpdateTokenByUsername(string username, AccountTokenDto accountTokenDto) {
        return await base.UpdateTokenByUsername(username, accountTokenDto, _accountType);
    }

    [HttpPost("{username}/login")]
    public async Task<IActionResult> Login(string username, AccountTokenDto logInDto) {
        return await base.Login(username, logInDto, _accountType);
    }
    
    [HttpDelete("{username}/logout")]
    public async Task<IActionResult> Logout(string username) {
        return await base.Logout(username, _accountType);
    }
}
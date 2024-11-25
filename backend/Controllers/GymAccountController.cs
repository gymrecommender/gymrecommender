using backend.DTO;
using backend.Enums;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class GymAccountController : AccountControllerTemplate {
    public GymAccountController(GymrecommenderContext context, IOptions<AppSettings> appSettings) :
        base(context, appSettings) {
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
    
    [HttpGet("{username}/token")]
    public async Task<IActionResult> GetTokenByUsername(string username) {
        return await base.GetTokenByUsername(username, _accountType);
    }

    [HttpPost("{username}/token")]
    public async Task<IActionResult> SaveTokenByUsername(string username, AccountTokenDto accountTokenDto) {
        return await base.SaveTokenByUsername(username, accountTokenDto, _accountType);
    }

    [HttpDelete("{username}/token")]
    public async Task<IActionResult> DeleteTokenByUsername(string username) {
        return await base.DeleteTokenByUsername(username, _accountType);
    }

    [HttpPut("{username}/token")]
    public async Task<IActionResult> UpdateTokenByUsername(string username, AccountTokenDto accountTokenDto) {
        return await base.UpdateTokenByUsername(username, accountTokenDto, _accountType);
    }
}
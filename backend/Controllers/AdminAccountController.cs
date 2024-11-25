using backend.DTO;
using backend.Enums;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AdminAccountController : AccountControllerTemplate {
    public AdminAccountController(GymrecommenderContext context, IOptions<AppSettings> appSettings) :
        base(context, appSettings) {
        _accountType = AccountType.admin;
    }

    [HttpGet]
    public async Task<IActionResult> GetAdminData(int page = 1, int sort = 1, bool ascending = true) {
        return await base.GetData(page, sort, ascending, _accountType);
    }
    
    [HttpPost]
    public async Task<IActionResult> SignUpAdmin(AccountDto accountDto) {
        return await SignUp(accountDto, _accountType);
    }
    
    [HttpGet("{username}")]
    public async Task<IActionResult> GetAdminByUsername(string username) {
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
}
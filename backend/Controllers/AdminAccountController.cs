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

    [HttpGet]// GET /api/adminaccount
    public async Task<IActionResult> GetAdminData(int page = 1, int sort = 1, bool ascending = true) {
        return await base.GetData(page, sort, ascending, _accountType);
    }
    
    [HttpPost]// POST /api/adminaccount
    public async Task<IActionResult> SignUpAdmin(AccountDto accountDto) {
        return await SignUp(accountDto, _accountType);
    }
    
    [HttpGet("{username}")] // GET /api/adminaccount/{username}
    public async Task<IActionResult> GetAdminByUsername(string username) {
        return await base.GetByUsername(username, _accountType);
    }
    
    [HttpPut("{username}")] // PUT /api/adminaccount/{username}
    public async Task<IActionResult> UpdateByUsername(string username, AccountPutDto accountPutDto) {
        return await base.UpdateByUsername(username, accountPutDto, _accountType);
    }
    
    [HttpDelete("{username}")] // DELETE /api/adminaccount/{username}
    public async Task<IActionResult> DeleteByUsername(string username) {
        return await base.DeleteByUsername(username, _accountType);
    }
    
    [HttpPost("{username}/login")]
    public async Task<IActionResult> Login(string username) {
        return await base.Login(username, _accountType);
    }
    
    [HttpDelete("{username}/logout")] // DELETE /api/adminaccount/{username}/logout
    public async Task<IActionResult> Logout(string username) {
        return await base.Logout(username, _accountType);
    }
}
using backend.DTO;
using backend.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using backend.Models;
using backend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Query;

namespace backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AccountController : AccountControllerTemplate {
    public AccountController(GymrecommenderContext context, IOptions<AppSettings> appSettings) : base(context, appSettings) {}
    
    [HttpGet] // GET /api/account
    public async Task<IActionResult> GetAccounts(int page = 1, int sort = 1, bool ascending = true) {
        return await base.GetData(page, sort, ascending);
    }
    
    [HttpGet("{username}")]// GET /api/account/{username}
    public async Task<IActionResult> GetByUsername(string username) {
        return await base.GetByUsername(username);
    }

    [HttpGet("{uid}/role")]// GET /api/account/{uid}/role
    public async Task<IActionResult> GetRoleByUid(string uid) {
        return await base.GetRoleByUid(uid);
    }
}
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
    
    [HttpGet("{username}")]// GET /api/account/{username}
    public async Task<IActionResult> GetByUsername(string username) {
        return await base.GetByUsername(username);
    }

    [HttpGet("role")]
    [Authorize]
    public async Task<IActionResult> GetRole() {
        return await base.GetRoleByUid();
    }
}
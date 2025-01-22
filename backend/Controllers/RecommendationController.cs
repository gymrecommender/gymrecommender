using backend.DTO;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationController : Controller {
    private readonly RecommendationService _recommendationService;
    private readonly GymrecommenderContext _context;

    public RecommendationController(GymrecommenderContext context, RecommendationService recommendationService) {
        _recommendationService = recommendationService;
        _context = context;
    }

    /// <summary>
    /// Endpoint to get gym recommendations based on user preferences and filters.
    /// </summary>
    /// <param name="request">User's gym recommendation request containing filters and preferences.</param>
    /// <returns>List of recommended gyms with normalized scores and final scores.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateRecommendationsRequest([FromBody] GymRecommendationRequestDto request) {
        if (request == null) {
            return BadRequest("Invalid request data.");
        }
        
        var firebaseUid = HttpContext.User.FindFirst("user_id")?.Value;
        Account? account = null;
        if (firebaseUid != null) {
            account = _context.Accounts.AsNoTracking().First(a => a.OuterUid == firebaseUid);
        }

        // Call the service to get recommendations
        var recommendations = await _recommendationService.GetRecommendations(request, account);

        // Assuming GetRecommendations returns IActionResult
        return recommendations;
    }
}
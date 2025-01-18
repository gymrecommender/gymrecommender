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
        var recommendations = _recommendationService.GetRecommendations(request, account);

        // Assuming GetRecommendations returns IActionResult
        return await recommendations;
    }

[HttpGet("{requestId:guid}/ratings")]
public async Task<IActionResult> GetRecommendationRatingsByRequestId(Guid requestId)
{
    try
    {
        var ratings = await _recommendationService.GetRatingsByRequestIdAsync(requestId);
        if (ratings == null || ratings.Count == 0)
        {
            return NotFound(new { Message = "No ratings found for the given request ID." });
        }

        return Ok(ratings);
    }
    catch (Exception)
    {
        return StatusCode(500, new { Message = "An error occurred while processing your request." });
    }
}


}
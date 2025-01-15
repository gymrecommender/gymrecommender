using backend.DTO;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecomendationController : Controller
{
    private readonly RecommendationService _recommendationService;

    public RecomendationController(RecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    /// <summary>
    /// Endpoint to get gym recommendations based on user preferences and filters.
    /// </summary>
    /// <param name="request">User's gym recommendation request containing filters and preferences.</param>
    /// <returns>List of recommended gyms with normalized scores and final scores.</returns>
    [HttpPost("recommendations")]
    public async Task<IActionResult> CreateRecommendationsRequest([FromBody] GymRecommendationRequestDto request)
    {
        if (request == null)
        {
            return BadRequest("Invalid request data.");
        }

        // Call the service to get recommendations
        var recommendations = _recommendationService.GetRecomendations(request);

        // Assuming GetRecomendations returns IActionResult
        return await recommendations;
    }
}
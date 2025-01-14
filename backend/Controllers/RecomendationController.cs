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
    [Authorize(Policy = "UserOnly")]
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

    /// <summary>
    /// Retrieves all requests associated with a specific user email.
    /// Accessible only by users with the Admin role.
    /// </summary>
    /// <param name="username">The username of the user whose requests are to be retrieved.</param>
    /// <returns>A list of RequestDto objects.</returns>
    [HttpGet("getRequestsHistory/{username}")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GetRequestsHistory(string username)
    {
        try
        {
            // Retrieve requests using the recommendation service
            var requests = await _recommendationService.GetRequestsByUsernameAsync(username);

            return Ok(requests);
        }
        catch (KeyNotFoundException knfEx)
        {
            return NotFound(new { message = knfEx.Message });
        }
    }
}
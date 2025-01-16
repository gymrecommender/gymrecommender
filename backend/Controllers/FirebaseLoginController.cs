using System.Text;
using System.Text.Json;
using backend.DTO;
using DotNetEnv;

namespace backend.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/[controller]")]
public class FirebaseLoginController : Controller {
    private readonly HttpClient _httpClient;

    public FirebaseLoginController(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    [HttpPost("firebase/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request) {
        Env.Load();
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) {
            return BadRequest("Email and Password are required.");
        }

        // Replace this with your Firebase Web API key
        var firebaseApiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY")
                             ?? throw new InvalidOperationException("FIREBASE_API_KEY not set.");
        var firebaseUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={firebaseApiKey}";

        var payload = new {
            email = request.Email,
            password = request.Password,
            returnSecureToken = true
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(firebaseUrl, content);

        if (!response.IsSuccessStatusCode) {
            return Unauthorized("Invalid email or password.");
        }

        var responseData = await JsonSerializer.DeserializeAsync<FirebaseTokenResponse>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return Ok(responseData);
    }
}
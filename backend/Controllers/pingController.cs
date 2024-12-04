using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("ping")]

public class pingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("backend is alive");
    }
}
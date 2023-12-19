using front_end.Udp;
using Microsoft.AspNetCore.Mvc;

namespace front_end.Controllers.api;

[ApiController]
[Route("api/[controller]")]
public class PlayController(ILogger<PlayController> logger, PlayerClient playerClient) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Play()
    {
        var connectionString = await playerClient.SendConnectRequest();
        logger.LogInformation("Recieved connection string {}", connectionString);
        return Ok(connectionString);
    }
}

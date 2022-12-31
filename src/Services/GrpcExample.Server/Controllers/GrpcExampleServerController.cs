using Microsoft.AspNetCore.Mvc;

namespace GrpcExample.Server.Controllers;
[ApiController]
[Route("[controller]")]
public class GrpcExampleServerController : ControllerBase
{
    private readonly ILogger<GrpcExampleServerController> _logger;

    public GrpcExampleServerController(ILogger<GrpcExampleServerController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetGrpcExampleType")]
    public async Task<string> Get()
    {
        _logger.LogInformation("Server received a GET request");

        return await Task.FromResult("server").ConfigureAwait(false);
    }
}

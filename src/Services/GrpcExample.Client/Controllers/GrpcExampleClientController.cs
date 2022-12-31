using Microsoft.AspNetCore.Mvc;

namespace GrpcExample.Client.Controllers;
[ApiController]
[Route("[controller]")]
public class GrpcExampleClientController : ControllerBase
{
    private readonly ILogger<GrpcExampleClientController> _logger;

    public GrpcExampleClientController(ILogger<GrpcExampleClientController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetGrpcExampleType")]
    public async Task<string> Get()
    {
        _logger.LogInformation("Client received a GET request");

        return await Task.FromResult("client").ConfigureAwait(false);
    }
}

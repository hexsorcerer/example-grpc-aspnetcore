using Microsoft.AspNetCore.Mvc;

namespace GrpcExample.Client.Controllers;
[ApiController]
[Route("[controller]")]
public class GrpcExampleClientController : ControllerBase
{
    private readonly GrpcExample.GrpcExampleClient _grpcClient;
    private readonly ILogger<GrpcExampleClientController> _logger;

    public GrpcExampleClientController(
        GrpcExample.GrpcExampleClient grpcClient,
        ILogger<GrpcExampleClientController> logger)
    {
        _grpcClient = grpcClient ?? throw new ArgumentNullException(nameof(grpcClient));
        _logger = logger;
    }

    [HttpGet(Name = "GetGrpcExampleType")]
    public async Task<GrpcExampleResponse> Get()
    {
        _logger.LogInformation("Client received a GET request");

        return await _grpcClient.GetGrpcExampleResponseAsync(new GrpcExampleRequest
        {
            Message = "yo this is the grpc client"
        });
    }
}

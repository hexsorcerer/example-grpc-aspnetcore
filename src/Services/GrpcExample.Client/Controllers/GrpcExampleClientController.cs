using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace GrpcExample.Client.Controllers;
[ApiController]
[Route("[controller]")]
public class GrpcExampleClientController : ControllerBase
{
    private const decimal NanoScaleFactor = 1_000_000_000;

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

        var response = await _grpcClient.GetGrpcExampleResponseAsync(new GrpcExampleRequest
        {
            Message = "yo this is the grpc client"
        });

        // just an example to show that you can easily do this
        _ = response.Modified.ToDateTime();

        var amount = response.Amount.Units + response.Amount.Nanos / NanoScaleFactor;
        _logger.LogInformation("Amount was about {Amount}", amount.ToString("C", CultureInfo.CreateSpecificCulture("en-US")));

        return response;
    }
}

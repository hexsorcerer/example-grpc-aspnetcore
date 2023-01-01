using Google.Protobuf.WellKnownTypes;
using Google.Type;
using Grpc.Core;

namespace GrpcExample.Server.Grpc;

public class GrpcExampleService : GrpcExample.GrpcExampleBase
{
    private const decimal NanoFactor = 1_000_000_000;

    public override async Task<GrpcExampleResponse> GetGrpcExampleResponse(
        GrpcExampleRequest request,
        ServerCallContext context)
    {
        const decimal value = 3.50m;
        var units = decimal.ToInt64(value);
        var nanos = decimal.ToInt32((value - units) * NanoFactor);
        return await Task.FromResult(new GrpcExampleResponse
        {
            Message = "yo this is the grpc server",
            Modified = Timestamp.FromDateTime(DateTime.UtcNow),
            Amount = new Money
            {
                CurrencyCode = "USD",
                Units = units,
                Nanos = nanos
            }
        }).ConfigureAwait(false);
    }
}

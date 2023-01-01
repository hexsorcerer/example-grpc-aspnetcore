using Google.Protobuf.WellKnownTypes;
using Google.Type;
using Grpc.Core;

namespace GrpcExample.Server.Grpc;

public class GrpcExampleService : GrpcExample.GrpcExampleBase
{
    public override async Task<GrpcExampleResponse> GetGrpcExampleResponse(
        GrpcExampleRequest request,
        ServerCallContext context)
    {
        return await Task.FromResult(new GrpcExampleResponse
        {
            Message = "yo this is the grpc server",
            Modified = Timestamp.FromDateTime(DateTime.UtcNow),
            Amount = new Money
            {
                CurrencyCode = "USD",
                Units = 3,
                Nanos = 500_000_000
            }
        }).ConfigureAwait(false);
    }
}

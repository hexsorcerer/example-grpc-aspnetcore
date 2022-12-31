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
            Message = "yo this is the grpc server"
        }).ConfigureAwait(false);
    }
}

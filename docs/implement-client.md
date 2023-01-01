## Building your gRPC client in C#
First thing we need to do is reference the proto file from the server project.
This assumes you're working in a monorepo scenario and both projects are in the
same repo. If you're working in a polyrepo scenario, you'll have some extra
work to do to manage your protos that we won't be talking about in this example.

### Side Note
Referencing proto files from nuget seems to be the hard part, there's an open
issue on it [here](https://github.com/grpc/grpc-dotnet/issues/183), which will
lead you to [this PR](https://github.com/dotnet/aspnetcore/pull/44999) that
seems to show a solution.

There's also
[an interesting talk from an engineer at Spotify](https://youtu.be/fMq3IpPE3TU)
where he mentions keeping all proto files in a dedicated repo, and some of the
benefits they see from doing so, definitely worth a watch.

### Reference the proto file from your client project file
Add a block to your client csproj file that looks like this:
```
<ItemGroup>
  <Protobuf Include="../GrpcExample.Server/Proto/example.proto" GrpcServices="Client" />
</ItemGroup>
```

This will generate the C# client code from the proto file, which you can then
use to create a client.

### Add the client factory to your service collection
Now that you have the grpc client available, you can create and configure it
like this:
```
builder.Services.AddGrpcClient<GrpcExample.GrpcExample.GrpcExampleClient>(options =>
{
    options.Address = new Uri("http://grpcexample-server:5001");
});
```

This will make a ```GrpcExampleClient``` available to all your services through
dependency injection, and it will be configured to connect to the specified URL.

### Add the client to your controller
The easiest way to see this in action is to add it to your controller GET
method, and then see the response in the swagger page. Add a new member to your
controller:
```
private readonly GrpcExample.GrpcExampleClient _grpcClient;
```

And set it in your constructor:
```
public GrpcExampleClientController(
    GrpcExample.GrpcExampleClient grpcClient,
    ILogger<GrpcExampleClientController> logger)
{
    _grpcClient = grpcClient ?? throw new ArgumentNullException(nameof(grpcClient));
    _logger = logger;
}
```

You can now call it from your GET method:
```
[HttpGet(Name = "GetGrpcExampleType")]
public async Task<GrpcExampleResponse> Get()
{
    _logger.LogInformation("Client received a GET request");

    return await _grpcClient.GetGrpcExampleResponseAsync(new GrpcExampleRequest
    {
        Message = "yo this is the grpc client"
    });
}
```

If you go hit this endpoint on the swagger page now, you'll see the message that
was returned from the server in the response, showing that our gRPC client is
working.

```
{
  "message": "yo this is the grpc server"
}
```

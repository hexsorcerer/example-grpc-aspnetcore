## Implement your gRPC generated code in C#
Now that your server-side code is generated, we can actually start building
something! You should now have a new base class available that you can inherit
from and override the rpc method. The name of this class is derived from your
proto file, and will be formatted like ```ServiceName.ServiceNameBase```

Next, you'll need to override a method on this base class that will be the rpc
method you defined in your proto file. It will have a return type of
```Task<ResponseName>``` and take whatever parameters you defined plus an
additional one at the end that is of type ```ServerCallContext```.

So if our proto file looks like this:
```
service GrpcExample {
  rpc GetGrpcExampleResponse (GrpcExampleRequest) returns (GrpcExampleResponse);
}

message GrpcExampleRequest {
  string message = 1;
}

message GrpcExampleResponse {
  string message = 1;
}
```

then our service will look like this:
```
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
```

### Register the new service with the application
The last thing we need to do is register the new service with the application,
which requires 3 additions to our ```Program.cs``` file:

1. Add Grpc to our service collection:
```
// Add Grpc
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});
```

2. Add ```app.UseRouting();``` above ```app.UseAuthorization();```

3. Add a call to map the Grpc service endpoint ```after``` the call to
```app.UseAuthorization();```:
```
app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<GrpcExampleService>();
});
```

NOTE: it's important that the call to ```app.UseAuthorization();``` is exactly
between these two new calls we just added. I haven't had time to dig in to
the details of why this works this way, but the
[official documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0)
is probably a good place to start if you want to learn more.

### Configuring Kestrel
I said the previous step was the "last thing we need to do", but that wasn't
entirely correct. In this example we're not using HTTPS, which is normally
required for gRPC. My understanding is that for ports that accept both HTTP/1.1
and HTTP/2 connections, TLS is required for gRPC to work.

Therefore, in order for gRPC to work over plain old http, we have to configure
kestrel to listen on the gRPC port for HTTP/2 connections only. We can do that
by adding the following to our ```Program.cs```:
```
builder.WebHost.ConfigureKestrel(options =>
{
    var (httpPort, grpcPort) = GetDefinedPorts();
    options.Listen(IPAddress.Any, httpPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });

    options.Listen(IPAddress.Any, grpcPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});
```

This uses a helper method called ```GetDefinedPorts``` which I have copy/pasted
at the end of my ```Program.cs``` file, and looks like this:
```
static (int httpPort, int grpcPort) GetDefinedPorts()
{
    const int grpcPort = 5001;
    const int port = 80;
    return (port, grpcPort);
}
```

I saw this in the [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers/blob/dev/src/Services/Ordering/Ordering.API/Program.cs)
project and made use of it here.

NOTE: this is another good time to do a spot check, make sure your projects
still build and run successfully, and you can hit the controllers on both. If
you get any errors (you shouldn't), review and fix before continuing.

At this point, our server should be good to go, so let's go build the client
and start getting some messages going!

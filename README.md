# example-grpc-aspnetcore
An example project showing how to do some basic gRPC stuff in .NET.

## Getting Started
The example starts with a basic pair of client/server web API projects.
docker-compose is configured to launch both projects, and you can hit the
controllers on ports 8000/8001 for the server and client respectively. They each
return a string of either "server" or "client".

Assuming a starting point of something like this, let's add a basic gRPC client
and server that can send a message and get a reply.

## Create and configure a gRPC client/server
First we need to install some nuget packages. If you search for ```grpc``` on
nuget.org, you'll see a long list of dozens of packages, and it's not at all
clear which ones you need.

Ultimately you can just install the
[Grpc.AspNetCore](https://www.nuget.org/packages/Grpc.AspNetCore) package, which
is described as a "meta-package" and installs everything you need for either a
client or server.

If you would prefer to be a little more granular, or just want to know more
details, here's a breakdown:

### gRPC packages that all projects need
There are 2 packages that are needed by all gRPC projects, whether they are
a client or server:

- [Grpc.Tools](https://www.nuget.org/packages/Grpc.Tools)
gives you access to the protoc compilter and \<Protobuf\> tag support in
your .csproj file.
- [Google.Protobuf](https://www.nuget.org/packages/Google.Protobuf)
is the serialization technology that gRPC uses, and handles converting your
messages into binary to send over the wire

### gRPC server nuget package
In addition to the 2 packages that all gRPC projects need, you'll also need this:

- [Grpc.AspNetCore.Server.ClientFactory](https://www.nuget.org/packages/Grpc.AspNetCore.Server.ClientFactory):
gives you the ```AddGrpc``` extension method on ```IServiceCollection```,
and the ```MapGrpcService``` extenion method on ```IEndpointRouteBuilder```

### gRPC client nuget package
In addition to the 2 packages that all gRPC projects need, you'll also need this:

- [Grpc.Net.ClientFactory](https://www.nuget.org/packages/Grpc.Net.ClientFactory)
gives you access to the ```AddGrpcClient``` extension method on
```IServiceCollection```

Now that we have all the libraries and tools we need, we can create a proto
file.

## Creating a proto file
To create a gRPC service, you must create a proto file. This file represents the
contract of your gRPC service, and provides a schema for the messages that can
be sent and received.

In the interest of conciseness, I won't go into too much detail on the proto
file, but it's actually fairly straightforward. If you would like to dig into
the details, you can check out the
[official documentation](https://developers.google.com/protocol-buffers/docs/proto3)
from Google.

## Configure ```protoc``` to generate code from our proto file
Now that we have a basic proto file, we need to configure the ```protoc```
compiler to generate C# code so we can make use of it from our application.

The simplest way to do this is to right-click on your proto file in Visual
Studio and click Properties. In the Properties window, change the build action
from None to Protobuf Compiler, and gRPC Stub Classes to Server Only.

NOTE: After changing the Build Action to Protobuf Compiler, the next time to go
to the properties page on the proto file it will open a totally different UI
window instead of the properties tab. It has all the same info, but might throw
you off the first time you experience it, so thought I'd mention it. This is
totally normal and expected, no worries.

### What does this actually do to the project?

If you open the csproj file for your Server project now, you'll see a new line
near the bottom that looks like this:
```
  <ItemGroup>
    <Protobuf Include="Proto\example.proto" GrpcServices="Server" />
  </ItemGroup>
```

This is how you tell protoc that a proto file needs to have code generated for
it, and what type of service (Client/Server/etc.) should be generated. You can
update the csproj file yourself (and we'll actually do that for the rest of the
example).

NOTE: might not be a bad idea to rebuild your project at this point and just
make sure everything builds successfully (it should) before we continue. If you
get any errors you'll want to sort those out before moving on.

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

# Configuring Kestrel
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

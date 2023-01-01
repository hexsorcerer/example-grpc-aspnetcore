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

## Accessing the Google Well Known types
Now that you have gRPC working, you'll naturally start crafting some messages
with more complex data. One of the first ones I ran into was time, so how do you
define that?

Google has provided a number of additional types on top of what gRPC provides
out of the box. There's a group of these that are known as the "Well Known"
types, and you have these available to you immediately. Here's how you could add
a time field to your proto file messages.

First, import the type in your proto file:
```
import "google/protobuf/timestamp.proto";
```

Then add a field to your response message:
```
message GrpcExampleResponse {
  string message = 1;
  google.protobuf.Timestamp modified = 2;
}
```

Back in your server implementation, you can set this new field like this:
```
return await Task.FromResult(new GrpcExampleResponse
{
    Message = "yo this is the grpc server",
    Modified = Timestamp.FromDateTime(DateTime.UtcNow)
}).ConfigureAwait(false);
```
Now when you hit your GET endpoint in swagger, you'll see the time data
in the response:
```
{
  "message": "yo this is the grpc server",
  "modified": {
    "seconds": 1672535649,
    "nanos": 401316300
  }
}
```
If you needed to convert this back into a standard .NET DateTime, you can do
this:
```
var time = response.Modified.ToDateTime();
```

There's quite a bit more available to you here, highly suggested to check out
these resources for more info:

[Well Known Types proto files](https://github.com/protocolbuffers/protobuf/tree/main/src/google/protobuf)

[Well Known Types documentation](https://developers.google.com/protocol-buffers/docs/reference/csharp/namespace/google/protobuf/well-known-types)

## Accessing the Google Common types
Eventually you'll probably come across a type that you need that isn't available
with anything we've seen yet. The one that did it for me was Money. There is no
native decimal type available as of now, and
[the discussion](https://github.com/protocolbuffers/protobuf/issues/4406) on
the topic has been going for almost 5 years now, so I don't know if I'd expect
it anytime soon. There's also nothing in the well known types for storing
money values. So what can we do?

Well, there's a couple of options. Theres
[an excellent article](https://visualrecode.com/blog/csharp-decimals-in-grpc/)
on implementing this yourself, and you could do that. But Google has actually
already done this for their own internal use, and they've made that work
available [here](https://github.com/googleapis/googleapis/tree/master/google/type),
so why not make use of it?

Unfortunately, you do "not" have these immediately available, you gotta do some
work. The first problem is how do you get the proto files in your project? Is
there a nuget package we can install to provide it for us? The answer is no. =(

### Using git submodules to consume proto files
I'm sure there are numerous ways you could approch this problem, but I wasn't
able to find any widely accepted way online. The solution I came up with was to
use git submodules to clone this repo inside of mine, and then you can access
all the proto files by just referencing them from your project file. This
actually seems to work pretty well, so here's how I did it.
```
mkdir -p src/BuildingBlocks
cd src/BuildingBlocks
git submodule add https://github.com/googleapis/googleapis.git
```
This will clone the googleapis repo into a directory in your project, but will
not track anything in that directory.

Now we can reference the money.proto from our projects:

The server:
```
<ItemGroup>
  <Protobuf Include="Proto\example.proto" GrpcServices="Server" AdditionalImportDirs="..\..\..\BuildingBlocks\googleapis" />
  <Protobuf Include="..\..\..\BuildingBlocks\googleapis\google\type\money.proto" ProtoRoot="..\..\..\BuildingBlocks\googleapis\google\type" />
</ItemGroup>
```

The client:
```
<ItemGroup>
  <Protobuf Include="../GrpcExample.Server/Proto/example.proto" GrpcServices="Client" AdditionalImportDirs="..\..\..\BuildingBlocks\googleapis" />
  <Protobuf Include="..\..\..\BuildingBlocks\googleapis\google\type\money.proto" ProtoRoot="..\..\..\BuildingBlocks\googleapis\google\type" />
</ItemGroup>
```

A couple of important things going on here that took me a while to figure out:
- AdditionalImportDirs lets our proto file know where to find the proto files
we're importing
- ProtoRoot is normally set by default to the parent of the invluded file, but
this only applies to protos within the project. When referencing proto files
outside of the project like we are now, we have to set this ourselves.

NOTE: While writing this, I realized that I didn't have to set this for my own
proto file when referencing from the client project, and based on the docs I
kinda feel like I should have.

Some excellent documentation here that shows you a lot of what you can do with
the Protobuf directive in the project file.

[Protocol Buffers/gRPC Codegen Integration Into .NET Build](https://chromium.googlesource.com/external/github.com/grpc/grpc/+/HEAD/src/csharp/BUILD-INTEGRATION.md)

### Import money into your proto file
You can now import the money type into your own proto file:
```
import "google/type/money.proto";
```

And you can use it to add money fields to your messages:
```
message GrpcExampleResponse {
  string message = 1;
  google.protobuf.Timestamp modified = 2;
  google.type.Money amount = 3;
}
```

Back in your server implementation, you can set an amount of about three fitty
like this:

Add a scaling factor to the service:
```
private const decimal NanoFactor = 1_000_000_000;
```
Convert your decimal value to units and nanos:
```
const decimal value = 3.50m;
var units = decimal.ToInt64(value);
var nanos = decimal.ToInt32((value - units) * NanoFactor);
```
Now you can use these values to build a money object to send over gRPC:
```
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
```

The basic explanation is that units represent dollars and nanos represent cents,
with the caveat that it's in nano units, which are 10^-9, so $0.50 would be
500_000_000 nano units. You'll need to know this in order for the caller to
convert Money back to decimal.

### Convert from Money back to decimal
You should now be receiving your shiny new Money amount in your responses. So
how do we convert it back into a decimal? A little math:
```
decimal value = units + nanos / scale factor
```
Basically just need to scale the nano units back down to their decimal value,
then add the whole dollar amount.

Go back to your controller and add a const member:
```
private const decimal NanoScaleFactor = 1_000_000_000;
```
Be sure to make this type ```decimal``` even though it screams ```int```, so
our math ends up in the correct type without and casting or anything needed.

Now you can do the math:
```
var amount = response.Amount.Units + response.Amount.Nanos / NanoScaleFactor;
_logger.LogInformation("Amount was about {Amount}", amount.ToString("C", CultureInfo.CreateSpecificCulture("en-US")));
```

I chose to log it because that was a quicker easier way for me to see it was
working, you can just look in the docker logs to see the correct amount.
```
2022-12-31 21:16:14       Amount was about $3.50
```

## Closing Thoughts
Learning the basics of gRPC has been a really difficult process for me for some
reason. There's a lot of really good documentation out there, but it can be hard
to find if you don't already know exactly what you're looking for. There's a lot
of really good examples out there, but they all stop at the absolute minimum
functionality of sending a string or an integer.

I wrote all this down so I would have something to reference in my own work and
not forget everything, but I hope this is able to fill a small gap and show how
to deal with some very common problems that come up soon after the basic
example.

## Resources
I found a lot of really good documentation while researching all of this, and
although I reference it through the docs, I thought it might be helpful to have
it all in one place also.

### Github
- [grpc-dotnet](https://github.com/grpc/grpc-dotnet)
- [Google Well Known Types](https://github.com/protocolbuffers/protobuf/tree/main/src/google/protobuf)
- [Google Common Types](https://github.com/googleapis/googleapis/tree/master/google/type)
- [Decimal Type?](https://github.com/protocolbuffers/protobuf/issues/4406)

### NuGet
- [Grpc.AspNetCore](https://www.nuget.org/packages/Grpc.AspNetCore/2.50.0#readme-body-tab)
- [Grpc.Net.ClientFactory](https://www.nuget.org/packages/Grpc.Net.ClientFactory/#readme-body-tab)

### MSDN
- [gRPC services with ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/grpc/aspnetcore?view=aspnetcore-6.0&tabs=visual-studio)

### Google
- [Overview](https://developers.google.com/protocol-buffers/docs/overview)
- [proto3](https://developers.google.com/protocol-buffers/docs/proto3)
- [proto style guide](https://developers.google.com/protocol-buffers/docs/style)
- [C# Tutorial](https://developers.google.com/protocol-buffers/docs/csharptutorial)
- [Google.Protobuf.WellKnownTypes](https://developers.google.com/protocol-buffers/docs/reference/csharp/namespace/google/protobuf/well-known-types)
- [Protocol Buffers/gRPC Codegen Integration Into .NET Build](https://chromium.googlesource.com/external/github.com/grpc/grpc/+/HEAD/src/csharp/BUILD-INTEGRATION.md)

### Web
- [C# decimals in gRPC](https://visualrecode.com/blog/csharp-decimals-in-grpc/)
- [Don't Do It All Yourself: Exploiting gRPC Well Known Types in .NET Core](https://visualstudiomagazine.com/articles/2020/01/16/grpc-well-known-types.aspx)
- [Reusing and Recycling Data Structures in gRPC Services in .NET Core](https://visualstudiomagazine.com/articles/2020/01/09/grpc-data-structures.aspx)

### Youtube
- [The Story of Why We Migrate to gRPC and How We Go About It - Matthias Grüter, Spotify](https://youtu.be/fMq3IpPE3TU)

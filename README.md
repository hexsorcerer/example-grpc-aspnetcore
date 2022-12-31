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

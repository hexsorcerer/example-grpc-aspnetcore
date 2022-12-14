## Install NuGet dependencies in client/server projects
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
and the ```MapGrpcService``` extension method on ```IEndpointRouteBuilder```

### gRPC client nuget package
In addition to the 2 packages that all gRPC projects need, you'll also need this:

- [Grpc.Net.ClientFactory](https://www.nuget.org/packages/Grpc.Net.ClientFactory)
gives you access to the ```AddGrpcClient``` extension method on
```IServiceCollection```

Now that we have all the libraries and tools we need, we can create a proto
file.

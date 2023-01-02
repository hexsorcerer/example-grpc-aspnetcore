# example-grpc-aspnetcore
An example project showing how to do some basic gRPC stuff in .NET.

## Overview
This guide will walk you through step-by-step how to get a couple of ASP.NET
Core services setup with gRPC and communicatiing with each other using basic
data types like strings and ints, along with more advanced types like time and
money.

It is written from the perspective of someone who has never worked with gRPC,
and tries to explain the "what" and "why" of each step along the way. 

## Walkthrough

1. [Install dependencies from NuGet](docs/install-deps.md)
2. [Create proto file](docs/create-proto.md)
3. [Generate C# code](docs/generate-csharp-code.md)
4. [Implement server](docs/implement-server.md)
5. [Implement client](docs/implement-client.md)
6. [Using the Well Known types](docs/well-known-types.md)
7. [Using the Common types](docs/common-types.md)

## Further Reading
Here's a collection of documentation I discovered while researching this topic.

[gRPC Resources](docs/resources.md)


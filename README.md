# example-grpc-aspnetcore
An example project showing how to do some basic gRPC stuff in .NET.

## Overview
The example starts with a basic pair of client/server web API projects.
docker-compose is configured to launch both projects, and you can hit the
controllers on ports 8000/8001 for the server and client respectively. They each
return a string of either "server" or "client".

Assuming a starting point of something like this, the following example will
take you through a few different scenarios. Steps 1-6 will walk you through
the basics of creating a simple client/server setup, much like many other
examples out there.

Steps 6-7 are a little more interesting, and what (imho) set this guide apart.
Being aware of and knowing how to use both of these collections of additional
types is something you'll encounter almost immediately after working through
the basic example, and there's hardly anything out there that walks you through
figuring this out. We'll look at Timestamp and Money in these examples, which
are probably the most common things you would want next (at least I thought so).

## Walkthrough

1. [Install dependencies from NuGet](docs/install-deps.md)
2. [Create proto file](docs/create-proto.md)
3. [Generate C# code](docs/generate-csharp-code.md)
4. [Implement server](docs/implement-server.md)
5. [Implement client](docs/implement-client.md)
6. [Using the Well Known types](docs/well-known-types.md)
7. [Using the Common types](docs/common-types.md)

As an added bonus, here is an aggregation of all the documentation I discovered
while figuring all this out:

[gRPC Resources](docs/resources.md)

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

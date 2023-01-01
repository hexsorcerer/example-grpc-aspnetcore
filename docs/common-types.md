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
- ProtoRoot is normally set by default to the parent of the included file, but
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

Back in your server implementation, you can set an amount of about tree fitty
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

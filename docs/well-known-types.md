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

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

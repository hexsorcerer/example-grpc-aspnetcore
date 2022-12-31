#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["GrpcExample.Server/GrpcExample.Server.csproj", "GrpcExample.Server/"]
RUN dotnet restore "GrpcExample.Server/GrpcExample.Server.csproj"
COPY . .
WORKDIR "/src/GrpcExample.Server"
RUN dotnet build "GrpcExample.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GrpcExample.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GrpcExample.Server.dll"]
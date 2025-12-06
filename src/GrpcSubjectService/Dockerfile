# Multi-stage Dockerfile for GrpcSubjectService (ASP.NET Core / gRPC)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the entire src tree so sibling projects (and their Protos) are available
COPY src/ ./

# Build from the service folder
WORKDIR /src/GrpcSubjectService
RUN dotnet restore "GrpcSubjectService.csproj"
RUN dotnet publish "GrpcSubjectService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN useradd -m -u 1000 app || true

COPY --chown=app:app --from=build /app/publish .

ENV ASPNETCORE_URLS="http://+:80"
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 80

USER app

ENTRYPOINT ["dotnet", "GrpcSubjectService.dll"]

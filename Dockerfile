# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY PortfolioEnterprise.sln .
COPY src/Domain/*.csproj src/Domain/
COPY src/Application/*.csproj src/Application/
COPY src/Infrastructure/*.csproj src/Infrastructure/
COPY src/WebAPI/*.csproj src/WebAPI/
COPY tests/Core.Tests/*.csproj tests/Core.Tests/

# Restore dependencies
RUN dotnet restore

# Copy all source
COPY src/ src/
COPY tests/ tests/

# Publish WebAPI
WORKDIR /src/src/WebAPI
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "WebAPI.dll"]

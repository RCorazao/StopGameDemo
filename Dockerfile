# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the solution file and restore dependencies
COPY *.sln .
COPY StopGame.Application/*.csproj ./StopGame.Application/
COPY StopGame.Domain/*.csproj ./StopGame.Domain/
COPY StopGame.Infrastructure/*.csproj ./StopGame.Infrastructure/
COPY StopGame.Web/*.csproj ./StopGame.Web/
RUN dotnet restore

# Copy the rest of the application source code
COPY . .

# Publish the application
WORKDIR /app/StopGame.Web
RUN dotnet publish -c Release -o /app/publish

# Use the ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose the necessary ports
EXPOSE 8080
EXPOSE 8081

# Set the entry point for the application
ENTRYPOINT ["dotnet", "StopGame.Web.dll"]

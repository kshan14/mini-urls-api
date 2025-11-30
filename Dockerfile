FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY MiniUrl/MiniUrl.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY MiniUrl/ ./
RUN dotnet publish -c Release -o out

# Use the runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port 8080
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "MiniUrl.dll"]
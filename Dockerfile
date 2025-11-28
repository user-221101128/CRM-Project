# ===========================
# 1. Build Stage
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore dependencies
RUN dotnet restore

# Build the project
RUN dotnet publish -c Release -o /app/publish


# ===========================
# 2. Runtime Stage
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS runtime
WORKDIR /app

# Copy published output from build
COPY --from=build /app/publish .

# Expose port (Render expects 8080)
EXPOSE 8080

# Set URLs to 0.0.0.0 on port 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Start the app
ENTRYPOINT ["dotnet", "CRM.dll"]

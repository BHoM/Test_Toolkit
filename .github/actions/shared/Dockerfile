# Multi-stage build using .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the entire source code
COPY . .

# Restore and build the .NET Standard projects first
RUN dotnet restore CodeCompliance_oM/CodeComplianceTest_oM.csproj
RUN dotnet restore CodeComplianceTest_Engine/CodeComplianceTest_Engine.csproj
RUN dotnet restore Test_Engine/Test_Engine.csproj

# Build the projects
RUN dotnet build CodeCompliance_oM/CodeComplianceTest_oM.csproj -c Release
RUN dotnet build Test_Engine/Test_Engine.csproj -c Release
RUN dotnet build CodeComplianceTest_Engine/CodeComplianceTest_Engine.csproj -c Release

# Build the compliance runner console app
WORKDIR /src/.github/actions/shared/ComplianceRunner
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

# Install git for getting changed files
RUN apt-get update && apt-get install -y git && rm -rf /var/lib/apt/lists/*

# Copy built application
COPY --from=build /app/publish .

# Copy the entrypoint script
COPY .github/actions/shared/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Set the entrypoint
ENTRYPOINT ["/app/entrypoint.sh"]

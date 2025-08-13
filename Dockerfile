# Dockerfile for Stella-OpenAI Discord Bot
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY Stella-OpenAI/*.csproj ./
RUN dotnet restore

# Copy source code
COPY Stella-OpenAI/ .

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_ENVIRONMENT=Production

# Expose port (if needed)
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "Stella-OpenAI.dll"]

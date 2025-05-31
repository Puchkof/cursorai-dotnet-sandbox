# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for better layer caching
COPY src/HeroBoxAI.Domain/HeroBoxAI.Domain.csproj src/HeroBoxAI.Domain/
COPY src/HeroBoxAI.Application/HeroBoxAI.Application.csproj src/HeroBoxAI.Application/
COPY src/HeroBoxAI.Infrastructure/HeroBoxAI.Infrastructure.csproj src/HeroBoxAI.Infrastructure/
COPY src/HeroBoxAI.WebApi/HeroBoxAI.WebApi.csproj src/HeroBoxAI.WebApi/

# Restore dependencies for the main application projects only
WORKDIR /src/src/HeroBoxAI.WebApi
RUN dotnet restore

# Copy the rest of the source code
WORKDIR /src
COPY src/ src/

# Build the application
WORKDIR /src/src/HeroBoxAI.WebApi
RUN dotnet build -c Release -o /app/build --no-restore

# Publish the application
RUN dotnet publish -c Release -o /app/publish --no-restore

# Use the official .NET 8 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user
RUN addgroup --system --gid 1001 dotnet \
    && adduser --system --uid 1001 --ingroup dotnet --shell /bin/false dotnet

# Copy the published application
COPY --from=build /app/publish .

# Create logs directory and set permissions
RUN mkdir -p /app/logs && chown -R dotnet:dotnet /app

# Switch to non-root user
USER dotnet

# Expose the port
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "HeroBoxAI.WebApi.dll"] 
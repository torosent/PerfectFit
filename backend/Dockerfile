# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY PerfectFit.sln ./
COPY src/PerfectFit.Core/PerfectFit.Core.csproj ./src/PerfectFit.Core/
COPY src/PerfectFit.UseCases/PerfectFit.UseCases.csproj ./src/PerfectFit.UseCases/
COPY src/PerfectFit.Infrastructure/PerfectFit.Infrastructure.csproj ./src/PerfectFit.Infrastructure/
COPY src/PerfectFit.Web/PerfectFit.Web.csproj ./src/PerfectFit.Web/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY src/ ./src/

# Build the application
RUN dotnet build src/PerfectFit.Web/PerfectFit.Web.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish src/PerfectFit.Web/PerfectFit.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" --uid 1000 appuser

# Copy published application
COPY --from=publish /app/publish .

# Set ownership and switch to non-root user
RUN chown -R appuser:appuser /app
USER appuser

# Expose port (Azure Container Apps uses 80 by default)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "PerfectFit.Web.dll"]

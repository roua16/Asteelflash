# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY Directory.Packages.props ./
COPY src/ITStockM.WebApi/ITStockM.WebApi.csproj src/ITStockM.WebApi/
COPY src/ITStockM.Application/ITStockM.Application.csproj src/ITStockM.Application/
COPY src/ITStockM.Infrastructure/ITStockM.Infrastructure.csproj src/ITStockM.Infrastructure/
COPY src/ITStockM.Domain/ITStockM.Domain.csproj src/ITStockM.Domain/
RUN dotnet restore --disable-parallel --verbosity normal ./src/ITStockM.WebApi/ITStockM.WebApi.csproj

# Copy everything else and publish
COPY . ./
RUN dotnet publish ./src/ITStockM.WebApi/ITStockM.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Blazor Server/ASP.NET Core will listen on this port
ENV ASPNETCORE_HTTP_PORTS=8080
# Ensure Kestrel binds to the expected port
ENV ASPNETCORE_URLS="http://+:8080"
EXPOSE 8080

COPY --from=build /app/publish ./
# Copy configuration files from build stage (needed as dotnet publish doesn't include them by default)
COPY --from=build /src/src/ITStockM.WebApi/appsettings.json ./
COPY --from=build /src/src/ITStockM.WebApi/appsettings.Development.json ./

ENTRYPOINT ["dotnet", "ITStockM.WebApi.dll"]

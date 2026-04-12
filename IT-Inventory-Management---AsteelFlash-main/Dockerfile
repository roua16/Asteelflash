# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ITStockM.csproj ./
RUN dotnet restore --disable-parallel ./ITStockM.csproj

# Copy everything else and publish
COPY . ./
RUN dotnet publish ./ITStockM.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Blazor Server/ASP.NET Core will listen on this port
ENV ASPNETCORE_HTTP_PORTS=8080
# Ensure Kestrel binds to the expected port
ENV ASPNETCORE_URLS="http://+:8080"
EXPOSE 8080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "ITStockM.dll"]

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Improve restore reliability on unstable/proxied networks.
ENV DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP2SUPPORT=false

# Copy csproj and restore as distinct layers
COPY ITStockM.csproj ./
RUN for i in 1 2 3; do \
			dotnet restore --disable-parallel ./ITStockM.csproj && break; \
			if [ "$i" -eq 3 ]; then exit 1; fi; \
			echo "dotnet restore failed, retrying ($i/3)..."; \
		done

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

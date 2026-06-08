# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy full source first so restore can always resolve centrally-managed package versions
COPY . ./
RUN dotnet restore --disable-parallel --verbosity normal ./src/ITStockM.WebApi/ITStockM.WebApi.csproj

# Publish
RUN dotnet publish ./src/ITStockM.WebApi/ITStockM.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Required by System.DirectoryServices.Protocols on Linux.
RUN apt-get update \
	&& apt-get install -y --no-install-recommends libldap2 libsasl2-2 ca-certificates \
		&& for d in /lib/*-linux-gnu /usr/lib/*-linux-gnu; do \
				 if [ -e "$d/libldap.so.2" ]; then ln -sf "$d/libldap.so.2" "$d/libldap-2.5.so.0"; fi; \
				 if [ -e "$d/liblber.so.2" ]; then ln -sf "$d/liblber.so.2" "$d/liblber-2.5.so.0"; fi; \
			 done \
	&& rm -rf /var/lib/apt/lists/*

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

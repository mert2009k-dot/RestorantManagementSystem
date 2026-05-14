# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["RestoranProjesi/RestoranProjesi.csproj", "RestoranProjesi/"]
RUN dotnet restore "RestoranProjesi/RestoranProjesi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/RestoranProjesi"
RUN dotnet build "RestoranProjesi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RestoranProjesi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Render dynamically assigns a port via the PORT environment variable.
# ASP.NET Core 8+ listens on 8080 by default, but we can override it or 
# let Render map its internal port to the container.
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "RestoranProjesi.dll"]

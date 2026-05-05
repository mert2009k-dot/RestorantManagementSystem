# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy csproj and restore
COPY ["RestoranProjesi/RestoranProjesi.csproj", "RestoranProjesi/"]
RUN dotnet restore "RestoranProjesi/RestoranProjesi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/source/RestoranProjesi"
RUN dotnet publish "RestoranProjesi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RestoranProjesi.dll"]

# Bookshelf.API — obraz produkcyjny (multi-stage).
# Etap build: SDK kompiluje i publikuje; etap runtime: lekki aspnet serwuje.

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Najpierw sam csproj -> warstwa restore cache'uje sie, dopoki nie zmienisz zaleznosci.
COPY Bookshelf.API.csproj ./
RUN dotnet restore

# Reszta zrodel (wraz z wwwroot/avatars i appsettings.json).
COPY . ./
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# HTTP-only w kontenerze (bez certow); compose/nginx kieruja ruch na 8080.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Bookshelf.API.dll"]

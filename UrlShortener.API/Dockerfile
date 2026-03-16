FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["UrlShortener.API/UrlShortener.API.csproj", "UrlShortener.API/"]
COPY ["UrlShortener.Common/UrlShortener.Common.csproj", "UrlShortener.Common/"]
COPY ["UrlShortener.Data/UrlShortener.Data.csproj", "UrlShortener.Data/"]
COPY ["UrlShortener.Services/UrlShortener.Services.csproj", "UrlShortener.Services/"]

RUN dotnet restore "UrlShortener.API/UrlShortener.API.csproj"

COPY . .
WORKDIR "/src/UrlShortener.API"
RUN dotnet publish "UrlShortener.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "UrlShortener.API.dll"]
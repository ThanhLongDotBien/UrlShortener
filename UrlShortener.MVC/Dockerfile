FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["UrlShortener.MVC/UrlShortener.MVC.csproj", "UrlShortener.MVC/"]
COPY ["UrlShortener.Common/UrlShortener.Common.csproj", "UrlShortener.Common/"]
COPY ["UrlShortener.Data/UrlShortener.Data.csproj", "UrlShortener.Data/"]
COPY ["UrlShortener.Services/UrlShortener.Services.csproj", "UrlShortener.Services/"]

RUN dotnet restore "UrlShortener.MVC/UrlShortener.MVC.csproj"

COPY . .
WORKDIR "/src/UrlShortener.MVC"
RUN dotnet publish "UrlShortener.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "UrlShortener.MVC.dll"]
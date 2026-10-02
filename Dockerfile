# Render / container deploy for MoviePlatform.Api
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore backend/MoviePlatform.Api/MoviePlatform.Api.csproj
RUN dotnet publish backend/MoviePlatform.Api/MoviePlatform.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
# Render sets PORT; ASP.NET Core 8+ honors ASPNETCORE_HTTP_PORTS when set at runtime.
ENTRYPOINT ["dotnet", "MoviePlatform.Api.dll"]

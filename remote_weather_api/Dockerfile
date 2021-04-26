FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5100
EXPOSE 5101

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["remote_weather_api/remote_weather_api.csproj", "remote_weather_api/"]
RUN dotnet restore "remote_weather_api/remote_weather_api.csproj"
COPY . .
WORKDIR "/src/remote_weather_api"
RUN dotnet build "remote_weather_api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p password
RUN dotnet publish "remote_weather_api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /https /https
ENTRYPOINT ["dotnet", "remote_weather_api.dll"]
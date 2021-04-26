FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["research_redis_key_pattern_api/research_redis_key_pattern_api.csproj", "research_redis_key_pattern_api/"]
RUN dotnet restore "research_redis_key_pattern_api/research_redis_key_pattern_api.csproj"
COPY . .
WORKDIR "/src/research_redis_key_pattern_api"
RUN dotnet build "research_redis_key_pattern_api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p password
RUN dotnet publish "research_redis_key_pattern_api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /https /https
ENTRYPOINT ["dotnet", "research_redis_key_pattern_api.dll"]
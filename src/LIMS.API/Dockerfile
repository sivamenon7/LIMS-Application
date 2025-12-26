FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/LIMS.API/LIMS.API.csproj", "src/LIMS.API/"]
COPY ["src/LIMS.Application/LIMS.Application.csproj", "src/LIMS.Application/"]
COPY ["src/LIMS.Core/LIMS.Core.csproj", "src/LIMS.Core/"]
COPY ["src/LIMS.Infrastructure/LIMS.Infrastructure.csproj", "src/LIMS.Infrastructure/"]
COPY ["src/LIMS.EventBus/LIMS.EventBus.csproj", "src/LIMS.EventBus/"]
COPY ["src/LIMS.Caching/LIMS.Caching.csproj", "src/LIMS.Caching/"]
COPY ["infrastructure/LIMS.ServiceDefaults/LIMS.ServiceDefaults.csproj", "infrastructure/LIMS.ServiceDefaults/"]

RUN dotnet restore "src/LIMS.API/LIMS.API.csproj"
COPY . .
WORKDIR "/src/src/LIMS.API"
RUN dotnet build "LIMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LIMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LIMS.API.dll"]

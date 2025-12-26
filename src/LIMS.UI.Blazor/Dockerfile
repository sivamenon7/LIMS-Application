FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/LIMS.UI.Blazor/LIMS.UI.Blazor.csproj", "src/LIMS.UI.Blazor/"]
RUN dotnet restore "src/LIMS.UI.Blazor/LIMS.UI.Blazor.csproj"
COPY . .
WORKDIR "/src/src/LIMS.UI.Blazor"
RUN dotnet build "LIMS.UI.Blazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LIMS.UI.Blazor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY src/LIMS.UI.Blazor/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80

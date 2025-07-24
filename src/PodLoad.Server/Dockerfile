FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=80
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PodLoad.Server/PodLoad.Server.csproj", "PodLoad.Server/"]
COPY ["PodLoad.Common/PodLoad.Common.csproj", "PodLoad.Common/"]
RUN dotnet restore "PodLoad.Server/PodLoad.Server.csproj"
COPY . .
WORKDIR "/src/PodLoad.Server"
RUN dotnet build "./PodLoad.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PodLoad.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PodLoad.Server.dll"]

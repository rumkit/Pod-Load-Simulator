FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
ENV ClientSettings__DefaultMemory_Mb=64
ENV ClientSettings__DefaultPercentage=0
ENV ClientSettings__ServerAddress=podload.server
ENV ClientSettings__ServerPort=80
ENV ClientSettings__ReportEndpoint=api/client-report    
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PodLoad.Client/PodLoad.Client.csproj", "PodLoad.Client/"]
COPY ["PodLoad.Common/PodLoad.Common.csproj", "PodLoad.Common/"]
RUN dotnet restore "PodLoad.Client/PodLoad.Client.csproj"
COPY . .
WORKDIR "/src/PodLoad.Client"
RUN dotnet build "./PodLoad.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PodLoad.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PodLoad.Client.dll"]

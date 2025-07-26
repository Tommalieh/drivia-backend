FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DriveSmart.Api/DriveSmart.Api.csproj", "DriveSmart.Api/"]
COPY ["DriveSmart.Application/DriveSmart.Application.csproj", "DriveSmart.Application/"]
COPY ["DriveSmart.Domain/DriveSmart.Domain.csproj", "DriveSmart.Domain/"]
COPY ["DriveSmart.Persistence/DriveSmart.Persistence.csproj", "DriveSmart.Persistence/"]
COPY ["DriveSmart.Shared/DriveSmart.Shared.csproj", "DriveSmart.Shared/"]
RUN dotnet restore "DriveSmart.Api/DriveSmart.Api.csproj"
COPY . .
WORKDIR "/src/DriveSmart.Api"
RUN dotnet build "./DriveSmart.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DriveSmart.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DriveSmart.Api.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5003
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src


COPY ["LocalNuget/", "/src/LocalNuget/"]
COPY ["nuget.config", "/src/"]

COPY ["UserService/QUserService.API/QUserService.API.csproj", "QUserService.API/"]
COPY ["UserService/QUserService.Domain/QUserService.Domain.csproj", "QUserService.Domain/"]
COPY ["UserService/QUserService.Infrastructure/QUserService.Infrastructure.csproj", "QUserService.Infrastructure/"]
COPY ["UserService/QUserService.Application/QUserService.Application.csproj", "QUserService.Application/"]
COPY ["UserService/QUserService.Contracts/QUserService.Contracts.csproj", "QUserService.Contracts/"]
RUN dotnet restore "QUserService.API/QUserService.API.csproj"
COPY . .
WORKDIR "/src/UserService/QUserService.API"
RUN dotnet build "./QUserService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./QUserService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QUserService.API.dll"]

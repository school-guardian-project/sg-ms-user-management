FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ms-user-management.Api/ms-user-management.Api.csproj", "src/ms-user-management.Api/"]
RUN dotnet restore "src/ms-user-management.Api/ms-user-management.Api.csproj"
COPY . .
WORKDIR "/src/src/ms-user-management.Api"
RUN dotnet build "./ms-user-management.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ms-user-management.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ms-user-management.Api.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/KrtBank.Api/KrtBank.Api.csproj", "src/KrtBank.Api/"]
COPY ["src/KrtBank.Infrastructure/KrtBank.Infrastructure.csproj", "src/KrtBank.Infrastructure/"]
COPY ["src/KrtBank.Application/KrtBank.Application.csproj", "src/KrtBank.Application/"]
COPY ["src/KrtBank.Domain/KrtBank.Domain.csproj", "src/KrtBank.Domain/"]
RUN dotnet restore "./src/KrtBank.Api/KrtBank.Api.csproj"
COPY . .
WORKDIR "/src/src/KrtBank.Api"
RUN dotnet build "./KrtBank.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./KrtBank.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KrtBank.Api.dll"]

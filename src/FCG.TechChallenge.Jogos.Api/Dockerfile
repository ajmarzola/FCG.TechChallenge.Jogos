# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0.100 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/FCG.TechChallenge.Jogos.Api/FCG.TechChallenge.Jogos.Api.csproj", "src/FCG.TechChallenge.Jogos.Api/"]
COPY ["src/FCG.TechChallenge.Jogos.Application/FCG.TechChallenge.Jogos.Application.csproj", "src/FCG.TechChallenge.Jogos.Application/"]
COPY ["src/FCG.TechChallenge.Jogos.Domain/FCG.TechChallenge.Jogos.Domain.csproj", "src/FCG.TechChallenge.Jogos.Domain/"]
COPY ["src/FCG.TechChallenge.Jogos.Infrastructure/FCG.TechChallenge.Jogos.Infrastructure.csproj", "src/FCG.TechChallenge.Jogos.Infrastructure/"]

RUN dotnet restore "src/FCG.TechChallenge.Jogos.Api/FCG.TechChallenge.Jogos.Api.csproj"

COPY ./src .
WORKDIR /src/FCG.TechChallenge.Jogos.Api
RUN dotnet build "FCG.TechChallenge.Jogos.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Etapa de publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FCG.TechChallenge.Jogos.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Etapa final (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "FCG.TechChallenge.Jogos.Api.dll"]
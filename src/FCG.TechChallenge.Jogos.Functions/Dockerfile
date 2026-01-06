# =======================
#   Etapa de Build
# =======================
FROM mcr.microsoft.com/dotnet/sdk:8.0.100 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia apenas os csproj necessários para cache eficiente
COPY FCG.TechChallenge.Jogos.sln .
COPY src/FCG.TechChallenge.Jogos.Functions/FCG.TechChallenge.Jogos.Functions.csproj src/FCG.TechChallenge.Jogos.Functions/
COPY src/FCG.TechChallenge.Jogos.Api/FCG.TechChallenge.Jogos.Api.csproj src/FCG.TechChallenge.Jogos.Api/
COPY src/FCG.TechChallenge.Jogos.Application/FCG.TechChallenge.Jogos.Application.csproj src/FCG.TechChallenge.Jogos.Application/
COPY src/FCG.TechChallenge.Jogos.Domain/FCG.TechChallenge.Jogos.Domain.csproj src/FCG.TechChallenge.Jogos.Domain/
COPY src/FCG.TechChallenge.Jogos.Infrastructure/FCG.TechChallenge.Jogos.Infrastructure.csproj src/FCG.TechChallenge.Jogos.Infrastructure/
COPY src/FCG.TechChallenge.Jogos.Contracts/FCG.TechChallenge.Jogos.Contracts.csproj src/FCG.TechChallenge.Jogos.Contracts/

# 🔧 Restaurar apenas o projeto Functions (evita erro com testes ausentes)
RUN dotnet restore "src/FCG.TechChallenge.Jogos.Functions/FCG.TechChallenge.Jogos.Functions.csproj"

# Copia todo o código-fonte
COPY ./src ./src

# Compila apenas o projeto Functions
WORKDIR /src/src/FCG.TechChallenge.Jogos.Functions
RUN dotnet build "FCG.TechChallenge.Jogos.Functions.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# =======================
#   Etapa de Publish
# =======================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/src/FCG.TechChallenge.Jogos.Functions
RUN dotnet publish "FCG.TechChallenge.Jogos.Functions.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# =======================
#   Etapa de Runtime
# =======================
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 8080

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCG.TechChallenge.Jogos.Functions.dll"]

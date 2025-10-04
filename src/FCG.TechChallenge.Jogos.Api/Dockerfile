# ===== Etapa de build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0.100 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia somente os csproj para otimizar cache de restore
COPY ["src/FCG.TechChallenge.Jogos.Api/FCG.TechChallenge.Jogos.Api.csproj", "src/FCG.TechChallenge.Jogos.Api/"]
COPY ["src/FCG.TechChallenge.Jogos.Application/FCG.TechChallenge.Jogos.Application.csproj", "src/FCG.TechChallenge.Jogos.Application/"]
COPY ["src/FCG.TechChallenge.Jogos.Domain/FCG.TechChallenge.Jogos.Domain.csproj", "src/FCG.TechChallenge.Jogos.Domain/"]
COPY ["src/FCG.TechChallenge.Jogos.Infrastructure/FCG.TechChallenge.Jogos.Infrastructure.csproj", "src/FCG.TechChallenge.Jogos.Infrastructure/"]

# Restore primeiro (aproveita cache entre builds)
RUN dotnet restore "src/FCG.TechChallenge.Jogos.Api/FCG.TechChallenge.Jogos.Api.csproj"

# Agora copia o código todo e compila
COPY ./src ./src
# ATENÇÃO: após o COPY acima, o caminho correto é /src/src/...
WORKDIR /src/src/FCG.TechChallenge.Jogos.Api
RUN dotnet build "FCG.TechChallenge.Jogos.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# ===== Etapa de publish =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# O WORKDIR permanece /src/src/FCG.TechChallenge.Jogos.Api aqui
RUN dotnet publish "FCG.TechChallenge.Jogos.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-build

# ===== Etapa final (runtime) =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Kestrel escutando em 8080 (requerido pelo ECS/health)
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "FCG.TechChallenge.Jogos.Api.dll"]
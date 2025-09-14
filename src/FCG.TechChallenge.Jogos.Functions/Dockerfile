FROM mcr.microsoft.com/dotnet/runtime-deps:8.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FCG.TechChallenge.Jogos.Functions/FCG.TechChallenge.Jogos.Functions.csproj", "src/FCG.TechChallenge.Jogos.Functions/"]
# + copie csproj das libs referenciadas
RUN dotnet restore "src/FCG.TechChallenge.Jogos.Functions/FCG.TechChallenge.Jogos.Functions.csproj"
COPY . .
WORKDIR "/src/src/FCG.TechChallenge.Jogos.Functions"
RUN dotnet publish -c Release -o /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8
WORKDIR /home/site/wwwroot
COPY --from=build /home/site/wwwroot .
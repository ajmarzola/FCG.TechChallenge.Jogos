$resourceGroup = "rg-jogos-fiap"
$location = "eastus2"
$elasticName = "elastic-jogos-fiap"

# Install extension if needed
az extension add --name elastic --yes

Write-Host "Creating Elastic Monitor Resource..."
# Note: Creating Elastic resource via CLI might require accepting marketplace terms manually first.
# If this fails, please create via Portal: https://portal.azure.com/#create/Microsoft.Elastic
az elastic monitor create --name $elasticName --resource-group $resourceGroup --location $location --sku "essentials_monthly"

Write-Host "--------------------------------------------------"
Write-Host "Elastic Resource Created (or attempted)."
Write-Host "Please go to the Azure Portal -> $elasticName -> 'Manage in Elastic Cloud' to get your:"
Write-Host "1. URI (Endpoint)"
Write-Host "2. API Key (Generate one in Management -> Security -> API Keys)"
Write-Host "--------------------------------------------------"
Write-Host "Then update:"
Write-Host " - src/FCG.TechChallenge.Jogos.Api/appsettings.json"
Write-Host " - src/FCG.TechChallenge.Jogos.Functions/local.settings.json"
Write-Host "--------------------------------------------------"

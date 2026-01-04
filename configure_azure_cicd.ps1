$resourceGroup = "rg-jogos-fiap"
$appName = "jogos-azfunc-fzgzbhbsbuejc2bk"
$subscriptionId = az account show --query id -o tsv

# App Settings Values
$postgres = "Host=dpg-d32oka3uibrs73a1qk0g-a.oregon-postgres.render.com;Port=5432;Database=fcg_games_db;Username=usr_fcg;Password=RTfpEwPmwVkGMjF5ObSgpLbtfBhIDigG;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Maximum Pool Size=50"
$sbConn = "Endpoint=sb://rg-fiap-jogos-prod.servicebus.windows.net/;SharedAccessKeyName=apidejogos;SharedAccessKey=FZKeKOg8vjcNp5ahOD0dwD5fRPSXKNCQ/+ASbIMjCno="
$sbQueue = "jogos-outbox"
$esUri = "https://api-jogos-es-ad94fa.es.eastus.azure.elastic.cloud"
$esKey = "bVltQWlwc0JvUERqTW4zMXVqdDU6QTNUY054QmN6Sjc5QkpoSEJFZUdkdw=="
$esIndex = "jogos"

Write-Host "Configuring App Settings for $appName..."
az functionapp config appsettings set --name $appName --resource-group $resourceGroup --settings "Postgres=$postgres" "ServiceBus:ConnectionString=$sbConn" "ServiceBus:QueueName=$sbQueue" "Elasticsearch:Uri=$esUri" "Elasticsearch:ApiKey=$esKey" "Elasticsearch:IndexName=$esIndex"

Write-Host "Creating Service Principal for GitHub Actions..."
$spName = "sp-github-actions-jogos"
# Create SP with Contributor role on the Resource Group and output JSON for GitHub Secret
$spJson = az ad sp create-for-rbac --name $spName --role contributor --scopes /subscriptions/$subscriptionId/resourceGroups/$resourceGroup --sdk-auth

Write-Host "--------------------------------------------------"
Write-Host "CONFIGURATION COMPLETE"
Write-Host "--------------------------------------------------"
Write-Host "1. App Settings have been updated in Azure."
Write-Host "2. Copy the JSON content below:"
Write-Host ""
Write-Host $spJson
Write-Host ""
Write-Host "3. Go to your GitHub Repository -> Settings -> Secrets and variables -> Actions"
Write-Host "4. Create a New Repository Secret named 'AZURE_CREDENTIALS' and paste the JSON content."
Write-Host "--------------------------------------------------"

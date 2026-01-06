$resourceGroup = "rg-jogos-fiap"
$location = "eastus2"
$storageName = "stjogosfiap$((Get-Random -Minimum 1000 -Maximum 9999))"
$planName = "plan-jogos-fiap"
$appName = "func-jogos-fiap-$((Get-Random -Minimum 1000 -Maximum 9999))"

Write-Host "Creating Resource Group..."
az group create --name $resourceGroup --location $location

Write-Host "Creating Storage Account..."
az storage account create --name $storageName --resource-group $resourceGroup --location $location --sku Standard_LRS

Write-Host "Creating App Service Plan..."
az functionapp plan create --name $planName --resource-group $resourceGroup --location $location --sku Y1 --is-linux

Write-Host "Creating Function App..."
az functionapp create --name $appName --storage-account $storageName --resource-group $resourceGroup --plan $planName --runtime dotnet-isolated --runtime-version 8.0 --functions-version 4 --os-type Linux

Write-Host "Publishing..."
dotnet publish src/FCG.TechChallenge.Jogos.Functions/FCG.TechChallenge.Jogos.Functions.csproj -c Release -o ./publish
Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force
az functionapp deployment source config-zip --resource-group $resourceGroup --name $appName --src ./publish.zip

$url = "https://$appName.azurewebsites.net"
Write-Host "--------------------------------------------------"
Write-Host "Deployment Complete!"
Write-Host "Function App URL: $url"
Write-Host "Please update src/FCG.TechChallenge.Jogos.Api/appsettings.json with this URL in 'PaymentServiceUrl'"
Write-Host "--------------------------------------------------"

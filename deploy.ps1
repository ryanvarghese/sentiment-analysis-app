# Azure App Service Deployment Script
# Run this script to deploy your sentiment analysis application

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$AppServiceName,
    
    [Parameter(Mandatory=$true)]
    [string]$Location = "East US"
)

Write-Host "Starting deployment to Azure App Service..." -ForegroundColor Green

# 1. Create Resource Group (if it doesn't exist)
Write-Host "Creating resource group: $ResourceGroupName" -ForegroundColor Yellow
az group create --name $ResourceGroupName --location $Location

# 2. Create App Service Plan
Write-Host "Creating App Service Plan..." -ForegroundColor Yellow
az appservice plan create --name "$AppServiceName-plan" --resource-group $ResourceGroupName --sku B1 --is-linux

# 3. Create Web App
Write-Host "Creating Web App: $AppServiceName" -ForegroundColor Yellow
az webapp create --resource-group $ResourceGroupName --plan "$AppServiceName-plan" --name $AppServiceName --runtime "DOTNET|8.0"

# 4. Configure Application Settings
Write-Host "Configuring application settings..." -ForegroundColor Yellow
Write-Host "Please update the following settings in Azure Portal:" -ForegroundColor Cyan
Write-Host "1. CosmosDb:ConnectionString" -ForegroundColor White
Write-Host "2. AzureCognitiveServices:Endpoint" -ForegroundColor White
Write-Host "3. AzureCognitiveServices:ApiKey" -ForegroundColor White
Write-Host "4. OpenAI:ApiKey" -ForegroundColor White

# 5. Deploy the application
Write-Host "Deploying application..." -ForegroundColor Yellow
az webapp deployment source config --name $AppServiceName --resource-group $ResourceGroupName --repo-url "https://github.com/YOUR_USERNAME/YOUR_REPO.git" --branch main --manual-integration

Write-Host "Deployment completed!" -ForegroundColor Green
Write-Host "Your app will be available at: https://$AppServiceName.azurewebsites.net" -ForegroundColor Cyan
Write-Host "Don't forget to configure your application settings in the Azure Portal!" -ForegroundColor Red



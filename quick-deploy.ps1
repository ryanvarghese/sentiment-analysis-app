# Quick Azure App Service Deployment Script
# Run: .\quick-deploy.ps1

param(
    [string]$ResourceGroupName = "sentiment-analysis-rg",
    [string]$AppName = "sentiment-analysis-$(Get-Date -Format 'yyyyMMddHHmmss')",
    [string]$Location = "East US"
)

Write-Host "🚀 Starting Azure App Service Deployment..." -ForegroundColor Green

$PlanName = "sentiment-analysis-plan"

Write-Host "📋 Configuration:" -ForegroundColor Blue
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor White
Write-Host "App Name: $AppName" -ForegroundColor White
Write-Host "Location: $Location" -ForegroundColor White
Write-Host ""

# Check if user is logged in to Azure
Write-Host "🔐 Checking Azure login..." -ForegroundColor Yellow
try {
    $account = az account show 2>$null | ConvertFrom-Json
    if (-not $account) {
        throw "Not logged in"
    }
    Write-Host "✅ Logged in to Azure" -ForegroundColor Green
} catch {
    Write-Host "❌ Not logged in to Azure. Please run: az login" -ForegroundColor Red
    exit 1
}

# Create resource group
Write-Host "📦 Creating resource group..." -ForegroundColor Yellow
az group create --name $ResourceGroupName --location $Location --output none
Write-Host "✅ Resource group created" -ForegroundColor Green

# Create App Service Plan
Write-Host "🏗️ Creating App Service Plan..." -ForegroundColor Yellow
az appservice plan create --name $PlanName --resource-group $ResourceGroupName --sku B1 --is-linux --output none
Write-Host "✅ App Service Plan created" -ForegroundColor Green

# Create Web App
Write-Host "🌐 Creating Web App..." -ForegroundColor Yellow
az webapp create --resource-group $ResourceGroupName --plan $PlanName --name $AppName --runtime "DOTNET|8.0" --output none
Write-Host "✅ Web App created" -ForegroundColor Green

# Build and deploy
Write-Host "🔨 Building application..." -ForegroundColor Yellow
dotnet publish -c Release -o ./publish --no-restore
Write-Host "✅ Application built" -ForegroundColor Green

Write-Host "📤 Deploying to Azure..." -ForegroundColor Yellow
Set-Location ./publish
Compress-Archive -Path * -DestinationPath ../deploy.zip -Force
Set-Location ..

az webapp deployment source config-zip --name $AppName --resource-group $ResourceGroupName --src ./deploy.zip --output none
Write-Host "✅ Application deployed" -ForegroundColor Green

# Clean up
Remove-Item -Recurse -Force ./publish -ErrorAction SilentlyContinue
Remove-Item -Force ./deploy.zip -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "🎉 Deployment Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📱 Your app is available at:" -ForegroundColor Blue
Write-Host "https://$AppName.azurewebsites.net" -ForegroundColor Green
Write-Host ""
Write-Host "⚠️  Next Steps:" -ForegroundColor Yellow
Write-Host "1. Configure your API keys in Azure Portal:" -ForegroundColor White
Write-Host "   - Go to: https://portal.azure.com" -ForegroundColor White
Write-Host "   - Navigate to your App Service" -ForegroundColor White
Write-Host "   - Go to Configuration > Application settings" -ForegroundColor White
Write-Host "   - Add your API keys:" -ForegroundColor White
Write-Host "     • CosmosDb:ConnectionString" -ForegroundColor White
Write-Host "     • AzureCognitiveServices:Endpoint" -ForegroundColor White
Write-Host "     • AzureCognitiveServices:ApiKey" -ForegroundColor White
Write-Host "     • OpenAI:ApiKey" -ForegroundColor White
Write-Host ""
Write-Host "2. Upload your CSV files to the app" -ForegroundColor White
Write-Host "3. Test all three analysis approaches" -ForegroundColor White
Write-Host ""
Write-Host "🔧 Useful Commands:" -ForegroundColor Blue
Write-Host "View logs: az webapp log tail --name $AppName --resource-group $ResourceGroupName" -ForegroundColor White
Write-Host "Restart app: az webapp restart --name $AppName --resource-group $ResourceGroupName" -ForegroundColor White
Write-Host "Delete resources: az group delete --name $ResourceGroupName --yes" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Happy analyzing!" -ForegroundColor Green



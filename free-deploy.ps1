# FREE Azure App Service Deployment Script
# Uses F1 (Free) tier - 1 hour/day limit

param(
    [string]$ResourceGroupName = "sentiment-analysis-free-rg",
    [string]$AppName = "sentiment-analysis-free-$(Get-Date -Format 'yyyyMMddHHmmss')",
    [string]$Location = "East US"
)

Write-Host "🆓 Starting FREE Azure App Service Deployment..." -ForegroundColor Green
Write-Host "⚠️  Note: F1 tier has 1 hour/day limit" -ForegroundColor Yellow

$PlanName = "sentiment-analysis-free-plan"

Write-Host "📋 Configuration:" -ForegroundColor Blue
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor White
Write-Host "App Name: $AppName" -ForegroundColor White
Write-Host "Location: $Location" -ForegroundColor White
Write-Host "Tier: F1 (FREE)" -ForegroundColor Green
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

# Create App Service Plan (F1 FREE tier)
Write-Host "🏗️ Creating FREE App Service Plan (F1 tier)..." -ForegroundColor Yellow
az appservice plan create --name $PlanName --resource-group $ResourceGroupName --sku F1 --is-linux --output none
Write-Host "✅ FREE App Service Plan created" -ForegroundColor Green

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
Write-Host "🎉 FREE Deployment Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📱 Your FREE app is available at:" -ForegroundColor Blue
Write-Host "https://$AppName.azurewebsites.net" -ForegroundColor Green
Write-Host ""
Write-Host "⚠️  F1 (FREE) Tier Limitations:" -ForegroundColor Yellow
Write-Host "• 1 hour of execution time per day" -ForegroundColor White
Write-Host "• 1 GB RAM" -ForegroundColor White
Write-Host "• 1 GB storage" -ForegroundColor White
Write-Host "• No custom domains" -ForegroundColor White
Write-Host "• No SSL certificates" -ForegroundColor White
Write-Host ""
Write-Host "🔧 Next Steps:" -ForegroundColor Blue
Write-Host "1. Configure your API keys in Azure Portal" -ForegroundColor White
Write-Host "2. Test your app (remember the 1 hour/day limit!)" -ForegroundColor White
Write-Host "3. Upgrade to B1 tier ($13/month) for production use" -ForegroundColor White
Write-Host ""
Write-Host "💰 To upgrade to production tier:" -ForegroundColor Cyan
Write-Host "az appservice plan update --name $PlanName --resource-group $ResourceGroupName --sku B1" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Your FREE sentiment analysis app is ready!" -ForegroundColor Green



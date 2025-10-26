# Create Azure Service Principal for GitHub Actions
# This script will create the credentials needed for GitHub Actions to deploy to Azure

Write-Host "🔧 Creating Azure Service Principal for GitHub Actions..." -ForegroundColor Green

# Check if user is logged in to Azure
Write-Host "`n1. Checking Azure login status..." -ForegroundColor Yellow
$loginStatus = az account show 2>$null
if (-not $loginStatus) {
    Write-Host "❌ Not logged in to Azure. Please run 'az login' first." -ForegroundColor Red
    Write-Host "Run: az login" -ForegroundColor Cyan
    exit 1
}

Write-Host "✅ Logged in to Azure" -ForegroundColor Green

# Get subscription info
Write-Host "`n2. Getting subscription information..." -ForegroundColor Yellow
$subscription = az account show --query "{subscriptionId:id, subscriptionName:name}" -o json | ConvertFrom-Json
Write-Host "Subscription: $($subscription.subscriptionName)" -ForegroundColor Cyan
Write-Host "Subscription ID: $($subscription.subscriptionId)" -ForegroundColor Cyan

# Get resource group (you'll need to specify this)
Write-Host "`n3. Getting resource groups..." -ForegroundColor Yellow
$resourceGroups = az group list --query "[].{Name:name, Location:location}" -o table
Write-Host $resourceGroups

$resourceGroupName = Read-Host "`nEnter the resource group name where your App Service is located"

# Verify resource group exists
$rgExists = az group show --name $resourceGroupName 2>$null
if (-not $rgExists) {
    Write-Host "❌ Resource group '$resourceGroupName' not found." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Resource group '$resourceGroupName' found" -ForegroundColor Green

# Create service principal
Write-Host "`n4. Creating Azure Service Principal..." -ForegroundColor Yellow
$spName = "github-actions-sentiment-analysis-$(Get-Date -Format 'yyyyMMdd')"

Write-Host "Creating service principal: $spName" -ForegroundColor Cyan

$credentials = az ad sp create-for-rbac --name $spName --role contributor --scopes "/subscriptions/$($subscription.subscriptionId)/resourceGroups/$resourceGroupName" --sdk-auth

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to create service principal" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Service principal created successfully!" -ForegroundColor Green

# Display the credentials
Write-Host "`n5. Azure Credentials (copy this JSON):" -ForegroundColor Yellow
Write-Host "=" * 60 -ForegroundColor Yellow
Write-Host $credentials -ForegroundColor White
Write-Host "=" * 60 -ForegroundColor Yellow

Write-Host "`n6. Next Steps:" -ForegroundColor Yellow
Write-Host "1. Copy the JSON above" -ForegroundColor Cyan
Write-Host "2. Go to your GitHub repository" -ForegroundColor Cyan
Write-Host "3. Go to Settings > Secrets and variables > Actions" -ForegroundColor Cyan
Write-Host "4. Click 'New repository secret'" -ForegroundColor Cyan
Write-Host "5. Name: AZURE_CREDENTIALS" -ForegroundColor Cyan
Write-Host "6. Value: Paste the JSON above" -ForegroundColor Cyan
Write-Host "7. Click 'Add secret'" -ForegroundColor Cyan

Write-Host "`n🎉 Setup complete! Your GitHub Actions should now be able to deploy to Azure." -ForegroundColor Green

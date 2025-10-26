# Get Azure App Service Publish Profile using Azure CLI
Write-Host "Getting Azure App Service Publish Profile..." -ForegroundColor Green

# Check if logged in
$loginCheck = az account show 2>$null
if (-not $loginCheck) {
    Write-Host "Please run 'az login' first" -ForegroundColor Red
    exit 1
}

Write-Host "Getting subscription info..." -ForegroundColor Yellow
$sub = az account show --query '{"id":"id","name":"name"}' -o json | ConvertFrom-Json
Write-Host "Subscription: $($sub.name)" -ForegroundColor Cyan

# Get App Service info
Write-Host "`nGetting App Service information..." -ForegroundColor Yellow
$appName = "sentiment-analysis-ryan-20241215"
$resourceGroup = "rg-sentimentanalyzer-proj1-dev-001"

Write-Host "App Service: $appName" -ForegroundColor Cyan
Write-Host "Resource Group: $resourceGroup" -ForegroundColor Cyan

# Get publish profile
Write-Host "`nDownloading publish profile..." -ForegroundColor Yellow
$publishProfile = az webapp deployment list-publishing-profiles --name $appName --resource-group $resourceGroup --xml

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nSUCCESS! Copy this XML to GitHub Secrets:" -ForegroundColor Green
    Write-Host "=" * 60 -ForegroundColor Yellow
    Write-Host $publishProfile -ForegroundColor White
    Write-Host "=" * 60 -ForegroundColor Yellow
    Write-Host "`nAdd this as a GitHub Secret named: AZUREAPPSERVICE_PUBLISHPROFILE" -ForegroundColor Cyan
    Write-Host "`nSteps:" -ForegroundColor Yellow
    Write-Host "1. Go to: https://github.com/ryanvarghese/sentiment-analysis-app/settings/secrets/actions" -ForegroundColor Cyan
    Write-Host "2. Click 'New repository secret'" -ForegroundColor Cyan
    Write-Host "3. Name: AZUREAPPSERVICE_PUBLISHPROFILE" -ForegroundColor Cyan
    Write-Host "4. Value: Paste the XML above" -ForegroundColor Cyan
    Write-Host "5. Click 'Add secret'" -ForegroundColor Cyan
} else {
    Write-Host "Failed to get publish profile" -ForegroundColor Red
    Write-Host "Trying alternative method..." -ForegroundColor Yellow
    
    # Alternative: Get app settings and create basic auth
    Write-Host "`nGetting App Service details..." -ForegroundColor Yellow
    $appDetails = az webapp show --name $appName --resource-group $resourceGroup --query '{"name":"name","resourceGroup":"resourceGroup","defaultHostName":"defaultHostName"}' -o json | ConvertFrom-Json
    Write-Host "App Name: $($appDetails.name)" -ForegroundColor Cyan
    Write-Host "Default Host: $($appDetails.defaultHostName)" -ForegroundColor Cyan
    
    Write-Host "`nYou can also try:" -ForegroundColor Yellow
    Write-Host "1. Go to Azure Portal" -ForegroundColor Cyan
    Write-Host "2. Navigate to your App Service" -ForegroundColor Cyan
    Write-Host "3. Go to 'Deployment Center'" -ForegroundColor Cyan
    Write-Host "4. Click 'Download publish profile'" -ForegroundColor Cyan
    Write-Host "5. If that fails, try 'Get publish profile' button" -ForegroundColor Cyan
}

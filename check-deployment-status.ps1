# Check Azure App Service Deployment Status
Write-Host "Checking Azure App Service deployment status..." -ForegroundColor Green

$appName = "sentiment-analysis-ryan-20241215"
$resourceGroup = "rg-sentimentanalyzer-proj1-dev-001"

Write-Host "`n1. Checking deployment history..." -ForegroundColor Yellow
Write-Host "App: $appName" -ForegroundColor Cyan
Write-Host "Resource Group: $resourceGroup" -ForegroundColor Cyan

# Get deployment history
Write-Host "`nGetting deployment history..." -ForegroundColor Yellow
$deployments = az webapp deployment list --name $appName --resource-group $resourceGroup --query '[].{"id":"id","status":"status","author":"author","deployer":"deployer","deploy_time":"deploy_time","message":"message"}' -o table

if ($deployments) {
    Write-Host $deployments -ForegroundColor White
} else {
    Write-Host "No deployment history found or error occurred" -ForegroundColor Yellow
}

Write-Host "`n2. Checking current app settings..." -ForegroundColor Yellow
$appSettings = az webapp config appsettings list --name $appName --resource-group $resourceGroup --query '[?name==`WEBSITE_RUN_FROM_PACKAGE`]' -o table

if ($appSettings) {
    Write-Host "WEBSITE_RUN_FROM_PACKAGE setting:" -ForegroundColor Cyan
    Write-Host $appSettings -ForegroundColor White
} else {
    Write-Host "No WEBSITE_RUN_FROM_PACKAGE setting found" -ForegroundColor Yellow
}

Write-Host "`n3. Checking app logs..." -ForegroundColor Yellow
Write-Host "To check logs manually:" -ForegroundColor Cyan
Write-Host "1. Go to Azure Portal" -ForegroundColor Cyan
Write-Host "2. Navigate to your App Service" -ForegroundColor Cyan
Write-Host "3. Go to 'Log stream' or 'Logs'" -ForegroundColor Cyan
Write-Host "4. Look for recent deployment messages" -ForegroundColor Cyan

Write-Host "`n4. Manual verification steps:" -ForegroundColor Yellow
Write-Host "- Check if the app shows recent changes" -ForegroundColor Cyan
Write-Host "- Look for any 'Deployment successful' messages in logs" -ForegroundColor Cyan
Write-Host "- Verify the app version matches your latest code" -ForegroundColor Cyan

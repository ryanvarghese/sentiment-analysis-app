# Create Azure Service Principal with limited scope
Write-Host "Creating Azure Service Principal with limited scope..." -ForegroundColor Green

# Check if logged in
$loginCheck = az account show 2>$null
if (-not $loginCheck) {
    Write-Host "Please run 'az login' first" -ForegroundColor Red
    exit 1
}

Write-Host "Getting subscription info..." -ForegroundColor Yellow
$sub = az account show --query '{"id":"id","name":"name"}' -o json | ConvertFrom-Json
Write-Host "Subscription: $($sub.name)" -ForegroundColor Cyan

# Create service principal with limited scope (just the app service)
Write-Host "`nCreating service principal with limited scope..." -ForegroundColor Yellow
$spName = "github-actions-sentiment-$(Get-Date -Format 'yyyyMMdd')"
$appId = "sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by"

Write-Host "App Service ID: $appId" -ForegroundColor Cyan

# Try to create service principal with just the app service scope
$creds = az ad sp create-for-rbac --name $spName --role contributor --scopes "/subscriptions/$($sub.id)/resourceGroups/rg-sentimentanalyzer-proj1-dev-001/providers/Microsoft.Web/sites/$appId" --sdk-auth

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nSUCCESS! Copy this JSON to GitHub Secrets:" -ForegroundColor Green
    Write-Host "=" * 60 -ForegroundColor Yellow
    Write-Host $creds -ForegroundColor White
    Write-Host "=" * 60 -ForegroundColor Yellow
    Write-Host "`nAdd this as a GitHub Secret named: AZURE_CREDENTIALS" -ForegroundColor Cyan
    Write-Host "`nSteps:" -ForegroundColor Yellow
    Write-Host "1. Go to: https://github.com/ryanvarghese/sentiment-analysis-app/settings/secrets/actions" -ForegroundColor Cyan
    Write-Host "2. Click 'New repository secret'" -ForegroundColor Cyan
    Write-Host "3. Name: AZURE_CREDENTIALS" -ForegroundColor Cyan
    Write-Host "4. Value: Paste the JSON above" -ForegroundColor Cyan
    Write-Host "5. Click 'Add secret'" -ForegroundColor Cyan
} else {
    Write-Host "`nFailed to create service principal. Trying alternative method..." -ForegroundColor Yellow
    
    # Alternative: Try with just the resource group scope
    Write-Host "Trying with resource group scope..." -ForegroundColor Yellow
    $creds = az ad sp create-for-rbac --name $spName --role contributor --scopes "/subscriptions/$($sub.id)/resourceGroups/rg-sentimentanalyzer-proj1-dev-001" --sdk-auth
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nSUCCESS with resource group scope! Copy this JSON:" -ForegroundColor Green
        Write-Host "=" * 60 -ForegroundColor Yellow
        Write-Host $creds -ForegroundColor White
        Write-Host "=" * 60 -ForegroundColor Yellow
    } else {
        Write-Host "`nStill failed. You may need to ask your Azure admin for permissions." -ForegroundColor Red
        Write-Host "`nAlternative: Use the publish profile method with the correct XML:" -ForegroundColor Yellow
        Write-Host "1. Copy the XML from fresh-publish-profile.xml" -ForegroundColor Cyan
        Write-Host "2. Add it as AZUREAPPSERVICE_PUBLISHPROFILE secret" -ForegroundColor Cyan
    }
}

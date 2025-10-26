# Simple Azure Service Principal Creation Script
Write-Host "Creating Azure Service Principal for GitHub Actions..." -ForegroundColor Green

# Check if logged in
$loginCheck = az account show 2>$null
if (-not $loginCheck) {
    Write-Host "Please run 'az login' first" -ForegroundColor Red
    exit 1
}

Write-Host "Getting subscription info..." -ForegroundColor Yellow
$sub = az account show --query '{"id":"id","name":"name"}' -o json | ConvertFrom-Json
Write-Host "Subscription: $($sub.name)" -ForegroundColor Cyan
Write-Host "Subscription ID: $($sub.id)" -ForegroundColor Cyan

Write-Host "`nGetting resource groups..." -ForegroundColor Yellow
az group list --query '[].{"Name":"name","Location":"location"}' -o table

$rg = Read-Host "`nEnter resource group name"

Write-Host "`nCreating service principal..." -ForegroundColor Yellow
$spName = "github-actions-sentiment-$(Get-Date -Format 'yyyyMMdd')"
$creds = az ad sp create-for-rbac --name $spName --role contributor --scopes "/subscriptions/$($sub.id)/resourceGroups/$rg" --sdk-auth

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nSUCCESS! Copy this JSON to GitHub Secrets:" -ForegroundColor Green
    Write-Host "=" * 50 -ForegroundColor Yellow
    Write-Host $creds -ForegroundColor White
    Write-Host "=" * 50 -ForegroundColor Yellow
    Write-Host "`nAdd this as a GitHub Secret named: AZURE_CREDENTIALS" -ForegroundColor Cyan
} else {
    Write-Host "Failed to create service principal" -ForegroundColor Red
}

# Test Manual Deployment
Write-Host "Testing manual deployment to verify publish profile..." -ForegroundColor Green

# Check if logged in to Azure
$loginCheck = az account show 2>$null
if (-not $loginCheck) {
    Write-Host "Please run 'az login' first" -ForegroundColor Red
    exit 1
}

Write-Host "`n1. Building the application..." -ForegroundColor Yellow
dotnet publish SentimentAnalysis.csproj -c Release -o ./test-publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful!" -ForegroundColor Green

Write-Host "`n2. Testing deployment with publish profile..." -ForegroundColor Yellow

# Create a simple test file
$testFile = "./test-publish/test.txt"
"Test deployment file" | Out-File -FilePath $testFile -Encoding UTF8

Write-Host "`n3. Attempting to deploy using Azure CLI..." -ForegroundColor Yellow
Write-Host "App Name: sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by" -ForegroundColor Cyan

# Try to deploy using Azure CLI
az webapp deployment source config-zip --name sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by --resource-group rg-sentimentanalyzer-proj1-dev-001 --src ./test-publish.zip

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Manual deployment successful!" -ForegroundColor Green
    Write-Host "The publish profile is working correctly." -ForegroundColor Green
} else {
    Write-Host "❌ Manual deployment failed!" -ForegroundColor Red
    Write-Host "This suggests the issue is with the publish profile itself." -ForegroundColor Yellow
}

Write-Host "`n4. Check your app at:" -ForegroundColor Yellow
Write-Host "https://sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by.eastus-01.azurewebsites.net" -ForegroundColor Cyan

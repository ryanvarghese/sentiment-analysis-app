# Automated Deployment Script for Sentiment Analysis App
# Usage: powershell -ExecutionPolicy Bypass -File deploy.ps1

Write-Host "🚀 Starting automated deployment..." -ForegroundColor Green

# Step 1: Clean and build
Write-Host "🧹 Cleaning and building application..." -ForegroundColor Blue
dotnet clean SentimentAnalysis.csproj
dotnet publish SentimentAnalysis.csproj -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed! Please check the errors above." -ForegroundColor Red
    exit 1
}

# Step 2: Remove old deployment files
Write-Host "🗑️ Cleaning old deployment files..." -ForegroundColor Blue
Get-ChildItem -Path . -Name "deploy-*.zip" | Remove-Item -Force

# Step 3: Create deployment package
Write-Host "📦 Creating deployment package..." -ForegroundColor Blue
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$deployFile = "deploy-$timestamp.zip"

cd publish
Compress-Archive -Path * -DestinationPath "../$deployFile" -Force
cd ..

Write-Host "✅ Deployment package created: $deployFile" -ForegroundColor Green

# Step 4: Deploy to Azure
Write-Host "📤 Deploying to Azure App Service..." -ForegroundColor Blue
az webapp deploy --resource-group rg-sentimentanalyzer-proj1-dev-001 --name sentiment-analysis-ryan-20241215 --src-path $deployFile --type zip

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Deployment successful!" -ForegroundColor Green
    Write-Host "🌐 Your app is available at: https://sentiment-analysis-ryan-20241215.azurewebsites.net" -ForegroundColor Cyan
    
    # Clean up deployment file
    Write-Host "🧹 Cleaning up deployment file..." -ForegroundColor Blue
    Remove-Item $deployFile -Force
    Write-Host "✅ Cleanup complete!" -ForegroundColor Green
} else {
    Write-Host "❌ Automated deployment failed!" -ForegroundColor Red
    Write-Host "📁 Manual upload file: $deployFile" -ForegroundColor Yellow
    Write-Host "📤 Upload this file to Azure Portal → Deployment Center" -ForegroundColor Yellow
}

Write-Host "🎉 Deployment process completed!" -ForegroundColor Green
# Test CI/CD Deployment
Write-Host "Testing CI/CD Deployment..." -ForegroundColor Green

Write-Host "`n1. Making a small change to trigger CI/CD..." -ForegroundColor Yellow

# Add a timestamp to README to trigger a new build
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$readmeContent = @"
# Sentiment Analysis Application

A comprehensive sentiment analysis application that combines Azure Cognitive Services with ChatGPT for advanced review analysis.

🚀 **CI/CD Pipeline Active!** - Automated deployment to Azure App Service.
**Build triggered: $(Get-Date)**
**Test deployment: $timestamp**

## 🚀 Features
"@

$readmeContent | Out-File -FilePath "README.md" -Encoding UTF8

Write-Host "✅ Updated README with timestamp" -ForegroundColor Green

Write-Host "`n2. Committing and pushing changes..." -ForegroundColor Yellow
git add README.md
git commit -m "Test CI/CD deployment - $timestamp"
git push

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Changes pushed successfully!" -ForegroundColor Green
    Write-Host "`n3. Monitor the deployment:" -ForegroundColor Yellow
    Write-Host "   Go to: https://github.com/ryanvarghese/sentiment-analysis-app/actions" -ForegroundColor Cyan
    Write-Host "   Look for the new workflow run" -ForegroundColor Cyan
    Write-Host "   Watch the 'deploy' job" -ForegroundColor Cyan
    
    Write-Host "`n4. Check your live app:" -ForegroundColor Yellow
    Write-Host "   https://sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by.eastus-01.azurewebsites.net" -ForegroundColor Cyan
    
    Write-Host "`n5. What to look for:" -ForegroundColor Yellow
    Write-Host "   ✅ Build job should pass" -ForegroundColor Green
    Write-Host "   ✅ Deploy job should pass" -ForegroundColor Green
    Write-Host "   ✅ App should be accessible" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to push changes" -ForegroundColor Red
}

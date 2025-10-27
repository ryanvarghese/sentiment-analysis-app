# Test CI/CD with Correct Publish Profile
Write-Host "Testing CI/CD deployment with correct publish profile..." -ForegroundColor Green

Write-Host "`n1. Making a test change to trigger deployment..." -ForegroundColor Yellow

# Add a timestamp to verify the deployment
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$testMessage = "✅ CI/CD TEST SUCCESSFUL - $timestamp"

# Update the test banner in Program.cs
$programContent = Get-Content "Program.cs" -Raw
$programContent = $programContent -replace "🧪 CI/CD TEST - DEPLOYMENT VERIFIED! 🚀", $testMessage
$programContent | Out-File -FilePath "Program.cs" -Encoding UTF8

Write-Host "✅ Updated test banner with timestamp" -ForegroundColor Green

Write-Host "`n2. Committing and pushing changes..." -ForegroundColor Yellow
git add Program.cs
git commit -m "🧪 TEST: Verify CI/CD with correct publish profile - $timestamp"
git push

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Changes pushed successfully!" -ForegroundColor Green
    Write-Host "`n3. Monitor the deployment:" -ForegroundColor Yellow
    Write-Host "   Go to: https://github.com/ryanvarghese/sentiment-analysis-app/actions" -ForegroundColor Cyan
    Write-Host "   Look for the new workflow run" -ForegroundColor Cyan
    Write-Host "   Watch the 'deploy' job - it should now PASS! ✅" -ForegroundColor Cyan
    
    Write-Host "`n4. Check your live app in 3-5 minutes:" -ForegroundColor Yellow
    Write-Host "   https://sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by.eastus-01.azurewebsites.net" -ForegroundColor Cyan
    Write-Host "   Look for the updated banner with timestamp" -ForegroundColor Cyan
    
    Write-Host "`n5. What to expect:" -ForegroundColor Yellow
    Write-Host "   ✅ Build job should pass" -ForegroundColor Green
    Write-Host "   ✅ Deploy job should now PASS (no more errors!)" -ForegroundColor Green
    Write-Host "   ✅ App should show the new timestamp banner" -ForegroundColor Green
    
    Write-Host "`n6. If deploy still fails:" -ForegroundColor Yellow
    Write-Host "   - Check the GitHub Actions logs" -ForegroundColor Cyan
    Write-Host "   - Verify the publish profile secret was updated correctly" -ForegroundColor Cyan
    Write-Host "   - Make sure Basic Auth is enabled in Azure" -ForegroundColor Cyan
} else {
    Write-Host "❌ Failed to push changes" -ForegroundColor Red
}

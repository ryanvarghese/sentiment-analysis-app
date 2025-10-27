# Monitor CI/CD Test Results
Write-Host "Monitoring CI/CD test results..." -ForegroundColor Green

Write-Host "`n1. GitHub Actions Status:" -ForegroundColor Yellow
Write-Host "   Go to: https://github.com/ryanvarghese/sentiment-analysis-app/actions" -ForegroundColor Cyan
Write-Host "   Look for the latest workflow run" -ForegroundColor Cyan
Write-Host "   Check if 'deploy' job shows ✅ (green checkmark)" -ForegroundColor Cyan

Write-Host "`n2. Test Your Live App:" -ForegroundColor Yellow
$appUrl = "https://sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by.eastus-01.azurewebsites.net"
Write-Host "   App URL: $appUrl" -ForegroundColor Cyan

Write-Host "`n3. What to Look For:" -ForegroundColor Yellow
Write-Host "   - Red banner at the top of the page" -ForegroundColor Cyan
Write-Host "   - Banner should show: '✅ CI/CD TEST SUCCESSFUL - [timestamp]'" -ForegroundColor Cyan
Write-Host "   - If you see the timestamp, CI/CD is working! 🎉" -ForegroundColor Cyan

Write-Host "`n4. Testing the app now..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $appUrl -Method GET -TimeoutSec 30
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ App is accessible! Status: $($response.StatusCode)" -ForegroundColor Green
        
        # Check if the response contains our test banner
        if ($response.Content -like "*CI/CD TEST SUCCESSFUL*") {
            Write-Host "🎉 SUCCESS! CI/CD is working! The test banner is visible!" -ForegroundColor Green
        } else {
            Write-Host "⚠️ App is running but test banner not visible yet" -ForegroundColor Yellow
            Write-Host "   - Deployment might still be in progress" -ForegroundColor Cyan
            Write-Host "   - Wait 2-3 more minutes and refresh" -ForegroundColor Cyan
        }
    } else {
        Write-Host "⚠️ App responded with status: $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ App is not accessible: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   - Check GitHub Actions for deployment status" -ForegroundColor Cyan
    Write-Host "   - Deployment might still be in progress" -ForegroundColor Cyan
}

Write-Host "`n5. Next Steps:" -ForegroundColor Yellow
Write-Host "   - If banner appears with timestamp: CI/CD is working! 🎉" -ForegroundColor Green
Write-Host "   - If no banner: Check GitHub Actions logs for errors" -ForegroundColor Cyan
Write-Host "   - If deploy fails: Verify publish profile secret is correct" -ForegroundColor Cyan

# Test Live Application
Write-Host "Testing your live application..." -ForegroundColor Green

$appUrl = "https://sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by.eastus-01.azurewebsites.net"

Write-Host "`n1. Testing app accessibility..." -ForegroundColor Yellow
Write-Host "App URL: $appUrl" -ForegroundColor Cyan

try {
    $response = Invoke-WebRequest -Uri $appUrl -Method GET -TimeoutSec 30
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ App is accessible! Status: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "✅ CI/CD deployment successful!" -ForegroundColor Green
    } else {
        Write-Host "⚠️ App responded with status: $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ App is not accessible: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "This might mean:" -ForegroundColor Yellow
    Write-Host "  - Deployment is still in progress" -ForegroundColor Cyan
    Write-Host "  - There's an issue with the deployment" -ForegroundColor Cyan
    Write-Host "  - App is not running" -ForegroundColor Cyan
}

Write-Host "`n2. Manual testing steps:" -ForegroundColor Yellow
Write-Host "  - Open the app URL in your browser" -ForegroundColor Cyan
Write-Host "  - Check if the sentiment analysis interface loads" -ForegroundColor Cyan
Write-Host "  - Try selecting a location from the dropdown" -ForegroundColor Cyan
Write-Host "  - Test the ChatGPT analysis button" -ForegroundColor Cyan

Write-Host "`n3. If the app doesn't work:" -ForegroundColor Yellow
Write-Host "  - Check GitHub Actions logs for errors" -ForegroundColor Cyan
Write-Host "  - Verify the publish profile secret is updated" -ForegroundColor Cyan
Write-Host "  - Check Azure App Service logs" -ForegroundColor Cyan

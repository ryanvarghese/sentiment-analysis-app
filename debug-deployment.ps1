# Debug GitHub Actions Deployment
Write-Host "Debugging GitHub Actions deployment..." -ForegroundColor Green

Write-Host "`n1. Checking GitHub Actions status..." -ForegroundColor Yellow
Write-Host "Go to: https://github.com/ryanvarghese/sentiment-analysis-app/actions" -ForegroundColor Cyan

Write-Host "`n2. Common deployment failure reasons:" -ForegroundColor Yellow
Write-Host "  - Publish profile secret is incorrect" -ForegroundColor Cyan
Write-Host "  - App name mismatch" -ForegroundColor Cyan
Write-Host "  - Package path issues" -ForegroundColor Cyan
Write-Host "  - Authentication problems" -ForegroundColor Cyan

Write-Host "`n3. Let's verify the publish profile secret..." -ForegroundColor Yellow
Write-Host "Current publish profile content:" -ForegroundColor Cyan
Write-Host "=" * 50 -ForegroundColor Yellow
Get-Content "correct-publish-profile.xml" -Raw
Write-Host "=" * 50 -ForegroundColor Yellow

Write-Host "`n4. Steps to fix:" -ForegroundColor Yellow
Write-Host "1. Go to GitHub Secrets:" -ForegroundColor Cyan
Write-Host "   https://github.com/ryanvarghese/sentiment-analysis-app/settings/secrets/actions" -ForegroundColor Cyan
Write-Host "2. Update AZUREAPPSERVICE_PUBLISHPROFILE with the content above" -ForegroundColor Cyan
Write-Host "3. Make sure it's EXACTLY as shown (one line, no extra spaces)" -ForegroundColor Cyan
Write-Host "4. Trigger a new deployment" -ForegroundColor Cyan

Write-Host "`n5. Alternative: Check if the issue is with the workflow..." -ForegroundColor Yellow
Write-Host "The workflow might be using the wrong package path or app name" -ForegroundColor Cyan

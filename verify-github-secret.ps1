# Verify GitHub Secret is Updated
Write-Host "Checking if GitHub secret needs to be updated..." -ForegroundColor Green

Write-Host "`n1. Current publish profile (from Azure):" -ForegroundColor Yellow
Write-Host "=" * 60 -ForegroundColor Yellow

# Get fresh publish profile
$freshProfile = az webapp deployment list-publishing-profiles --name sentiment-analysis-ryan-20241215 --resource-group rg-sentimentanalyzer-proj1-dev-001 --xml

Write-Host $freshProfile -ForegroundColor White
Write-Host "=" * 60 -ForegroundColor Yellow

Write-Host "`n2. Steps to update GitHub secret:" -ForegroundColor Yellow
Write-Host "1. Go to: https://github.com/ryanvarghese/sentiment-analysis-app/settings/secrets/actions" -ForegroundColor Cyan
Write-Host "2. Find 'AZUREAPPSERVICE_PUBLISHPROFILE' secret" -ForegroundColor Cyan
Write-Host "3. Click 'Update'" -ForegroundColor Cyan
Write-Host "4. DELETE all old content" -ForegroundColor Red
Write-Host "5. Copy the EXACT content above (one line)" -ForegroundColor Cyan
Write-Host "6. Paste it exactly" -ForegroundColor Cyan
Write-Host "7. Click 'Update secret'" -ForegroundColor Cyan

Write-Host "`n3. Common issues:" -ForegroundColor Yellow
Write-Host "- Secret contains old/incorrect publish profile" -ForegroundColor Cyan
Write-Host "- Extra spaces or line breaks in the secret" -ForegroundColor Cyan
Write-Host "- Secret was never updated after we fixed the app name" -ForegroundColor Cyan

Write-Host "`n4. After updating the secret:" -ForegroundColor Yellow
Write-Host "- Go to GitHub Actions" -ForegroundColor Cyan
Write-Host "- Click 'Re-run all jobs' on the latest workflow" -ForegroundColor Cyan
Write-Host "- Watch the deploy step" -ForegroundColor Cyan

# Verify GitHub Secret Update
Write-Host "Checking if you need to update the GitHub secret..." -ForegroundColor Green

Write-Host "`n1. Go to GitHub Secrets:" -ForegroundColor Yellow
Write-Host "   https://github.com/ryanvarghese/sentiment-analysis-app/settings/secrets/actions" -ForegroundColor Cyan

Write-Host "`n2. Find the secret named: AZUREAPPSERVICE_PUBLISHPROFILE" -ForegroundColor Yellow

Write-Host "`n3. Click 'Update' and verify the content matches this:" -ForegroundColor Yellow
Write-Host "   (Should start with <publishData> and end with </publishData>)" -ForegroundColor Cyan

Write-Host "`n4. If it doesn't match, copy the EXACT content from fresh-publish-profile.xml:" -ForegroundColor Yellow
Write-Host "   File: fresh-publish-profile.xml" -ForegroundColor Cyan

Write-Host "`n5. The content should be on ONE line, no line breaks" -ForegroundColor Yellow

Write-Host "`n6. After updating, trigger a new build:" -ForegroundColor Yellow
Write-Host "   - Go to Actions tab" -ForegroundColor Cyan
Write-Host "   - Click 'Re-run all jobs'" -ForegroundColor Cyan

Write-Host "`nCurrent publish profile content:" -ForegroundColor Green
Write-Host "=" * 50 -ForegroundColor Yellow
Get-Content "fresh-publish-profile.xml" -Raw
Write-Host "=" * 50 -ForegroundColor Yellow

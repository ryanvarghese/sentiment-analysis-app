# Get Real Publish Profile from Azure Portal
Write-Host "Getting real publish profile with actual credentials..." -ForegroundColor Green

Write-Host "`n1. Go to Azure Portal:" -ForegroundColor Yellow
Write-Host "   https://portal.azure.com" -ForegroundColor Cyan

Write-Host "`n2. Navigate to your App Service:" -ForegroundColor Yellow
Write-Host "   - Go to 'App Services'" -ForegroundColor Cyan
Write-Host "   - Click on 'sentiment-analysis-ryan-20241215'" -ForegroundColor Cyan

Write-Host "`n3. Download Publish Profile:" -ForegroundColor Yellow
Write-Host "   - Go to 'Deployment Center' (in the left menu)" -ForegroundColor Cyan
Write-Host "   - Click 'Download publish profile' button" -ForegroundColor Cyan
Write-Host "   - This will download a .PublishSettings file" -ForegroundColor Cyan

Write-Host "`n4. Open the downloaded file:" -ForegroundColor Yellow
Write-Host "   - Open the .PublishSettings file in Notepad or VS Code" -ForegroundColor Cyan
Write-Host "   - Copy the ENTIRE content (it will have real usernames/passwords)" -ForegroundColor Cyan

Write-Host "`n5. Update GitHub Secret:" -ForegroundColor Yellow
Write-Host "   - Go to: https://github.com/ryanvarghese/sentiment-analysis-app/settings/secrets/actions" -ForegroundColor Cyan
Write-Host "   - Find 'AZUREAPPSERVICE_PUBLISHPROFILE'" -ForegroundColor Cyan
Write-Host "   - Click 'Update'" -ForegroundColor Cyan
Write-Host "   - Paste the REAL content from the .PublishSettings file" -ForegroundColor Cyan
Write-Host "   - Click 'Update secret'" -ForegroundColor Cyan

Write-Host "`n6. Alternative - Try Azure CLI with different flags:" -ForegroundColor Yellow
Write-Host "   Let me try to get the real credentials..." -ForegroundColor Cyan

# Try different Azure CLI commands to get real credentials
Write-Host "`nTrying alternative Azure CLI methods..." -ForegroundColor Yellow

# Method 1: Try without --xml flag
Write-Host "Method 1: Without --xml flag" -ForegroundColor Cyan
$profile1 = az webapp deployment list-publishing-profiles --name sentiment-analysis-ryan-20241215 --resource-group rg-sentimentanalyzer-proj1-dev-001
if ($profile1 -and $profile1 -notlike "*REDACTED*") {
    Write-Host "SUCCESS! Found real credentials:" -ForegroundColor Green
    Write-Host $profile1 -ForegroundColor White
} else {
    Write-Host "Still shows REDACTED" -ForegroundColor Yellow
}

# Method 2: Try with different output format
Write-Host "`nMethod 2: Different output format" -ForegroundColor Cyan
$profile2 = az webapp deployment list-publishing-profiles --name sentiment-analysis-ryan-20241215 --resource-group rg-sentimentanalyzer-proj1-dev-001 --query '[0]' -o json
if ($profile2 -and $profile2 -notlike "*REDACTED*") {
    Write-Host "SUCCESS! Found real credentials:" -ForegroundColor Green
    Write-Host $profile2 -ForegroundColor White
} else {
    Write-Host "Still shows REDACTED" -ForegroundColor Yellow
}

Write-Host "`nIf both methods show REDACTED, you MUST use Azure Portal method above." -ForegroundColor Red

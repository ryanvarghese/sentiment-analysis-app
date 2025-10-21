# Azure App Service Deployment Diagnostic Script
# Run this to identify deployment issues

Write-Host "🔍 Azure App Service Deployment Diagnostic Tool" -ForegroundColor Green
Write-Host ""

# Check if deploy.zip exists and get its size
if (Test-Path "deploy.zip") {
    $zipSize = (Get-Item "deploy.zip").Length
    Write-Host "✅ deploy.zip found" -ForegroundColor Green
    Write-Host "📦 File size: $([math]::Round($zipSize/1MB, 2)) MB" -ForegroundColor Cyan
    
    if ($zipSize -gt 50MB) {
        Write-Host "⚠️  WARNING: File is larger than 50MB - this might cause deployment issues!" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ deploy.zip not found" -ForegroundColor Red
    Write-Host "Run: dotnet publish -c Release -o ./publish" -ForegroundColor Yellow
    Write-Host "Then: cd publish && Compress-Archive -Path * -DestinationPath ../deploy.zip -Force" -ForegroundColor Yellow
}

Write-Host ""

# Check if publish folder exists
if (Test-Path "publish") {
    Write-Host "✅ publish folder found" -ForegroundColor Green
    $publishFiles = Get-ChildItem "publish" -Recurse | Measure-Object
    Write-Host "📁 Files in publish folder: $($publishFiles.Count)" -ForegroundColor Cyan
} else {
    Write-Host "❌ publish folder not found" -ForegroundColor Red
}

Write-Host ""

# Check for common deployment issues
Write-Host "🔧 Common Deployment Issues & Solutions:" -ForegroundColor Blue
Write-Host ""
Write-Host "1. FILE SIZE ISSUE:" -ForegroundColor Yellow
Write-Host "   - If deploy.zip > 50MB, it might fail" -ForegroundColor White
Write-Host "   - Solution: Remove unnecessary files from publish folder" -ForegroundColor White
Write-Host ""
Write-Host "2. MISSING FILES:" -ForegroundColor Yellow
Write-Host "   - Check if all required files are in publish folder" -ForegroundColor White
Write-Host "   - Solution: Ensure appsettings.json, web.config are included" -ForegroundColor White
Write-Host ""
Write-Host "3. RUNTIME ISSUE:" -ForegroundColor Yellow
Write-Host "   - Make sure you selected .NET 8 and Linux" -ForegroundColor White
Write-Host "   - Solution: Recreate Web App with correct settings" -ForegroundColor White
Write-Host ""
Write-Host "4. PERMISSIONS ISSUE:" -ForegroundColor Yellow
Write-Host "   - Check if you have proper Azure permissions" -ForegroundColor White
Write-Host "   - Solution: Ensure you're the owner of the resource group" -ForegroundColor White
Write-Host ""
Write-Host "5. API KEYS ISSUE:" -ForegroundColor Yellow
Write-Host "   - Check if all required settings are configured" -ForegroundColor White
Write-Host "   - Solution: Verify all 4 API keys are set correctly" -ForegroundColor White
Write-Host ""

# Check publish folder contents
if (Test-Path "publish") {
    Write-Host "📋 Files in publish folder:" -ForegroundColor Blue
    Get-ChildItem "publish" | ForEach-Object {
        Write-Host "   - $($_.Name)" -ForegroundColor White
    }
    Write-Host ""
    
    # Check for critical files
    $criticalFiles = @("SentimentAnalysis.dll", "appsettings.json", "web.config")
    foreach ($file in $criticalFiles) {
        if (Test-Path "publish/$file") {
            Write-Host "✅ $file found" -ForegroundColor Green
        } else {
            Write-Host "❌ $file missing" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "🚀 Next Steps:" -ForegroundColor Green
Write-Host "1. Check Azure Portal → Your Web App → Deployment Center → Logs" -ForegroundColor White
Write-Host "2. Check Azure Portal → Your Web App → Monitoring → Log stream" -ForegroundColor White
Write-Host "3. Try redeploying with a smaller file if size is the issue" -ForegroundColor White
Write-Host "4. Verify all API keys are configured correctly" -ForegroundColor White
Write-Host ""
Write-Host "💡 Pro Tip: Use 'Deployment Center' → 'ZIP' method for easier deployment" -ForegroundColor Cyan



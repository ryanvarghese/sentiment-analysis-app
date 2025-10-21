Write-Host "Building and running Sentiment Analysis Data Processor..." -ForegroundColor Green
Write-Host ""

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Build successful! Starting application..." -ForegroundColor Green
Write-Host ""

# Run the application
dotnet run

Write-Host ""
Write-Host "Application completed." -ForegroundColor Green
Read-Host "Press Enter to exit"

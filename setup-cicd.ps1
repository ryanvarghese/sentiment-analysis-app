# CI/CD Setup Script for Sentiment Analysis Application
# This script helps you set up automated deployment with GitHub Actions

Write-Host "Setting up CI/CD for Sentiment Analysis Application" -ForegroundColor Green
Write-Host ""

# Check if git is initialized
if (-not (Test-Path ".git")) {
    Write-Host "Git repository not initialized. Please run 'git init' first." -ForegroundColor Red
    exit 1
}

Write-Host "Git repository found" -ForegroundColor Green

# Check if GitHub remote exists
$remote = git remote get-url origin 2>$null
if ($remote) {
    Write-Host "GitHub remote found: $remote" -ForegroundColor Green
} else {
    Write-Host "No GitHub remote found. You'll need to add one:" -ForegroundColor Yellow
    Write-Host "   git remote add origin https://github.com/YOUR_USERNAME/sentiment-analysis-app.git" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Create a GitHub repository named 'sentiment-analysis-app'" -ForegroundColor White
Write-Host "2. Push your code: git push -u origin main" -ForegroundColor White
Write-Host "3. Set up Azure service principal (see CI_CD_SETUP.md)" -ForegroundColor White
Write-Host "4. Configure GitHub secrets (see CI_CD_SETUP.md)" -ForegroundColor White
Write-Host "5. Update workflow files with your Azure details" -ForegroundColor White

Write-Host ""
Write-Host "For detailed instructions, see: CI_CD_SETUP.md" -ForegroundColor Cyan
Write-Host ""

# Show current status
Write-Host "Current Status:" -ForegroundColor Yellow
Write-Host "CI/CD workflow files created" -ForegroundColor Green
Write-Host ".gitignore configured" -ForegroundColor Green
Write-Host "Code committed to git" -ForegroundColor Green
Write-Host "Ready for GitHub push" -ForegroundColor Yellow

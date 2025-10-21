# GitHub Setup Script for Sentiment Analysis App
Write-Host "🚀 Setting up GitHub repository..." -ForegroundColor Green

# Check if Git is installed
try {
    git --version
    Write-Host "✅ Git is installed" -ForegroundColor Green
} catch {
    Write-Host "❌ Git is not installed. Please install Git first:" -ForegroundColor Red
    Write-Host "   Download from: https://git-scm.com/download/win" -ForegroundColor Yellow
    Write-Host "   Then restart your terminal and run this script again." -ForegroundColor Yellow
    exit 1
}

# Initialize Git repository
Write-Host "📁 Initializing Git repository..." -ForegroundColor Blue
git init

# Add all files (except those in .gitignore)
Write-Host "📝 Adding files to Git..." -ForegroundColor Blue
git add .

# Create initial commit
Write-Host "💾 Creating initial commit..." -ForegroundColor Blue
git commit -m "Initial commit: Secure Sentiment Analysis App

- Removed API keys from appsettings.json
- Added environment variable configuration
- Added .gitignore for security
- Ready for Azure deployment"

# Set main branch
Write-Host "🌿 Setting main branch..." -ForegroundColor Blue
git branch -M main

Write-Host "✅ Git repository initialized successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Create a repository on GitHub.com" -ForegroundColor White
Write-Host "2. Copy the repository URL" -ForegroundColor White
Write-Host "3. Run: git remote add origin <your-repo-url>" -ForegroundColor White
Write-Host "4. Run: git push -u origin main" -ForegroundColor White
Write-Host "5. Configure Azure App Service with your GitHub PAT" -ForegroundColor White

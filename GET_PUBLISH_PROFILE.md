# 🔧 Get Azure Publish Profile for GitHub Actions

## Step-by-Step Guide

### 1. Go to Azure Portal
- Open [portal.azure.com](https://portal.azure.com)
- Sign in with your Azure account

### 2. Navigate to Your App Service
- Go to **App Services** in the left menu
- Find and click on your app: `sentiment-analysis-ryan-20241215`

### 3. Download Publish Profile
- In your App Service, go to **Deployment Center** (in the left menu)
- Click on **"Download publish profile"** button
- This will download a `.PublishSettings` file

### 4. Open the Publish Profile
- Open the downloaded `.PublishSettings` file in a text editor (Notepad, VS Code, etc.)
- **Copy the entire XML content** (it should start with `<publishData>` and end with `</publishData>`)

### 5. Add to GitHub Secrets
- Go to your GitHub repository: `https://github.com/ryanvarghese/sentiment-analysis-app`
- Click **Settings** tab
- Click **Secrets and variables** → **Actions**
- Click **"New repository secret"**
- **Name:** `AZUREAPPSERVICE_PUBLISHPROFILE`
- **Value:** Paste the entire XML content from step 4
- Click **"Add secret"**

### 6. Test the Deployment
- Go to **Actions** tab in your GitHub repository
- Find the latest workflow run
- Click **"Re-run all jobs"** or push a new commit

## ✅ What This Fixes
- No need to create service principals
- Works with student Azure accounts
- Simpler authentication method
- More reliable for basic deployments

## 🎯 Expected Result
Your GitHub Actions should now successfully deploy to Azure App Service!

# CI/CD Setup Guide for Sentiment Analysis Application

This guide will help you set up automated deployment using GitHub Actions and Azure App Service.

## 🚀 Quick Start

### 1. Create GitHub Repository

1. Go to [GitHub](https://github.com) and create a new repository
2. Name it: `sentiment-analysis-app`
3. Make it **Public** (for free GitHub Actions minutes)
4. **Don't** initialize with README (we already have files)

### 2. Push Your Code

```bash
# Add all files
git add .

# Commit changes
git commit -m "Initial commit with CI/CD setup"

# Add your GitHub repository as remote
git remote add origin https://github.com/YOUR_USERNAME/sentiment-analysis-app.git

# Push to GitHub
git push -u origin main
```

### 3. Set Up Azure Service Principal

#### Option A: Using Azure CLI (Recommended)

```bash
# Login to Azure
az login

# Create service principal
az ad sp create-for-rbac --name "github-actions-sentiment-analysis" --role contributor --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} --sdk-auth
```

#### Option B: Using Azure Portal

1. Go to **Azure Portal** → **Azure Active Directory** → **App registrations**
2. Click **New registration**
3. Name: `github-actions-sentiment-analysis`
4. Click **Register**
5. Go to **Certificates & secrets** → **New client secret**
6. Copy the **Value** (not the Secret ID)
7. Go to **API permissions** → **Add a permission** → **Azure Service Management** → **Delegated permissions** → **user_impersonation**
8. Click **Grant admin consent**

### 4. Configure GitHub Secrets

Go to your GitHub repository → **Settings** → **Secrets and variables** → **Actions**

Add these secrets:

#### Required Secrets:

```
AZURE_CREDENTIALS
```
*Value: The JSON output from the service principal creation*

```
COSMOS_CONNECTION_STRING
```
*Value: Your Cosmos DB connection string*

```
AZURE_COGNITIVE_ENDPOINT
```
*Value: Your Azure Cognitive Services endpoint*

```
AZURE_COGNITIVE_KEY
```
*Value: Your Azure Cognitive Services API key*

```
OPENAI_API_KEY
```
*Value: Your OpenAI API key*

#### Optional (for publish profile method):

```
AZUREAPPSERVICE_PUBLISHPROFILE
```
*Value: Download from Azure App Service → Get publish profile*

### 5. Update Workflow Files

Edit `.github/workflows/deploy-advanced.yml` and update:

```yaml
env:
  AZURE_WEBAPP_NAME: your-app-name
  AZURE_RESOURCE_GROUP: your-resource-group
```

## 🔧 Workflow Options

### Option 1: Simple Deploy (deploy.yml)
- Uses publish profile
- Simpler setup
- Good for testing

### Option 2: Advanced Deploy (deploy-advanced.yml)
- Uses Azure service principal
- Automatically configures app settings
- More secure
- Production ready

### Option 3: Test Only (test.yml)
- Only builds and tests
- No deployment
- Good for pull requests

## 🚀 How It Works

1. **Push Code** → Triggers GitHub Action
2. **Build** → .NET application is built
3. **Test** → Tests are run (if any)
4. **Deploy** → Application is deployed to Azure
5. **Configure** → App settings are updated automatically

## 📋 Deployment Checklist

- [ ] GitHub repository created
- [ ] Code pushed to GitHub
- [ ] Azure service principal created
- [ ] GitHub secrets configured
- [ ] Workflow files updated with correct names
- [ ] First deployment triggered

## 🔍 Troubleshooting

### Common Issues:

1. **Authentication Failed**
   - Check `AZURE_CREDENTIALS` secret
   - Verify service principal has correct permissions

2. **App Settings Not Updated**
   - Check all required secrets are set
   - Verify resource group name in workflow

3. **Build Fails**
   - Check .NET version compatibility
   - Verify all dependencies are in .csproj

4. **Deployment Fails**
   - Check Azure App Service name
   - Verify resource group exists

### View Logs:

1. Go to GitHub repository → **Actions** tab
2. Click on the failed workflow
3. Check the logs for specific errors

## 🎯 Next Steps

1. **Set up monitoring** with Application Insights
2. **Add staging environment** for testing
3. **Set up branch protection** rules
4. **Add automated testing** with unit tests
5. **Configure notifications** for deployment status

## 📚 Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [.NET GitHub Actions](https://github.com/actions/setup-dotnet)
- [Azure Login Action](https://github.com/azure/login)

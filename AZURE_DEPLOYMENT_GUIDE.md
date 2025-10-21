# 🚀 Complete Azure App Service Deployment Guide

Deploy your Sentiment Analysis application to Azure App Service so anyone can access it!

## 📋 Prerequisites

- [ ] Azure subscription
- [ ] Azure CLI installed
- [ ] Git installed
- [ ] Your API keys ready

## 🎯 Quick Start (5 Minutes)

### Step 1: Create Azure Resources

```bash
# Login to Azure
az login

# Create resource group
az group create --name sentiment-analysis-rg --location "East US"

# Create App Service Plan (B1 tier - cheapest)
az appservice plan create --name sentiment-analysis-plan --resource-group sentiment-analysis-rg --sku B1 --is-linux

# Create Web App
az webapp create --resource-group sentiment-analysis-rg --plan sentiment-analysis-plan --name YOUR-APP-NAME --runtime "DOTNET|8.0"
```

### Step 2: Configure Application Settings

```bash
# Set your API keys (replace with your actual values)
az webapp config appsettings set --name YOUR-APP-NAME --resource-group sentiment-analysis-rg --settings \
  "CosmosDb:ConnectionString=YOUR_COSMOS_CONNECTION_STRING" \
  "AzureCognitiveServices:Endpoint=YOUR_TEXT_ANALYTICS_ENDPOINT" \
  "AzureCognitiveServices:ApiKey=YOUR_TEXT_ANALYTICS_KEY" \
  "OpenAI:ApiKey=YOUR_OPENAI_API_KEY"
```

### Step 3: Deploy Your Code

```bash
# Deploy from local folder
az webapp deployment source config-zip --name YOUR-APP-NAME --resource-group sentiment-analysis-rg --src ./deploy.zip
```

## 🔧 Detailed Deployment Options

### Option A: GitHub Integration (Recommended)

1. **Push to GitHub:**
```bash
git init
git add .
git commit -m "Initial commit"
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO.git
git push -u origin main
```

2. **Configure GitHub Deployment:**
```bash
az webapp deployment source config --name YOUR-APP-NAME --resource-group sentiment-analysis-rg --repo-url "https://github.com/YOUR_USERNAME/YOUR_REPO.git" --branch main --manual-integration
```

### Option B: Docker Container

1. **Build and push to Azure Container Registry:**
```bash
# Create ACR
az acr create --resource-group sentiment-analysis-rg --name YOUR-REGISTRY --sku Basic

# Build and push
az acr build --registry YOUR-REGISTRY --image sentiment-analysis:latest .
```

2. **Deploy from container:**
```bash
az webapp config container set --name YOUR-APP-NAME --resource-group sentiment-analysis-rg --docker-custom-image-name YOUR-REGISTRY.azurecr.io/sentiment-analysis:latest
```

### Option C: ZIP Deployment

1. **Create deployment package:**
```bash
dotnet publish -c Release -o ./publish
cd publish
zip -r ../deploy.zip .
```

2. **Deploy ZIP:**
```bash
az webapp deployment source config-zip --name YOUR-APP-NAME --resource-group sentiment-analysis-rg --src ./deploy.zip
```

## 🔐 Security Configuration

### Using Azure Key Vault (Recommended)

1. **Create Key Vault:**
```bash
az keyvault create --name YOUR-KEYVAULT --resource-group sentiment-analysis-rg --location "East US"
```

2. **Store secrets:**
```bash
az keyvault secret set --vault-name YOUR-KEYVAULT --name "CosmosDbConnectionString" --value "YOUR_CONNECTION_STRING"
az keyvault secret set --vault-name YOUR-KEYVAULT --name "OpenAIApiKey" --value "YOUR_OPENAI_KEY"
```

3. **Configure App Service to use Key Vault:**
```bash
az webapp config appsettings set --name YOUR-APP-NAME --resource-group sentiment-analysis-rg --settings \
  "@Microsoft.KeyVault(SecretUri=https://YOUR-KEYVAULT.vault.azure.net/secrets/CosmosDbConnectionString/)" \
  "@Microsoft.KeyVault(SecretUri=https://YOUR-KEYVAULT.vault.azure.net/secrets/OpenAIApiKey/)"
```

## 📊 Monitoring & Logging

### Enable Application Insights

```bash
# Create Application Insights
az monitor app-insights component create --app sentiment-analysis-insights --location "East US" --resource-group sentiment-analysis-rg

# Get instrumentation key
az monitor app-insights component show --app sentiment-analysis-insights --resource-group sentiment-analysis-rg --query instrumentationKey -o tsv
```

### Configure Logging

```bash
# Enable application logging
az webapp log config --name YOUR-APP-NAME --resource-group sentiment-analysis-rg --application-logging filesystem --level information

# Enable web server logging
az webapp log config --name YOUR-APP-NAME --resource-group sentiment-analysis-rg --web-server-logging filesystem
```

## 🚀 Performance Optimization

### Scale Your App

```bash
# Scale up (more powerful instance)
az appservice plan update --name sentiment-analysis-plan --resource-group sentiment-analysis-rg --sku P1V2

# Scale out (more instances)
az webapp scale --name YOUR-APP-NAME --resource-group sentiment-analysis-rg --instance-count 3
```

### Configure Auto-scaling

```bash
# Create auto-scale rule
az monitor autoscale create --resource-group sentiment-analysis-rg --resource YOUR-APP-NAME --resource-type Microsoft.Web/sites --name sentiment-analysis-autoscale --min-count 1 --max-count 3 --count 1
```

## 🌐 Custom Domain (Optional)

### Add Custom Domain

```bash
# Add domain to App Service
az webapp config hostname add --webapp-name YOUR-APP-NAME --resource-group sentiment-analysis-rg --hostname your-domain.com
```

### Configure SSL

```bash
# Enable HTTPS redirect
az webapp config set --name YOUR-APP-NAME --resource-group sentiment-analysis-rg --https-only true
```

## 💰 Cost Management

### Monitor Costs

```bash
# Get cost analysis
az consumption usage list --start-date 2024-01-01 --end-date 2024-01-31
```

### Set Budget Alerts

```bash
# Create budget
az consumption budget create --budget-name sentiment-analysis-budget --resource-group sentiment-analysis-rg --amount 50 --time-grain Monthly
```

## 🔧 Troubleshooting

### Common Issues & Solutions

1. **App won't start:**
```bash
# Check logs
az webapp log tail --name YOUR-APP-NAME --resource-group sentiment-analysis-rg

# Check application settings
az webapp config appsettings list --name YOUR-APP-NAME --resource-group sentiment-analysis-rg
```

2. **Database connection issues:**
```bash
# Test connection
az cosmosdb check-name-exists --name YOUR-COSMOS-ACCOUNT
```

3. **API rate limits:**
```bash
# Check usage
az monitor metrics list --resource YOUR-APP-NAME --resource-group sentiment-analysis-rg --metric "HttpRequests"
```

## 📱 Testing Your Deployment

### 1. Access Your App
```
https://YOUR-APP-NAME.azurewebsites.net
```

### 2. Test All Features
- [ ] Data import works
- [ ] ChatGPT analysis works
- [ ] Azure analysis works  
- [ ] Hybrid analysis works
- [ ] Results display correctly

### 3. Performance Test
- [ ] Load time < 5 seconds
- [ ] Analysis completes successfully
- [ ] No errors in logs

## 🎉 Success!

Your sentiment analysis application is now live and accessible to anyone on the internet!

**Your app URL:** `https://YOUR-APP-NAME.azurewebsites.net`

## 📞 Support

If you encounter issues:
1. Check Azure Portal logs
2. Review application settings
3. Verify API keys are correct
4. Check resource quotas

## 🔄 Updates & Maintenance

### Deploy Updates
```bash
# Redeploy from GitHub
az webapp deployment source sync --name YOUR-APP-NAME --resource-group sentiment-analysis-rg
```

### Backup Strategy
```bash
# Backup App Service
az webapp config backup create --resource-group sentiment-analysis-rg --webapp-name YOUR-APP-NAME --backup-name backup-$(date +%Y%m%d)
```

---

**🎊 Congratulations! Your sentiment analysis application is now deployed and ready for the world!**



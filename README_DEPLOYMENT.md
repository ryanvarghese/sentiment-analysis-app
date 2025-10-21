# Azure App Service Deployment Guide

This guide will help you deploy your Sentiment Analysis application to Azure App Service.

## Prerequisites

1. **Azure CLI** installed and logged in
2. **Git** installed
3. **Azure subscription** with appropriate permissions
4. **API Keys** for:
   - Azure Cosmos DB
   - Azure Text Analytics
   - OpenAI API

## Step 1: Prepare Your Application

### 1.1 Update Configuration
Update `appsettings.Production.json` with your production values:

```json
{
  "CosmosDb": {
    "ConnectionString": "YOUR_COSMOS_DB_CONNECTION_STRING",
    "DatabaseId": "SentimentAnalysisDB",
    "ContainerId": "Reviews"
  },
  "AzureCognitiveServices": {
    "Endpoint": "YOUR_AZURE_TEXT_ANALYTICS_ENDPOINT",
    "ApiKey": "YOUR_AZURE_TEXT_ANALYTICS_API_KEY"
  },
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY"
  }
}
```

### 1.2 Create GitHub Repository
1. Create a new repository on GitHub
2. Push your code to the repository
3. Note the repository URL

## Step 2: Deploy to Azure

### Option A: Using PowerShell Script (Recommended)

1. **Run the deployment script:**
```powershell
.\deploy.ps1 -ResourceGroupName "sentiment-analysis-rg" -AppServiceName "sentiment-analysis-app" -Location "East US"
```

2. **Update the script** with your GitHub repository URL in `deploy.ps1`

### Option B: Manual Deployment

1. **Create Resource Group:**
```bash
az group create --name sentiment-analysis-rg --location "East US"
```

2. **Create App Service Plan:**
```bash
az appservice plan create --name sentiment-analysis-plan --resource-group sentiment-analysis-rg --sku B1 --is-linux
```

3. **Create Web App:**
```bash
az webapp create --resource-group sentiment-analysis-rg --plan sentiment-analysis-plan --name sentiment-analysis-app --runtime "DOTNET|8.0"
```

4. **Configure Deployment Source:**
```bash
az webapp deployment source config --name sentiment-analysis-app --resource-group sentiment-analysis-rg --repo-url "https://github.com/YOUR_USERNAME/YOUR_REPO.git" --branch main --manual-integration
```

## Step 3: Configure Application Settings

### 3.1 In Azure Portal:
1. Go to your App Service
2. Navigate to **Configuration** → **Application settings**
3. Add the following settings:

| Setting Name | Value |
|--------------|-------|
| `CosmosDb:ConnectionString` | Your Cosmos DB connection string |
| `AzureCognitiveServices:Endpoint` | Your Text Analytics endpoint |
| `AzureCognitiveServices:ApiKey` | Your Text Analytics API key |
| `OpenAI:ApiKey` | Your OpenAI API key |

### 3.2 Using Azure CLI:
```bash
az webapp config appsettings set --name sentiment-analysis-app --resource-group sentiment-analysis-rg --settings \
  "CosmosDb:ConnectionString=YOUR_CONNECTION_STRING" \
  "AzureCognitiveServices:Endpoint=YOUR_ENDPOINT" \
  "AzureCognitiveServices:ApiKey=YOUR_API_KEY" \
  "OpenAI:ApiKey=YOUR_OPENAI_KEY"
```

## Step 4: Upload Your Data

### 4.1 Upload CSV Files
1. Use Azure Storage Explorer or Azure Portal
2. Upload your CSV files to the app's file system
3. Update the `Datasets:Path` setting if needed

### 4.2 Import Data
1. Access your deployed application
2. Use the data import functionality to load CSV files
3. Verify data is stored in Cosmos DB

## Step 5: Test Your Deployment

1. **Access your application:**
   ```
   https://sentiment-analysis-app.azurewebsites.net
   ```

2. **Test all three approaches:**
   - ChatGPT button
   - Azure button  
   - Hybrid button

3. **Verify functionality:**
   - Data loading works
   - Analysis completes successfully
   - Results display correctly

## Troubleshooting

### Common Issues:

1. **Application won't start:**
   - Check application settings are configured
   - Verify all API keys are correct
   - Check logs in Azure Portal

2. **Cosmos DB connection issues:**
   - Verify connection string
   - Check firewall rules
   - Ensure database and container exist

3. **API rate limits:**
   - Monitor usage in Azure Portal
   - Consider upgrading service tiers

### Viewing Logs:
```bash
az webapp log tail --name sentiment-analysis-app --resource-group sentiment-analysis-rg
```

## Cost Optimization

1. **Use B1 tier** for development (cheapest)
2. **Scale up** only when needed
3. **Monitor usage** in Azure Portal
4. **Set up alerts** for cost thresholds

## Security Best Practices

1. **Use Key Vault** for sensitive configuration
2. **Enable HTTPS** (automatic with App Service)
3. **Configure CORS** if needed
4. **Regular security updates**

## Next Steps

1. **Set up CI/CD** with GitHub Actions
2. **Configure monitoring** with Application Insights
3. **Set up custom domain** if needed
4. **Implement backup strategies**

Your sentiment analysis application is now live and accessible to anyone on the internet!



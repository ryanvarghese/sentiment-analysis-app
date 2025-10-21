#!/bin/bash

# Quick Azure App Service Deployment Script
# Run: chmod +x quick-deploy.sh && ./quick-deploy.sh

echo "🚀 Starting Azure App Service Deployment..."

# Configuration
RESOURCE_GROUP="sentiment-analysis-rg"
APP_NAME="sentiment-analysis-$(date +%s)"  # Unique name
LOCATION="East US"
PLAN_NAME="sentiment-analysis-plan"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}📋 Configuration:${NC}"
echo "Resource Group: $RESOURCE_GROUP"
echo "App Name: $APP_NAME"
echo "Location: $LOCATION"
echo ""

# Check if user is logged in to Azure
echo -e "${YELLOW}🔐 Checking Azure login...${NC}"
if ! az account show &> /dev/null; then
    echo -e "${RED}❌ Not logged in to Azure. Please run: az login${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Logged in to Azure${NC}"

# Create resource group
echo -e "${YELLOW}📦 Creating resource group...${NC}"
az group create --name $RESOURCE_GROUP --location "$LOCATION" --output none
echo -e "${GREEN}✅ Resource group created${NC}"

# Create App Service Plan
echo -e "${YELLOW}🏗️ Creating App Service Plan...${NC}"
az appservice plan create --name $PLAN_NAME --resource-group $RESOURCE_GROUP --sku B1 --is-linux --output none
echo -e "${GREEN}✅ App Service Plan created${NC}"

# Create Web App
echo -e "${YELLOW}🌐 Creating Web App...${NC}"
az webapp create --resource-group $RESOURCE_GROUP --plan $PLAN_NAME --name $APP_NAME --runtime "DOTNET|8.0" --output none
echo -e "${GREEN}✅ Web App created${NC}"

# Build and deploy
echo -e "${YELLOW}🔨 Building application...${NC}"
dotnet publish -c Release -o ./publish --no-restore
echo -e "${GREEN}✅ Application built${NC}"

echo -e "${YELLOW}📤 Deploying to Azure...${NC}"
cd publish
zip -r ../deploy.zip . > /dev/null
cd ..

az webapp deployment source config-zip --name $APP_NAME --resource-group $RESOURCE_GROUP --src ./deploy.zip --output none
echo -e "${GREEN}✅ Application deployed${NC}"

# Clean up
rm -rf ./publish
rm -f ./deploy.zip

echo ""
echo -e "${GREEN}🎉 Deployment Complete!${NC}"
echo ""
echo -e "${BLUE}📱 Your app is available at:${NC}"
echo -e "${GREEN}https://$APP_NAME.azurewebsites.net${NC}"
echo ""
echo -e "${YELLOW}⚠️  Next Steps:${NC}"
echo "1. Configure your API keys in Azure Portal:"
echo "   - Go to: https://portal.azure.com"
echo "   - Navigate to your App Service"
echo "   - Go to Configuration > Application settings"
echo "   - Add your API keys:"
echo "     • CosmosDb:ConnectionString"
echo "     • AzureCognitiveServices:Endpoint"
echo "     • AzureCognitiveServices:ApiKey"
echo "     • OpenAI:ApiKey"
echo ""
echo "2. Upload your CSV files to the app"
echo "3. Test all three analysis approaches"
echo ""
echo -e "${BLUE}🔧 Useful Commands:${NC}"
echo "View logs: az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP"
echo "Restart app: az webapp restart --name $APP_NAME --resource-group $RESOURCE_GROUP"
echo "Delete resources: az group delete --name $RESOURCE_GROUP --yes"
echo ""
echo -e "${GREEN}🚀 Happy analyzing!${NC}"



# 🆓 FREE Azure App Service Deployment Guide

Deploy your sentiment analysis application to Azure App Service for **FREE**!

## 🎯 Free Options Available

### **Option 1: F1 (Free) Tier**
- ✅ **Completely FREE**
- ❌ **1 hour execution time per day**
- ❌ **1 GB RAM, 1 GB storage**
- ❌ **No custom domains**
- ❌ **No SSL certificates**

### **Option 2: Azure for Students**
- ✅ **$100 credit for 12 months**
- ✅ **No credit card required**
- ✅ **Access to most Azure services**

### **Option 3: Azure Free Account**
- ✅ **$200 credit for 30 days**
- ✅ **12 months of free services**
- ✅ **Always free services**

## 🚀 Quick FREE Deployment

### **Step 1: Run the FREE deployment script**
```powershell
.\free-deploy.ps1
```

### **Step 2: Configure API keys**
1. Go to Azure Portal
2. Navigate to your App Service
3. Go to Configuration → Application settings
4. Add your API keys

### **Step 3: Test your app**
- Visit: `https://YOUR-APP-NAME.azurewebsites.net`
- Remember: 1 hour execution time per day!

## 💰 Cost Breakdown

### **F1 (Free) Tier:**
- **App Service:** $0/month
- **Cosmos DB:** $0/month (25 GB free)
- **Text Analytics:** $0/month (5,000 transactions free)
- **OpenAI API:** Pay per use (~$5-20/month)
- **Total:** ~$5-20/month (only OpenAI costs)

### **B1 (Production) Tier:**
- **App Service:** $13/month
- **Cosmos DB:** $5-10/month
- **Text Analytics:** $1-5/month
- **OpenAI API:** $5-20/month
- **Total:** ~$25-50/month

## 🔧 FREE Deployment Commands

### **Manual FREE deployment:**
```bash
# Create resource group
az group create --name sentiment-analysis-free-rg --location "East US"

# Create FREE App Service Plan (F1 tier)
az appservice plan create --name sentiment-analysis-free-plan --resource-group sentiment-analysis-free-rg --sku F1 --is-linux

# Create Web App
az webapp create --resource-group sentiment-analysis-free-rg --plan sentiment-analysis-free-plan --name YOUR-APP-NAME --runtime "DOTNET|8.0"

# Deploy your app
dotnet publish -c Release -o ./publish
cd publish
zip -r ../deploy.zip .
cd ..
az webapp deployment source config-zip --name YOUR-APP-NAME --resource-group sentiment-analysis-free-rg --src ./deploy.zip
```

## ⚠️ F1 (Free) Tier Limitations

### **What you get:**
- ✅ Your app runs for 1 hour per day
- ✅ 1 GB RAM
- ✅ 1 GB storage
- ✅ Basic monitoring
- ✅ Azure support

### **What you don't get:**
- ❌ Custom domains
- ❌ SSL certificates
- ❌ Auto-scaling
- ❌ Staging slots
- ❌ Continuous deployment

## 🎯 Perfect Use Cases for FREE Tier

### **✅ Great for:**
- **Development and testing**
- **Demos and proof of concepts**
- **Learning Azure**
- **Small personal projects**
- **Prototyping**

### **❌ Not suitable for:**
- **Production workloads**
- **High traffic applications**
- **24/7 availability requirements**
- **Commercial use**

## 🔄 Upgrade Path

### **When to upgrade:**
- Need more than 1 hour/day
- Want custom domains
- Need SSL certificates
- Going to production

### **How to upgrade:**
```bash
# Upgrade to B1 tier ($13/month)
az appservice plan update --name YOUR-PLAN-NAME --resource-group YOUR-RG --sku B1
```

## 🎉 Success!

Your sentiment analysis application is now running **FREE** on Azure!

**Your FREE app URL:** `https://YOUR-APP-NAME.azurewebsites.net`

## 💡 Pro Tips

1. **Monitor usage:** Check your 1-hour daily limit
2. **Optimize code:** Make it run faster to use less time
3. **Plan upgrades:** Know when to move to paid tier
4. **Use free services:** Cosmos DB and Text Analytics have free tiers

## 🆓 Always Free Azure Services

- **App Service F1:** 1 hour/day
- **Cosmos DB:** 25 GB free
- **Text Analytics:** 5,000 transactions/month
- **Storage Account:** 5 GB free
- **Functions:** 1 million executions/month

---

**🎊 Congratulations! Your sentiment analysis app is now running FREE on Azure!**



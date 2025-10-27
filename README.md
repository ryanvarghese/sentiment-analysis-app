# Sentiment Analysis Application

A comprehensive sentiment analysis application that combines Azure Cognitive Services with ChatGPT for advanced review analysis.

🚀 **CI/CD Pipeline Active!** - Automated deployment to Azure App Service.

## 🚀 Features

- **Azure Sentiment Analysis**: Traditional sentiment analysis using Azure Cognitive Services
- **ChatGPT Integration**: AI-powered sentiment analysis with detailed insights
- **Hybrid Approach**: Combines both Azure and ChatGPT for comprehensive analysis
- **Cosmos DB Storage**: Scalable data storage for reviews and analysis results
- **Web Interface**: User-friendly web interface for analysis
- **Automated CI/CD**: GitHub Actions automatically deploys to Azure App Service

## 📁 Project Structure

```
SentimentAnalysis/
├── Models/                 # Data models
│   ├── ChatGptSentimentResult.cs
│   ├── ChatGptSentimentSummary.cs
│   ├── CsvReviewRecord.cs
│   ├── ReviewData.cs
│   ├── SentimentAnalysisResult.cs
│   ├── SentimentComparisonResult.cs
│   └── SentimentSummary.cs
├── Services/               # Business logic services
│   ├── ChatGptSentimentService.cs
│   ├── CosmosDbService.cs
│   ├── CsvParserService.cs
│   ├── SentimentAnalysisService.cs
│   └── SentimentComparisonService.cs
├── Datasets/              # Sample CSV data
│   ├── Apple-Alderwood.csv
│   ├── Apple-Bellevue Square.csv
│   ├── Apple-Southcenter.csv
│   └── Apple-University Village.csv
├── .github/workflows/     # CI/CD pipeline
│   └── main_sentiment-analysis-ryan-20241215.yml
├── Program.cs             # Main application entry point
└── SentimentAnalysis.csproj
```

## 🔧 Setup

### Prerequisites
- .NET 8.0 SDK
- Azure subscription
- Azure Cognitive Services account
- OpenAI API key
- Cosmos DB account

### Configuration
1. Copy `appsettings.example.json` to `appsettings.Local.json`
2. Add your API keys to `appsettings.Local.json`
3. For Azure deployment, set environment variables in Azure App Service

## 🚀 Deployment

### Automated Deployment (CI/CD)
The application automatically deploys to Azure App Service when you push to the `main` branch:

1. **Push code** to GitHub
2. **GitHub Actions** automatically builds the app
3. **Azure App Service** automatically deploys the new version
4. **Your app updates** live on Azure

### Manual Deployment
```bash
dotnet publish SentimentAnalysis.csproj -c Release -o ./publish
# Upload publish folder to Azure App Service
```

## 🌐 Live Application

**URL**: https://sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by.eastus-01.azurewebsites.net

## 📊 Analysis Methods

### 1. Azure Only
- Traditional sentiment analysis
- Opinion mining
- Aggregated statistics

### 2. ChatGPT Only
- AI-powered analysis
- Detailed insights
- Natural language summaries

### 3. Hybrid
- Combines Azure aggregates with ChatGPT summaries
- Best of both approaches

## 🔐 Security

- API keys stored as environment variables
- No secrets in source code
- Secure configuration management
- GitHub Actions uses Azure publish profile for deployment

## 📝 Usage

1. Visit the web application
2. Upload CSV data or use existing data
3. Select a location from the dropdown
4. Choose analysis method (ChatGPT, Azure, or Hybrid)
5. View results and insights

## 🛠️ Development

### Local Development
```bash
dotnet run
```

### Testing
- Navigate to `http://localhost:5001`
- Test with sample data in `Datasets/` folder

## 📈 Performance

- Handles large datasets efficiently
- Implements data filtering (last 12 months, max 1000 reviews)
- Optimized for production workloads
- Automated CI/CD pipeline for rapid deployment

## 🎯 CI/CD Pipeline

This project uses GitHub Actions for automated deployment:

- **Trigger**: Push to `main` branch
- **Build**: .NET 8.0 application
- **Deploy**: Azure App Service
- **Status**: ✅ Fully functional

The pipeline automatically:
1. Builds the application
2. Publishes the release version
3. Deploys to Azure App Service
4. Updates the live application

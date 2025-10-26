# Sentiment Analysis Application

A comprehensive sentiment analysis application that combines Azure Cognitive Services with ChatGPT for advanced review analysis.

🚀 **CI/CD Pipeline Active!** - Automated deployment to Azure App Service.
**Build triggered: $(Get-Date)**

## 🚀 Features

- **Azure Sentiment Analysis**: Traditional sentiment analysis using Azure Cognitive Services
- **ChatGPT Integration**: AI-powered sentiment analysis with detailed insights
- **Hybrid Approach**: Combines both Azure and ChatGPT for comprehensive analysis
- **Cosmos DB Storage**: Scalable data storage for reviews and analysis results
- **Web Interface**: User-friendly web interface for analysis

## 📁 Project Structure

```
SentimentAnalysis/
├── Models/                 # Data models
│   ├── ChatGptSentimentResult.cs
│   ├── ChatGptSentimentSummary.cs
│   ├── CsvReviewRecord.cs
│   ├── ReviewData.cs
│   ├── SentimentAnalysisResult.cs
│   └── SentimentSummary.cs
├── Services/               # Business logic services
│   ├── ChatGptSentimentService.cs
│   ├── CosmosDbService.cs
│   ├── CsvParserService.cs
│   └── SentimentAnalysisService.cs
├── Datasets/              # Sample CSV data
│   ├── Apple-Alderwood.csv
│   ├── Apple-Bellevue Square.csv
│   ├── Apple-Southcenter.csv
│   └── Apple-University Village.csv
├── Program.cs             # Main application entry point
├── deploy.ps1            # Automated deployment script
└── README.md             # This file
```

## 🔧 Setup

### Prerequisites
- .NET 8.0 SDK
- Azure subscription
- Azure Cognitive Services account
- OpenAI API key
- Cosmos DB account

### Configuration
1. Copy `appsettings.template.json` to `appsettings.json`
2. Add your API keys to `appsettings.json`
3. For Azure deployment, set environment variables in Azure App Service

## 🚀 Deployment

### Automated Deployment
```powershell
powershell -ExecutionPolicy Bypass -File deploy.ps1
```

### Manual Deployment
1. Build: `dotnet publish SentimentAnalysis.csproj -c Release -o ./publish`
2. Create ZIP: `Compress-Archive -Path ./publish/* -DestinationPath deploy.zip -Force`
3. Upload to Azure Portal → Deployment Center

## 🌐 Live Application

**URL**: https://sentiment-analysis-ryan-20241215.azurewebsites.net

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

## 📝 Usage

1. Visit the web application
2. Select a location from the dropdown
3. Choose analysis method (ChatGPT, Azure, or Hybrid)
4. View results and insights

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
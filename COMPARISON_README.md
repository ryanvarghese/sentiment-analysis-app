# Sentiment Analysis Comparison: Azure vs ChatGPT

This enhanced version of the sentiment analysis application now includes a comparison feature that allows you to analyze the same data using both Azure Text Analytics and ChatGPT models, providing detailed insights into their differences and performance.

## New Features

### 1. ChatGPT Sentiment Analysis
- **Service**: `ChatGptSentimentService`
- **Models**: `ChatGptSentimentResult`, `ChatGptSentimentSummary`
- **Features**:
  - Detailed sentiment analysis with reasoning
  - Key points extraction
  - Pros and cons identification
  - AI-generated comprehensive summaries
  - Confidence scoring

### 2. Comparison Analysis
- **Service**: `SentimentComparisonService`
- **Model**: `SentimentComparisonResult`
- **Metrics**:
  - Sentiment agreement percentage
  - Confidence score differences
  - Pros/cons overlap analysis
  - Processing time comparison
  - Cost estimates
  - AI-generated recommendations

## Setup Instructions

### 1. Configure OpenAI API Key

Update your `appsettings.json` with your OpenAI API key:

```json
{
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY_HERE"
  }
}
```

**Note**: You need to replace `YOUR_OPENAI_API_KEY_HERE` with your actual OpenAI API key.

### 2. Azure OpenAI Configuration (Alternative)

If you're using Azure OpenAI instead of direct OpenAI API, update the configuration:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "YOUR_AZURE_OPENAI_KEY",
    "DeploymentName": "gpt-3.5-turbo"
  }
}
```

## Usage

### 1. Upload Data
- Use the existing CSV upload functionality
- Import your review data as before

### 2. Run Analysis

#### Option A: Azure Text Analytics Only
- Click "Analyze (Azure)" button
- Uses your existing Azure Text Analytics setup
- Provides sentiment analysis with pros/cons

#### Option B: Compare Both Models
- Click "Compare Both" button
- Runs both Azure Text Analytics and ChatGPT analysis
- Provides detailed comparison metrics
- Shows side-by-side results
- Includes AI-generated recommendations

### 3. View Results

The comparison page displays:

#### Metrics Dashboard
- **Sentiment Agreement**: Percentage of reviews where both models agree
- **Confidence Difference**: Average difference in confidence scores
- **Pros/Cons Overlap**: How much the identified pros/cons overlap
- **Processing Times**: Performance comparison
- **Cost Estimates**: Estimated costs for both services

#### Side-by-Side Results
- **Azure Results**: Traditional sentiment analysis with opinion mining
- **ChatGPT Results**: AI-powered analysis with detailed reasoning
- **AI Summary**: Comprehensive business insights from ChatGPT

#### Recommendations
- AI-generated recommendations based on the comparison
- Suggestions for which model to use for different scenarios
- Performance and cost optimization tips

## Key Differences Between Models

### Azure Text Analytics
- **Strengths**:
  - Fast processing
  - Consistent results
  - Lower cost for high volume
  - Opinion mining capabilities
  - Enterprise-grade reliability

- **Best For**:
  - High-volume processing
  - Real-time applications
  - Cost-sensitive projects
  - Standard sentiment analysis

### ChatGPT Analysis
- **Strengths**:
  - Natural language understanding
  - Contextual analysis
  - Detailed reasoning
  - Comprehensive summaries
  - Nuanced sentiment detection

- **Best For**:
  - Complex text analysis
  - Detailed insights
  - Business intelligence
  - Research applications
  - Qualitative analysis

## Cost Considerations

### Azure Text Analytics
- **Pricing**: ~$1 per 1,000 transactions
- **Best for**: High-volume, cost-sensitive applications

### ChatGPT/OpenAI
- **Pricing**: ~$0.002 per 1K tokens
- **Best for**: Detailed analysis, lower volume, quality-focused applications

## API Endpoints

### New Endpoints
- `GET /compare?location={location}` - Run comparison analysis
- Existing endpoints remain unchanged

### Data Storage
All results are stored in Cosmos DB:
- `SentimentAnalysisResult` - Azure results
- `ChatGptSentimentResult` - ChatGPT results
- `SentimentComparisonResult` - Comparison metrics

## Troubleshooting

### Common Issues

1. **OpenAI API Key Not Configured**
   - Ensure you've set the OpenAI API key in `appsettings.json`
   - Verify the key is valid and has sufficient credits

2. **Rate Limiting**
   - ChatGPT has rate limits; the service processes reviews in small batches
   - Azure Text Analytics also has rate limits; both services handle this automatically

3. **Cost Concerns**
   - Monitor your OpenAI usage
   - Consider using Azure OpenAI for better cost control
   - Use the cost estimates in the comparison results

### Performance Tips

1. **Batch Processing**: Both services process data in batches to respect API limits
2. **Caching**: Results are stored in Cosmos DB to avoid re-processing
3. **Error Handling**: Failed requests are logged and don't stop the entire process

## Example Comparison Results

A typical comparison might show:
- **Sentiment Agreement**: 85% (high agreement)
- **Confidence Difference**: 0.12 (moderate difference)
- **Pros Overlap**: 70% (good overlap)
- **Cons Overlap**: 65% (reasonable overlap)
- **Processing Time**: Azure: 2.3s, ChatGPT: 8.7s
- **Recommendation**: "Both models show good agreement - either can be used reliably"

## Next Steps

1. **Configure your OpenAI API key**
2. **Upload your CSV data**
3. **Run a comparison analysis**
4. **Review the metrics and recommendations**
5. **Choose the best model for your use case**

The comparison feature provides valuable insights to help you make informed decisions about which sentiment analysis approach works best for your specific data and requirements.







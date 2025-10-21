using Microsoft.Extensions.Logging;
using SentimentAnalysis.Models;

namespace SentimentAnalysis.Services;

public interface ISentimentComparisonService
{
    Task<SentimentComparisonResult> CompareAnalysisAsync(string location, List<ReviewData> reviews);
}

public class SentimentComparisonService : ISentimentComparisonService
{
    private readonly ISentimentAnalysisService _azureService;
    private readonly IChatGptSentimentService _chatGptService;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly ILogger<SentimentComparisonService> _logger;

    public SentimentComparisonService(
        ISentimentAnalysisService azureService,
        IChatGptSentimentService chatGptService,
        ICosmosDbService cosmosDbService,
        ILogger<SentimentComparisonService> logger)
    {
        _azureService = azureService;
        _chatGptService = chatGptService;
        _cosmosDbService = cosmosDbService;
        _logger = logger;
    }

    public async Task<SentimentComparisonResult> CompareAnalysisAsync(string location, List<ReviewData> reviews)
    {
        _logger.LogInformation("Starting sentiment analysis comparison for location: {Location}", location);

        var comparisonResult = new SentimentComparisonResult
        {
            Location = location
        };

        var startTime = DateTime.UtcNow;

        // Run Azure analysis
        var azureStartTime = DateTime.UtcNow;
        var azureResults = await _azureService.AnalyzeReviewsAsync(reviews);
        var azureSummary = await _azureService.GenerateSummaryAsync(location, azureResults);
        var azureEndTime = DateTime.UtcNow;

        // Run ChatGPT analysis
        var chatGptStartTime = DateTime.UtcNow;
        var chatGptResults = await _chatGptService.AnalyzeReviewsAsync(reviews);
        var chatGptSummary = await _chatGptService.GenerateSummaryAsync(location, chatGptResults);
        var chatGptEndTime = DateTime.UtcNow;

        // Store results in Cosmos DB
        if (azureResults.Any())
        {
            await _cosmosDbService.StoreSentimentResultsAsync(azureResults);
        }
        if (chatGptResults.Any())
        {
            await _cosmosDbService.StoreChatGptResultsAsync(chatGptResults);
        }

        await _cosmosDbService.StoreSentimentSummaryAsync(azureSummary);
        await _cosmosDbService.StoreChatGptSummaryAsync(chatGptSummary);

        // Calculate comparison metrics
        var metrics = CalculateComparisonMetrics(
            azureResults, 
            chatGptResults, 
            azureSummary, 
            chatGptSummary,
            azureEndTime - azureStartTime,
            chatGptEndTime - chatGptStartTime);

        comparisonResult.AzureResults = azureSummary;
        comparisonResult.ChatGptResults = chatGptSummary;
        comparisonResult.ComparisonMetrics = metrics;

        // Store comparison result
        await _cosmosDbService.StoreComparisonResultAsync(comparisonResult);

        _logger.LogInformation("Completed sentiment analysis comparison for location: {Location}", location);
        return comparisonResult;
    }

    private ComparisonMetrics CalculateComparisonMetrics(
        List<SentimentAnalysisResult> azureResults,
        List<ChatGptSentimentResult> chatGptResults,
        SentimentSummary azureSummary,
        ChatGptSentimentSummary chatGptSummary,
        TimeSpan azureProcessingTime,
        TimeSpan chatGptProcessingTime)
    {
        var metrics = new ComparisonMetrics
        {
            ProcessingTimeAzure = azureProcessingTime,
            ProcessingTimeChatGpt = chatGptProcessingTime
        };

        // Calculate sentiment agreement
        var sentimentAgreement = CalculateSentimentAgreement(azureResults, chatGptResults);
        metrics.SentimentAgreement = sentimentAgreement;

        // Calculate confidence difference
        var confidenceDifference = CalculateConfidenceDifference(azureResults, chatGptResults);
        metrics.ConfidenceDifference = confidenceDifference;

        // Calculate pros/cons overlap
        var prosOverlap = CalculateOverlap(azureSummary.TopPros, chatGptSummary.TopPros);
        var consOverlap = CalculateOverlap(azureSummary.TopCons, chatGptSummary.TopCons);
        metrics.ProsOverlap = prosOverlap;
        metrics.ConsOverlap = consOverlap;

        // Calculate cost estimates (rough estimates)
        metrics.AzureCostEstimate = CalculateAzureCost(azureResults.Count);
        metrics.ChatGptCostEstimate = CalculateChatGptCost(chatGptResults.Count);
        metrics.TotalCostEstimate = metrics.AzureCostEstimate + metrics.ChatGptCostEstimate;

        // Generate recommendations
        metrics.Recommendations = GenerateRecommendations(metrics, azureSummary, chatGptSummary);

        return metrics;
    }

    private double CalculateSentimentAgreement(List<SentimentAnalysisResult> azureResults, List<ChatGptSentimentResult> chatGptResults)
    {
        if (!azureResults.Any() || !chatGptResults.Any())
            return 0.0;

        var agreementCount = 0;
        var totalComparisons = 0;

        foreach (var azureResult in azureResults)
        {
            var chatGptResult = chatGptResults.FirstOrDefault(c => c.ReviewId == azureResult.ReviewId);
            if (chatGptResult != null)
            {
                totalComparisons++;
                if (DoSentimentsMatch(azureResult.Sentiment, chatGptResult.Sentiment))
                {
                    agreementCount++;
                }
            }
        }

        return totalComparisons > 0 ? (double)agreementCount / totalComparisons * 100 : 0.0;
    }

    private bool DoSentimentsMatch(string azureSentiment, string chatGptSentiment)
    {
        var azureNormalized = azureSentiment.ToLower();
        var chatGptNormalized = chatGptSentiment.ToLower();

        return (azureNormalized == "positive" && chatGptNormalized == "positive") ||
               (azureNormalized == "negative" && chatGptNormalized == "negative") ||
               (azureNormalized == "neutral" && chatGptNormalized == "neutral");
    }

    private double CalculateConfidenceDifference(List<SentimentAnalysisResult> azureResults, List<ChatGptSentimentResult> chatGptResults)
    {
        if (!azureResults.Any() || !chatGptResults.Any())
            return 0.0;

        var differences = new List<double>();

        foreach (var azureResult in azureResults)
        {
            var chatGptResult = chatGptResults.FirstOrDefault(c => c.ReviewId == azureResult.ReviewId);
            if (chatGptResult != null)
            {
                differences.Add(Math.Abs(azureResult.Confidence - chatGptResult.Confidence));
            }
        }

        return differences.Any() ? differences.Average() : 0.0;
    }

    private double CalculateOverlap(List<string> list1, List<string> list2)
    {
        if (!list1.Any() || !list2.Any())
            return 0.0;

        var normalizedList1 = list1.Select(s => s.ToLower()).ToHashSet();
        var normalizedList2 = list2.Select(s => s.ToLower()).ToHashSet();

        var intersection = normalizedList1.Intersect(normalizedList2).Count();
        var union = normalizedList1.Union(normalizedList2).Count();

        return union > 0 ? (double)intersection / union * 100 : 0.0;
    }

    private decimal CalculateAzureCost(int reviewCount)
    {
        // Azure Text Analytics pricing: $1 per 1,000 transactions
        // Each review = 1 transaction for sentiment analysis
        return (decimal)reviewCount / 1000 * 1.00m;
    }

    private decimal CalculateChatGptCost(int reviewCount)
    {
        // ChatGPT pricing: $0.002 per 1K tokens (rough estimate)
        // Assuming average 100 tokens per review
        var estimatedTokens = reviewCount * 100;
        return (decimal)estimatedTokens / 1000 * 0.002m;
    }

    private List<string> GenerateRecommendations(ComparisonMetrics metrics, SentimentSummary azureSummary, ChatGptSentimentSummary chatGptSummary)
    {
        var recommendations = new List<string>();

        // Sentiment agreement analysis
        if (metrics.SentimentAgreement > 80)
        {
            recommendations.Add("High sentiment agreement between models - results are reliable");
        }
        else if (metrics.SentimentAgreement < 60)
        {
            recommendations.Add("Low sentiment agreement - consider manual review of conflicting cases");
        }

        // Processing time analysis
        if (metrics.ProcessingTimeChatGpt > metrics.ProcessingTimeAzure * 2)
        {
            recommendations.Add("ChatGPT processing is significantly slower - consider Azure for real-time needs");
        }

        // Cost analysis
        if (metrics.ChatGptCostEstimate > metrics.AzureCostEstimate * 2)
        {
            recommendations.Add("ChatGPT costs are higher - Azure may be more cost-effective for large volumes");
        }

        // Confidence analysis
        if (metrics.ConfidenceDifference > 0.3)
        {
            recommendations.Add("Significant confidence differences - models may have different sensitivity levels");
        }

        // Pros/cons overlap analysis
        if (metrics.ProsOverlap < 50)
        {
            recommendations.Add("Low pros overlap - models identify different positive aspects");
        }
        if (metrics.ConsOverlap < 50)
        {
            recommendations.Add("Low cons overlap - models identify different negative aspects");
        }

        // Overall recommendation
        if (metrics.SentimentAgreement > 75 && metrics.ProsOverlap > 60 && metrics.ConsOverlap > 60)
        {
            recommendations.Add("Both models show good agreement - either can be used reliably");
        }
        else
        {
            recommendations.Add("Consider using both models for comprehensive analysis");
        }

        return recommendations;
    }
}







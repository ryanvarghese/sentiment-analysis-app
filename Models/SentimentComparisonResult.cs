using System.Text.Json.Serialization;

namespace SentimentAnalysis.Models;

public class SentimentComparisonResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("azureResults")]
    public SentimentSummary AzureResults { get; set; } = new SentimentSummary();

    [JsonPropertyName("chatgptResults")]
    public ChatGptSentimentSummary ChatGptResults { get; set; } = new ChatGptSentimentSummary();

    [JsonPropertyName("comparisonMetrics")]
    public ComparisonMetrics ComparisonMetrics { get; set; } = new ComparisonMetrics();

    [JsonPropertyName("analysisDate")]
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => Location;

    public SentimentComparisonResult()
    {
        Id = Guid.NewGuid().ToString();
        AnalysisDate = DateTime.UtcNow;
    }
}

public class ComparisonMetrics
{
    [JsonPropertyName("sentimentAgreement")]
    public double SentimentAgreement { get; set; } // Percentage of reviews where both models agree

    [JsonPropertyName("confidenceDifference")]
    public double ConfidenceDifference { get; set; } // Average difference in confidence scores

    [JsonPropertyName("prosOverlap")]
    public double ProsOverlap { get; set; } // Percentage of pros that overlap between models

    [JsonPropertyName("consOverlap")]
    public double ConsOverlap { get; set; } // Percentage of cons that overlap between models

    [JsonPropertyName("processingTimeAzure")]
    public TimeSpan ProcessingTimeAzure { get; set; }

    [JsonPropertyName("processingTimeChatGpt")]
    public TimeSpan ProcessingTimeChatGpt { get; set; }

    [JsonPropertyName("totalCostEstimate")]
    public decimal TotalCostEstimate { get; set; }

    [JsonPropertyName("azureCostEstimate")]
    public decimal AzureCostEstimate { get; set; }

    [JsonPropertyName("chatgptCostEstimate")]
    public decimal ChatGptCostEstimate { get; set; }

    [JsonPropertyName("recommendations")]
    public List<string> Recommendations { get; set; } = new List<string>();
}







using System.Text.Json.Serialization;

namespace SentimentAnalysis.Models;

public class ChatGptSentimentSummary
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("overallSentiment")]
    public string OverallSentiment { get; set; } = string.Empty; // Positive, Negative, Neutral

    [JsonPropertyName("totalReviews")]
    public int TotalReviews { get; set; }

    [JsonPropertyName("positiveCount")]
    public int PositiveCount { get; set; }

    [JsonPropertyName("negativeCount")]
    public int NegativeCount { get; set; }

    [JsonPropertyName("neutralCount")]
    public int NeutralCount { get; set; }

    [JsonPropertyName("averageConfidence")]
    public double AverageConfidence { get; set; }

    [JsonPropertyName("averageStarRating")]
    public double AverageStarRating { get; set; }

    [JsonPropertyName("topPros")]
    public List<string> TopPros { get; set; } = new List<string>();

    [JsonPropertyName("topCons")]
    public List<string> TopCons { get; set; } = new List<string>();

    [JsonPropertyName("aiSummary")]
    public string AiSummary { get; set; } = string.Empty;

    [JsonPropertyName("summaryDate")]
    public DateTime SummaryDate { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => Location;

    public ChatGptSentimentSummary()
    {
        Id = Guid.NewGuid().ToString();
        SummaryDate = DateTime.UtcNow;
    }
}







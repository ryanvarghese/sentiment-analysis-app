using System.Text.Json.Serialization;

namespace SentimentAnalysis.Models;

public class SentimentAnalysisResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("reviewId")]
    public string ReviewId { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; } = string.Empty; // Positive, Negative, Neutral

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("positiveScore")]
    public double PositiveScore { get; set; }

    [JsonPropertyName("negativeScore")]
    public double NegativeScore { get; set; }

    [JsonPropertyName("neutralScore")]
    public double NeutralScore { get; set; }

    [JsonPropertyName("reviewContent")]
    public string ReviewContent { get; set; } = string.Empty;

    [JsonPropertyName("authorName")]
    public string AuthorName { get; set; } = string.Empty;

    [JsonPropertyName("starRating")]
    public int StarRating { get; set; }

    [JsonPropertyName("analysisDate")]
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => Location;

    public SentimentAnalysisResult()
    {
        Id = Guid.NewGuid().ToString();
        AnalysisDate = DateTime.UtcNow;
    }
}

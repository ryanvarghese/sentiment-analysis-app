using System.Text.Json.Serialization;

namespace SentimentAnalysis.Models;

public class ChatGptSentimentResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("reviewId")]
    public string ReviewId { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; } = string.Empty; // positive, negative, neutral

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;

    [JsonPropertyName("keyPoints")]
    public List<string> KeyPoints { get; set; } = new List<string>();

    [JsonPropertyName("pros")]
    public List<string> Pros { get; set; } = new List<string>();

    [JsonPropertyName("cons")]
    public List<string> Cons { get; set; } = new List<string>();

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

    public ChatGptSentimentResult()
    {
        Id = Guid.NewGuid().ToString();
        AnalysisDate = DateTime.UtcNow;
    }
}







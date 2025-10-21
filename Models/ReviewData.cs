using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SentimentAnalysis.Models;

public class ReviewData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("reviewDate")]
    public string ReviewDate { get; set; } = string.Empty;

    [JsonPropertyName("authorName")]
    public string AuthorName { get; set; } = string.Empty;

    [JsonPropertyName("starRating")]
    public int StarRating { get; set; }

    [JsonPropertyName("reviewContent")]
    public string ReviewContent { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("processed")]
    public bool Processed { get; set; } = false;

    [JsonPropertyName("sentimentResult")]
    public string SentimentResult { get; set; } = string.Empty;

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => Location;

    public ReviewData()
    {
        Id = Guid.NewGuid().ToString();
    }
}

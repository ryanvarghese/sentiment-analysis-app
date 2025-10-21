using CsvHelper.Configuration.Attributes;

namespace SentimentAnalysis.Models;

public class CsvReviewRecord
{
    [Name("Review date")]
    public string ReviewDate { get; set; } = string.Empty;

    [Name("Author name")]
    public string AuthorName { get; set; } = string.Empty;

    [Name("Star rating")]
    public int StarRating { get; set; }

    [Name("Review content")]
    public string ReviewContent { get; set; } = string.Empty;
}

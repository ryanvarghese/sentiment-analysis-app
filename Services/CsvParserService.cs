using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using SentimentAnalysis.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SentimentAnalysis.Services;

public interface ICsvParserService
{
    Task<List<ReviewData>> ParseCsvFilesAsync(string datasetsPath);
    Task<List<ReviewData>> ParseCsvStreamAsync(Stream fileStream, string fileName);
}

public class CsvParserService : ICsvParserService
{
    private readonly ILogger<CsvParserService> _logger;

    public CsvParserService(ILogger<CsvParserService> logger)
    {
        _logger = logger;
    }

    public async Task<List<ReviewData>> ParseCsvFilesAsync(string datasetsPath)
    {
        var allReviews = new List<ReviewData>();

        if (!Directory.Exists(datasetsPath))
        {
            _logger.LogError("Datasets directory not found: {Path}", datasetsPath);
            return allReviews;
        }

        var csvFiles = Directory.GetFiles(datasetsPath, "Apple-*.csv");
        _logger.LogInformation("Found {Count} Apple CSV files to process", csvFiles.Length);

        foreach (var filePath in csvFiles)
        {
            try
            {
                var location = ExtractLocationFromFileName(Path.GetFileName(filePath));
                _logger.LogInformation("Processing file: {FileName}, Location: {Location}", Path.GetFileName(filePath), location);

                var reviews = await ParseCsvFileAsync(filePath, location);
                allReviews.AddRange(reviews);

                _logger.LogInformation("Successfully processed {Count} reviews from {FileName}", reviews.Count, Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FileName}", Path.GetFileName(filePath));
            }
        }

        _logger.LogInformation("Total reviews processed: {Count}", allReviews.Count);
        return allReviews;
    }

    private async Task<List<ReviewData>> ParseCsvFileAsync(string filePath, string location)
    {
        var reviews = new List<ReviewData>();

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // Configure CSV reader
        csv.Context.RegisterClassMap<CsvReviewRecordMap>();

        await foreach (var record in csv.GetRecordsAsync<CsvReviewRecord>())
        {
            var reviewData = new ReviewData
            {
                ReviewDate = record.ReviewDate,
                AuthorName = record.AuthorName,
                StarRating = record.StarRating,
                ReviewContent = record.ReviewContent,
                Location = location
            };

            reviews.Add(reviewData);
        }

        return reviews;
    }

    public async Task<List<ReviewData>> ParseCsvStreamAsync(Stream fileStream, string fileName)
    {
        var reviews = new List<ReviewData>();
        var location = ExtractLocationFromFileName(Path.GetFileName(fileName));

        using var reader = new StreamReader(fileStream, leaveOpen: true);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<CsvReviewRecordMap>();

        await foreach (var record in csv.GetRecordsAsync<CsvReviewRecord>())
        {
            var reviewData = new ReviewData
            {
                ReviewDate = record.ReviewDate,
                AuthorName = record.AuthorName,
                StarRating = record.StarRating,
                ReviewContent = record.ReviewContent,
                Location = location
            };

            reviews.Add(reviewData);
        }

        // reset stream position for any callers that may re-read
        if (fileStream.CanSeek) fileStream.Seek(0, SeekOrigin.Begin);
        return reviews;
    }

    private static string ExtractLocationFromFileName(string fileName)
    {
        // Extract location from filename pattern: Apple-{Location}.csv
        var match = Regex.Match(fileName, @"Apple-(.+)\.csv", RegexOptions.IgnoreCase);
        
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Fallback: remove "Apple-" prefix and ".csv" suffix
        var location = fileName
            .Replace("Apple-", "", StringComparison.OrdinalIgnoreCase)
            .Replace(".csv", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        return string.IsNullOrEmpty(location) ? "Unknown" : location;
    }
}

// CSV mapping configuration
public sealed class CsvReviewRecordMap : ClassMap<CsvReviewRecord>
{
    public CsvReviewRecordMap()
    {
        Map(m => m.ReviewDate).Name("Review date");
        Map(m => m.AuthorName).Name("Author name");
        Map(m => m.StarRating).Name("Star rating");
        Map(m => m.ReviewContent).Name("Review content");
    }
}

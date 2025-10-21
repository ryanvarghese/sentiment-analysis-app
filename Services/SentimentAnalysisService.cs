using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Logging;
using SentimentAnalysis.Models;
using System.Text.RegularExpressions;

namespace SentimentAnalysis.Services;

public interface ISentimentAnalysisService
{
    Task<List<SentimentAnalysisResult>> AnalyzeReviewsAsync(List<ReviewData> reviews);
    Task<SentimentSummary> GenerateSummaryAsync(string location, List<SentimentAnalysisResult> results);
}

public class SentimentAnalysisService : ISentimentAnalysisService
{
    private readonly TextAnalyticsClient _textAnalyticsClient;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly ILogger<SentimentAnalysisService> _logger;

    public SentimentAnalysisService(
        TextAnalyticsClient textAnalyticsClient,
        ICosmosDbService cosmosDbService,
        ILogger<SentimentAnalysisService> logger)
    {
        _textAnalyticsClient = textAnalyticsClient;
        _cosmosDbService = cosmosDbService;
        _logger = logger;
    }

    public async Task<List<SentimentAnalysisResult>> AnalyzeReviewsAsync(List<ReviewData> reviews)
    {
        var results = new List<SentimentAnalysisResult>();

        if (!reviews.Any())
        {
            _logger.LogWarning("No reviews provided for sentiment analysis");
            return results;
        }

        _logger.LogInformation("Starting sentiment analysis for {Count} reviews", reviews.Count);

        // Process reviews in batches (Azure Text Analytics has limits)
        const int batchSize = 10;
        var batches = reviews.Chunk(batchSize);

        foreach (var batch in batches)
        {
            try
            {
                var batchResults = await ProcessBatchAsync(batch);
                results.AddRange(batchResults);
                
                _logger.LogDebug("Processed batch of {Count} reviews", batch.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch of reviews");
            }
        }

        _logger.LogInformation("Completed sentiment analysis. Processed {Count} reviews", results.Count);
        return results;
    }

    private async Task<List<SentimentAnalysisResult>> ProcessBatchAsync(IEnumerable<ReviewData> reviews)
    {
        var results = new List<SentimentAnalysisResult>();
        var documents = new List<TextDocumentInput>();

        foreach (var review in reviews)
        {
            // Clean and prepare the text for analysis
            var cleanText = CleanTextForAnalysis(review.ReviewContent);
            
            if (string.IsNullOrWhiteSpace(cleanText))
            {
                _logger.LogWarning("Skipping review {ReviewId} - empty content after cleaning", review.Id);
                continue;
            }

            documents.Add(new TextDocumentInput(review.Id, cleanText)
            {
                Language = "en" // Assuming English reviews
            });
        }

        if (!documents.Any())
        {
            return results;
        }

        try
        {
            var response = await _textAnalyticsClient.AnalyzeSentimentBatchAsync(documents);
            
            foreach (var document in response.Value)
            {
                if (document.HasError)
                {
                    _logger.LogWarning("Error analyzing document {Id}: {Error}", 
                        document.Id, document.Error.Message);
                    continue;
                }

                var review = reviews.FirstOrDefault(r => r.Id == document.Id);
                if (review == null)
                {
                    _logger.LogWarning("Review not found for document {Id}", document.Id);
                    continue;
                }

                var doc = document.DocumentSentiment;
                var sentimentResult = new SentimentAnalysisResult
                {
                    ReviewId = review.Id,
                    Location = review.Location,
                    ReviewContent = review.ReviewContent,
                    AuthorName = review.AuthorName,
                    StarRating = review.StarRating,
                    Sentiment = doc.Sentiment.ToString(),
                    Confidence = Math.Max(Math.Max(doc.ConfidenceScores.Positive, doc.ConfidenceScores.Negative), doc.ConfidenceScores.Neutral),
                    PositiveScore = doc.ConfidenceScores.Positive,
                    NegativeScore = doc.ConfidenceScores.Negative,
                    NeutralScore = doc.ConfidenceScores.Neutral
                };

                results.Add(sentimentResult);

                // Update the original review document in Cosmos DB
                var mapped = doc.Sentiment switch
                {
                    TextSentiment.Positive => "+",
                    TextSentiment.Negative => "-",
                    TextSentiment.Neutral => "mixed",
                    _ => "mixed"
                };

                _logger.LogDebug("Updating review {ReviewId} with processed=true and sentiment={Sentiment}", review.Id, mapped);
                var updated = await _cosmosDbService.UpdateReviewProcessedStatusAsync(review.Id, mapped);
                if (!updated)
                {
                    _logger.LogWarning("Failed to mark review {ReviewId} as processed", review.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Azure Text Analytics API");
            throw;
        }

        return results;
    }

    public async Task<SentimentSummary> GenerateSummaryAsync(string location, List<SentimentAnalysisResult> results)
    {
        var locationResults = results.Where(r => r.Location.Equals(location, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!locationResults.Any())
        {
            _logger.LogWarning("No sentiment results found for location: {Location}", location);
            return new SentimentSummary { Location = location };
        }

        var summary = new SentimentSummary
        {
            Location = location,
            TotalReviews = locationResults.Count,
            PositiveCount = locationResults.Count(r => r.Sentiment == "Positive"),
            NegativeCount = locationResults.Count(r => r.Sentiment == "Negative"),
            NeutralCount = locationResults.Count(r => r.Sentiment == "Neutral"),
            AverageConfidence = locationResults.Average(r => r.Confidence),
            AveragePositiveScore = locationResults.Average(r => r.PositiveScore),
            AverageNegativeScore = locationResults.Average(r => r.NegativeScore),
            AverageNeutralScore = locationResults.Average(r => r.NeutralScore),
            AverageStarRating = locationResults.Average(r => r.StarRating)
        };

        // Determine overall sentiment
        if (summary.PositiveCount > summary.NegativeCount && summary.PositiveCount > summary.NeutralCount)
        {
            summary.OverallSentiment = "Positive";
        }
        else if (summary.NegativeCount > summary.PositiveCount && summary.NegativeCount > summary.NeutralCount)
        {
            summary.OverallSentiment = "Negative";
        }
        else
        {
            summary.OverallSentiment = "Neutral";
        }

        // Extract dataset-level pros/cons using Opinion Mining (targets + assessments)
        summary.TopPros = await ExtractProsConsWithOpinionMiningAsync(
            locationResults.Where(r => r.Sentiment == "Positive").ToList());
        summary.TopCons = await ExtractProsConsWithOpinionMiningAsync(
            locationResults.Where(r => r.Sentiment == "Negative").ToList());

        _logger.LogInformation("Generated summary for {Location}: {Sentiment} sentiment, {Total} reviews", 
            location, summary.OverallSentiment, summary.TotalReviews);

        return summary;
    }

    // Enhanced Opinion Mining with Amazon-like filtering and normalization
    private static readonly HashSet<string> PersonWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "girl", "guy", "lady", "man", "woman", "person", "people", "staff", "employee", "worker",
        "associate", "rep", "technician", "manager", "supervisor", "clerk", "cashier"
    };

    private static readonly HashSet<string> GenericTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        "store", "time", "place", "experience", "thing", "day", "week", "month", "year", 
        "help", "issue", "problem", "lot", "everything", "stuff", "service", "customer"
    };

    // Target normalization mapping (synonyms -> canonical form)
    private static readonly Dictionary<string, string> TargetNormalization = new(StringComparer.OrdinalIgnoreCase)
    {
        { "staff", "staff" }, { "associate", "staff" }, { "rep", "staff" }, { "employee", "staff" },
        { "wait", "wait time" }, { "queue", "wait time" }, { "line", "wait time" },
        { "price", "pricing" }, { "cost", "pricing" }, { "money", "pricing" },
        { "device", "device" }, { "phone", "device" }, { "product", "device" },
        { "battery", "battery life" }, { "charge", "battery life" },
        { "repair", "repair service" }, { "fix", "repair service" }, { "service", "repair service" },
        { "return", "return policy" }, { "refund", "return policy" }
    };

    // Assessment normalization (clean up adjectives)
    private static readonly Dictionary<string, string> AssessmentNormalization = new(StringComparer.OrdinalIgnoreCase)
    {
        { "nice", "good" }, { "great", "excellent" }, { "awesome", "excellent" },
        { "terrible", "poor" }, { "awful", "poor" }, { "horrible", "poor" },
        { "fast", "quick" }, { "slow", "slow" }, { "quick", "quick" }
    };

    private async Task<List<string>> ExtractProsConsWithOpinionMiningAsync(List<SentimentAnalysisResult> results)
    {
        if (!results.Any())
        {
            return new List<string>();
        }

        var opinionPhrases = new List<OpinionPhrase>();
        const int batchSize = 10; // Azure limit

        foreach (var chunk in results.Chunk(batchSize))
        {
            var docs = new List<TextDocumentInput>();
            foreach (var r in chunk)
            {
                var text = CleanTextForAnalysis(r.ReviewContent);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    docs.Add(new TextDocumentInput(r.ReviewId, text) { Language = "en" });
                }
            }

            if (!docs.Any())
            {
                continue;
            }

            try
            {
                var options = new AnalyzeSentimentOptions { IncludeOpinionMining = true };
                var response = await _textAnalyticsClient.AnalyzeSentimentBatchAsync(docs, options);

                foreach (var doc in response.Value)
                {
                    if (doc.HasError)
                    {
                        continue;
                    }

                    foreach (var sentence in doc.DocumentSentiment.Sentences)
                    {
                        foreach (var opinion in sentence.Opinions)
                        {
                            var target = opinion.Target.Text?.Trim();
                            if (string.IsNullOrWhiteSpace(target))
                            {
                                continue;
                            }

                            // Filter out person words and generic terms
                            if (PersonWords.Contains(target) || GenericTerms.Contains(target))
                            {
                                continue;
                            }

                            // Normalize target
                            var normalizedTarget = NormalizeTarget(target);

                            foreach (var assessment in opinion.Assessments)
                            {
                                var assessmentText = assessment.Text?.Trim();
                                if (string.IsNullOrWhiteSpace(assessmentText))
                                {
                                    continue;
                                }

                                // Normalize assessment
                                var normalizedAssessment = NormalizeAssessment(assessmentText);

                                // Create human-readable phrase
                                var phrase = CreateReadablePhrase(normalizedTarget, normalizedAssessment, assessment.Sentiment);
                                
                                if (!string.IsNullOrWhiteSpace(phrase) && phrase.Length >= 8)
                                {
                                    opinionPhrases.Add(new OpinionPhrase
                                    {
                                        Phrase = phrase,
                                        Target = normalizedTarget,
                                        Assessment = normalizedAssessment,
                                        Sentiment = assessment.Sentiment.ToString(),
                                        Confidence = assessment.ConfidenceScores.Positive + assessment.ConfidenceScores.Negative
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting pros/cons with opinion mining");
            }
        }

        // Group by normalized target and assessment, then rank by frequency and confidence
        return opinionPhrases
            .GroupBy(op => $"{op.Target} {op.Assessment}".ToLower())
            .OrderByDescending(g => g.Count() * g.Average(x => x.Confidence))
            .Take(5)
            .Select(g => g.First().Phrase)
            .ToList();
    }

    private static string NormalizeTarget(string target)
    {
        if (TargetNormalization.TryGetValue(target, out var normalized))
        {
            return normalized;
        }
        return target.ToLower();
    }

    private static string NormalizeAssessment(string assessment)
    {
        if (AssessmentNormalization.TryGetValue(assessment, out var normalized))
        {
            return normalized;
        }
        return assessment.ToLower();
    }

    private static string CreateReadablePhrase(string target, string assessment, TextSentiment sentiment)
    {
        // Create more natural phrases based on sentiment
        return sentiment switch
        {
            TextSentiment.Positive => $"{target} is {assessment}",
            TextSentiment.Negative => $"{target} is {assessment}",
            _ => $"{target} {assessment}"
        };
    }

    private class OpinionPhrase
    {
        public string Phrase { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string Assessment { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    private static string CleanTextForAnalysis(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Remove excessive whitespace and normalize
        text = Regex.Replace(text, @"\s+", " ");
        
        // Remove very short text (likely not meaningful)
        if (text.Length < 10)
        {
            return string.Empty;
        }

        return text.Trim();
    }
}

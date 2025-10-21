using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure.AI.OpenAI;
using SentimentAnalysis.Models;
using System.Text.Json;

namespace SentimentAnalysis.Services;

public interface IChatGptSentimentService
{
    Task<List<ChatGptSentimentResult>> AnalyzeReviewsAsync(List<ReviewData> reviews);
    Task<ChatGptSentimentSummary> GenerateSummaryAsync(string location, List<ChatGptSentimentResult> results);
}

public class ChatGptSentimentService : IChatGptSentimentService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<ChatGptSentimentService> _logger;

    public ChatGptSentimentService(
        OpenAIClient openAIClient,
        ILogger<ChatGptSentimentService> logger)
    {
        _openAIClient = openAIClient;
        _logger = logger;
    }

    public async Task<List<ChatGptSentimentResult>> AnalyzeReviewsAsync(List<ReviewData> reviews)
    {
        var results = new List<ChatGptSentimentResult>();

        if (!reviews.Any())
        {
            _logger.LogWarning("No reviews provided for ChatGPT sentiment analysis");
            return results;
        }

        _logger.LogInformation("Starting ChatGPT sentiment analysis for {Count} reviews", reviews.Count);

        // Process reviews in batches to avoid token limits
        const int batchSize = 5; // Smaller batch size for ChatGPT
        var batches = reviews.Chunk(batchSize);

        foreach (var batch in batches)
        {
            try
            {
                var batchResults = await ProcessBatchAsync(batch);
                results.AddRange(batchResults);
                
                _logger.LogDebug("Processed ChatGPT batch of {Count} reviews", batch.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ChatGPT batch of reviews");
            }
        }

        _logger.LogInformation("Completed ChatGPT sentiment analysis. Processed {Count} reviews", results.Count);
        return results;
    }

    private async Task<List<ChatGptSentimentResult>> ProcessBatchAsync(IEnumerable<ReviewData> reviews)
    {
        var results = new List<ChatGptSentimentResult>();

        foreach (var review in reviews)
        {
            try
            {
                var result = await AnalyzeSingleReviewAsync(review);
                if (result != null)
                {
                    results.Add(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing review {ReviewId} with ChatGPT", review.Id);
            }
        }

        return results;
    }

    private async Task<ChatGptSentimentResult?> AnalyzeSingleReviewAsync(ReviewData review)
    {
        var cleanText = CleanTextForAnalysis(review.ReviewContent);
        
        if (string.IsNullOrWhiteSpace(cleanText))
        {
            _logger.LogWarning("Skipping review {ReviewId} - empty content after cleaning", review.Id);
            return null;
        }

        var prompt = $@"Analyze this Apple store customer review and provide a structured response.

Review: ""{cleanText}""

Please provide a JSON response with the following structure:
{{
  ""sentiment"": ""positive"", ""negative"", or ""neutral"",
  ""confidence"": 0.0-1.0,
  ""reasoning"": ""Brief explanation of why this sentiment was chosen"",
  ""keyPoints"": [""key point 1"", ""key point 2""],
  ""pros"": [""positive aspect 1"", ""positive aspect 2""],
  ""cons"": [""negative aspect 1"", ""negative aspect 2""]
}}

Focus on Apple store-specific aspects like:
- Customer service quality
- Staff knowledge and helpfulness
- Wait times and appointment availability
- Product availability and selection
- Store layout and atmosphere
- Technical support and repairs
- Overall shopping experience

Be objective and analytical.";

        try
        {
            var chatMessages = new List<ChatRequestMessage>
            {
                new ChatRequestSystemMessage("You are an expert sentiment analysis AI specializing in retail and Apple store customer reviews. Analyze reviews and provide structured JSON responses with sentiment, confidence, reasoning, key points, pros, and cons focused on retail experience."),
                new ChatRequestUserMessage(prompt)
            };

            var chatRequest = new ChatCompletionsOptions(
                deploymentName: "gpt-3.5-turbo",
                messages: chatMessages)
            {
                Temperature = 0.1f, // Low temperature for consistent analysis
                MaxTokens = 500
            };

            var response = await _openAIClient.GetChatCompletionsAsync(chatRequest);
            var content = response.Value.Choices[0].Message.Content;

            // Parse the JSON response
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var options = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    PropertyNameCaseInsensitive = true
                };
                var chatGptResponse = JsonSerializer.Deserialize<ChatGptResponse>(jsonContent, options);

                if (chatGptResponse != null)
                {
                    return new ChatGptSentimentResult
                    {
                        ReviewId = review.Id,
                        Location = review.Location,
                        ReviewContent = review.ReviewContent,
                        AuthorName = review.AuthorName,
                        StarRating = review.StarRating,
                        Sentiment = chatGptResponse.Sentiment,
                        Confidence = chatGptResponse.Confidence,
                        Reasoning = chatGptResponse.Reasoning,
                        KeyPoints = chatGptResponse.KeyPoints ?? new List<string>(),
                        Pros = chatGptResponse.Pros ?? new List<string>(),
                        Cons = chatGptResponse.Cons ?? new List<string>(),
                        AnalysisDate = DateTime.UtcNow
                    };
                }
            }

            _logger.LogWarning("Failed to parse ChatGPT response for review {ReviewId}", review.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling ChatGPT API for review {ReviewId}", review.Id);
        }

        return null;
    }

    public async Task<ChatGptSentimentSummary> GenerateSummaryAsync(string location, List<ChatGptSentimentResult> results)
    {
        var locationResults = results.Where(r => r.Location.Equals(location, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!locationResults.Any())
        {
            _logger.LogWarning("No ChatGPT sentiment results found for location: {Location}", location);
            return new ChatGptSentimentSummary { Location = location };
        }

        var summary = new ChatGptSentimentSummary
        {
            Location = location,
            TotalReviews = locationResults.Count,
            PositiveCount = locationResults.Count(r => r.Sentiment.Equals("positive", StringComparison.OrdinalIgnoreCase)),
            NegativeCount = locationResults.Count(r => r.Sentiment.Equals("negative", StringComparison.OrdinalIgnoreCase)),
            NeutralCount = locationResults.Count(r => r.Sentiment.Equals("neutral", StringComparison.OrdinalIgnoreCase)),
            AverageConfidence = locationResults.Average(r => r.Confidence),
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

        // Aggregate pros and cons from all reviews
        summary.TopPros = locationResults
            .SelectMany(r => r.Pros)
            .GroupBy(p => p.ToLower())
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.First())
            .ToList();

        summary.TopCons = locationResults
            .SelectMany(r => r.Cons)
            .GroupBy(c => c.ToLower())
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.First())
            .ToList();

        // Generate AI-powered summary
        summary.AiSummary = await GenerateAiSummaryAsync(location, locationResults);

        _logger.LogInformation("Generated ChatGPT summary for {Location}: {Sentiment} sentiment, {Total} reviews", 
            location, summary.OverallSentiment, summary.TotalReviews);

        return summary;
    }

    private async Task<string> GenerateAiSummaryAsync(string location, List<ChatGptSentimentResult> results)
    {
        var reviewsText = string.Join("\n\n", results.Take(10).Select(r => 
            $"Rating: {r.StarRating}/5\nReview: {r.ReviewContent}\nSentiment: {r.Sentiment}"));

        var prompt = $@"Based on these Apple store customer reviews for {location}, provide a concise summary:

{reviewsText}

Please provide a brief summary (1-2 paragraphs) that includes:
1. Overall customer satisfaction and sentiment
2. Key strengths and positive aspects
3. Main concerns and areas for improvement

Format the response with **bold keywords** for important terms like: **customer service**, **staff**, **wait times**, **product availability**, **store layout**, **technical support**, etc.

Focus on Apple store-specific insights and be concise.";

        try
        {
            var chatMessages = new List<ChatRequestMessage>
            {
                new ChatRequestSystemMessage("You are an expert retail analyst specializing in Apple store operations. Analyze customer reviews and provide concise, actionable insights with bold keywords for important terms."),
                new ChatRequestUserMessage(prompt)
            };

            var chatRequest = new ChatCompletionsOptions(
                deploymentName: "gpt-3.5-turbo",
                messages: chatMessages)
            {
                Temperature = 0.3f,
                MaxTokens = 800
            };

            var response = await _openAIClient.GetChatCompletionsAsync(chatRequest);
            return response.Value.Choices[0].Message.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI summary for location {Location}", location);
            return "Unable to generate AI summary due to technical issues.";
        }
    }

    private static string CleanTextForAnalysis(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Remove excessive whitespace and normalize
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
        
        // Remove very short text (likely not meaningful)
        if (text.Length < 10)
        {
            return string.Empty;
        }

        return text.Trim();
    }
}

// Response models for ChatGPT
public class ChatGptResponse
{
    public string Sentiment { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<string>? KeyPoints { get; set; }
    public List<string>? Pros { get; set; }
    public List<string>? Cons { get; set; }
}

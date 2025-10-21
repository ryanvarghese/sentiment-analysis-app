using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SentimentAnalysis.Models;
using System.Net;

namespace SentimentAnalysis.Services;

public interface ICosmosDbService
{
    Task InitializeAsync();
    Task<bool> StoreReviewsAsync(List<ReviewData> reviews);
    Task<bool> StoreSentimentResultsAsync(List<SentimentAnalysisResult> sentimentResults);
    Task<bool> StoreSentimentSummaryAsync(SentimentSummary summary);
    Task<bool> StoreChatGptResultsAsync(List<ChatGptSentimentResult> chatGptResults);
    Task<bool> StoreChatGptSummaryAsync(ChatGptSentimentSummary chatGptSummary);
    Task<bool> StoreComparisonResultAsync(SentimentComparisonResult comparisonResult);
    Task<List<ReviewData>> GetReviewsAsync(string? location = null);
    Task<List<ReviewData>> GetReviewsWithFilterAsync(string? location = null, int maxMonths = 12, int maxReviews = 1000);
    Task<List<SentimentAnalysisResult>> GetSentimentResultsAsync(string? location = null);
    Task<SentimentSummary?> GetSentimentSummaryAsync(string location);
    Task<List<ChatGptSentimentResult>> GetChatGptResultsAsync(string? location = null);
    Task<ChatGptSentimentSummary?> GetChatGptSummaryAsync(string location);
    Task<SentimentComparisonResult?> GetComparisonResultAsync(string location);
    Task<bool> UpdateReviewProcessedStatusAsync(string reviewId, string sentimentResult);
    Task<int> GetReviewCountAsync();
    Task<bool> HasReviewsAsync();
}

public class CosmosDbService : ICosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly string _databaseId;
    private readonly string _containerId;
    private readonly ILogger<CosmosDbService> _logger;
    private Container? _container;

    public CosmosDbService(
        CosmosClient cosmosClient,
        IConfiguration configuration,
        ILogger<CosmosDbService> logger)
    {
        _cosmosClient = cosmosClient;
        _databaseId = configuration["CosmosDb:DatabaseId"] ?? "SentimentAnalysisDB";
        _containerId = configuration["CosmosDb:ContainerId"] ?? "Reviews";
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Create database if it doesn't exist
            var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId);
            _logger.LogInformation("Database '{DatabaseId}' is ready", _databaseId);

            // Create container if it doesn't exist
            _container = await database.Database.CreateContainerIfNotExistsAsync(
                id: _containerId,
                partitionKeyPath: "/partitionKey");

            _logger.LogInformation("Container '{ContainerId}' is ready", _containerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Cosmos DB");
            throw;
        }
    }

    public async Task<bool> StoreReviewsAsync(List<ReviewData> reviews)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        if (!reviews.Any())
        {
            _logger.LogWarning("No reviews to store");
            return true;
        }

        var successCount = 0;
        var errorCount = 0;

        _logger.LogInformation("Starting to store {Count} reviews to Cosmos DB", reviews.Count);

        // Process reviews in batches to avoid overwhelming the service
        const int batchSize = 100;
        var batches = reviews.Chunk(batchSize);

        foreach (var batch in batches)
        {
            var tasks = batch.Select(async review =>
            {
                try
                {
                    // Ensure the review has a valid ID
                    if (string.IsNullOrEmpty(review.Id))
                    {
                        review.Id = Guid.NewGuid().ToString();
                    }

                    _logger.LogDebug("Creating document with ID: {Id}, PartitionKey: {PartitionKey}", 
                        review.Id, review.PartitionKey);

                    var response = await _container.CreateItemAsync(review);
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        Interlocked.Increment(ref successCount);
                        _logger.LogDebug("Successfully created document {Id}", review.Id);
                    }
                    else
                    {
                        Interlocked.Increment(ref errorCount);
                        _logger.LogWarning("Unexpected status code {StatusCode} for review {ReviewId}", 
                            response.StatusCode, review.Id);
                    }
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    // Document already exists, try to upsert instead
                    try
                    {
                        await _container.UpsertItemAsync(review);
                        Interlocked.Increment(ref successCount);
                        _logger.LogDebug("Upserted existing review {ReviewId}", review.Id);
                    }
                    catch (Exception upsertEx)
                    {
                        Interlocked.Increment(ref errorCount);
                        _logger.LogError(upsertEx, "Error upserting review {ReviewId}", review.Id);
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    _logger.LogError(ex, "Error storing review {ReviewId}", review.Id);
                }
            });

            await Task.WhenAll(tasks);
        }

        _logger.LogInformation("Storage completed. Success: {SuccessCount}, Errors: {ErrorCount}", 
            successCount, errorCount);

        return errorCount == 0;
    }

    public async Task<int> GetReviewCountAsync()
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        try
        {
            var query = "SELECT VALUE COUNT(1) FROM c WHERE c.reviewDate != null";
            var queryDefinition = new QueryDefinition(query);
            var iterator = _container.GetItemQueryIterator<int>(queryDefinition);
            
            var result = await iterator.ReadNextAsync();
            return result.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review count");
            return 0;
        }
    }

    public async Task<bool> HasReviewsAsync()
    {
        return await GetReviewCountAsync() > 0;
    }

    public async Task<List<ReviewData>> GetReviewsAsync(string? location = null)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        try
        {
            var query = "SELECT * FROM c WHERE c.reviewDate != null";
            if (!string.IsNullOrEmpty(location))
            {
                query += " AND c.location = @location";
            }

            var queryDefinition = new QueryDefinition(query);
            if (!string.IsNullOrEmpty(location))
            {
                queryDefinition.WithParameter("@location", location);
            }

            var iterator = _container.GetItemQueryIterator<ReviewData>(queryDefinition);
            var reviews = new List<ReviewData>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                reviews.AddRange(response);
            }

            return reviews;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews");
            return new List<ReviewData>();
        }
    }

    public async Task<List<ReviewData>> GetReviewsWithFilterAsync(string? location = null, int maxMonths = 12, int maxReviews = 1000)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        try
        {
            // Calculate the cutoff date (12 months ago)
            var cutoffDate = DateTime.UtcNow.AddMonths(-maxMonths);
            
            // Get all reviews first, then filter in memory since Cosmos DB string comparison
            // doesn't work well with different date formats
            var query = "SELECT * FROM c WHERE c.reviewDate != null";
            if (!string.IsNullOrEmpty(location))
            {
                query += " AND c.location = @location";
            }
            query += " ORDER BY c.reviewDate DESC";

            var queryDefinition = new QueryDefinition(query);
            
            if (!string.IsNullOrEmpty(location))
            {
                queryDefinition.WithParameter("@location", location);
            }

            var iterator = _container.GetItemQueryIterator<ReviewData>(queryDefinition);
            var allReviews = new List<ReviewData>();

            // Get all reviews first
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                allReviews.AddRange(response);
            }

            // Filter by date in memory and apply count limit
            var filteredReviews = new List<ReviewData>();
            var count = 0;

            foreach (var review in allReviews)
            {
                // Try to parse the date string (handles various formats)
                if (DateTime.TryParse(review.ReviewDate, out var reviewDateTime))
                {
                    // Check if review is within the time window
                    if (reviewDateTime >= cutoffDate)
                    {
                        filteredReviews.Add(review);
                        count++;
                        
                        // Stop if we've reached the max count
                        if (count >= maxReviews)
                        {
                            break;
                        }
                    }
                }
            }

            _logger.LogInformation("Retrieved {Count} reviews for location {Location} (filtered to last {Months} months, max {MaxReviews} reviews)", 
                filteredReviews.Count, location ?? "all", maxMonths, maxReviews);

            return filteredReviews;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filtered reviews");
            return new List<ReviewData>();
        }
    }

    public async Task<bool> StoreSentimentResultsAsync(List<SentimentAnalysisResult> sentimentResults)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        if (!sentimentResults.Any())
        {
            _logger.LogWarning("No sentiment results to store");
            return true;
        }

        var successCount = 0;
        var errorCount = 0;

        _logger.LogInformation("Starting to store {Count} sentiment results to Cosmos DB", sentimentResults.Count);

        foreach (var result in sentimentResults)
        {
            try
            {
                var response = await _container.CreateItemAsync(result);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Interlocked.Increment(ref successCount);
                }
                else
                {
                    Interlocked.Increment(ref errorCount);
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                // Document already exists, try to upsert instead
                try
                {
                    await _container.UpsertItemAsync(result);
                    Interlocked.Increment(ref successCount);
                }
                catch (Exception upsertEx)
                {
                    Interlocked.Increment(ref errorCount);
                    _logger.LogError(upsertEx, "Error upserting sentiment result {Id}", result.Id);
                }
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref errorCount);
                _logger.LogError(ex, "Error storing sentiment result {Id}", result.Id);
            }
        }

        _logger.LogInformation("Sentiment results storage completed. Success: {SuccessCount}, Errors: {ErrorCount}", 
            successCount, errorCount);

        return errorCount == 0;
    }

    public async Task<bool> StoreSentimentSummaryAsync(SentimentSummary summary)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        try
        {
            var response = await _container.UpsertItemAsync(summary, new PartitionKey(summary.PartitionKey));
            _logger.LogInformation("Successfully stored sentiment summary for {Location}", summary.Location);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing sentiment summary for {Location}", summary.Location);
            return false;
        }
    }

    public async Task<List<SentimentAnalysisResult>> GetSentimentResultsAsync(string? location = null)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        try
        {
            var query = "SELECT * FROM c WHERE c.reviewId != null";
            if (!string.IsNullOrEmpty(location))
            {
                query += " AND c.location = @location";
            }

            var queryDefinition = new QueryDefinition(query);
            if (!string.IsNullOrEmpty(location))
            {
                queryDefinition.WithParameter("@location", location);
            }

            var iterator = _container.GetItemQueryIterator<SentimentAnalysisResult>(queryDefinition);
            var results = new List<SentimentAnalysisResult>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sentiment results");
            return new List<SentimentAnalysisResult>();
        }
    }

    public async Task<SentimentSummary?> GetSentimentSummaryAsync(string location)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        try
        {
            var query = "SELECT * FROM c WHERE c.location = @location AND c.totalReviews != null";
            var queryDefinition = new QueryDefinition(query).WithParameter("@location", location);
            var iterator = _container.GetItemQueryIterator<SentimentSummary>(queryDefinition);

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                return response.FirstOrDefault();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sentiment summary for {Location}", location);
            return null;
        }
    }

    public async Task<bool> UpdateReviewProcessedStatusAsync(string reviewId, string sentimentResult)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        try
        {
            // Find the review to get the correct partition key (location)
            var query = "SELECT * FROM c WHERE c.id = @reviewId";
            var queryDefinition = new QueryDefinition(query).WithParameter("@reviewId", reviewId);
            var iterator = _container.GetItemQueryIterator<ReviewData>(queryDefinition);

            ReviewData? review = null;
            while (iterator.HasMoreResults && review == null)
            {
                var page = await iterator.ReadNextAsync();
                review = page.FirstOrDefault();
            }

            if (review == null)
            {
                _logger.LogWarning("Review {ReviewId} not found for update", reviewId);
                return false;
            }

            // Update fields
            review.Processed = true;
            review.SentimentResult = sentimentResult;

            await _container.UpsertItemAsync(review);
            _logger.LogDebug("Updated review {ReviewId}: processed=true, sentimentResult={Sentiment}", reviewId, sentimentResult);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating processed status for review {ReviewId}", reviewId);
            return false;
        }
    }

    public async Task<bool> StoreChatGptResultsAsync(List<ChatGptSentimentResult> chatGptResults)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        if (!chatGptResults.Any())
        {
            _logger.LogWarning("No ChatGPT results to store");
            return true;
        }

        var successCount = 0;
        var errorCount = 0;

        _logger.LogInformation("Starting to store {Count} ChatGPT results to Cosmos DB", chatGptResults.Count);

        foreach (var result in chatGptResults)
        {
            try
            {
                var response = await _container.CreateItemAsync(result);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Interlocked.Increment(ref successCount);
                }
                else
                {
                    Interlocked.Increment(ref errorCount);
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                try
                {
                    await _container.UpsertItemAsync(result);
                    Interlocked.Increment(ref successCount);
                }
                catch (Exception upsertEx)
                {
                    Interlocked.Increment(ref errorCount);
                    _logger.LogError(upsertEx, "Error upserting ChatGPT result {Id}", result.Id);
                }
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref errorCount);
                _logger.LogError(ex, "Error storing ChatGPT result {Id}", result.Id);
            }
        }

        _logger.LogInformation("ChatGPT results storage completed. Success: {SuccessCount}, Errors: {ErrorCount}", 
            successCount, errorCount);

        return errorCount == 0;
    }

    public async Task<bool> StoreChatGptSummaryAsync(ChatGptSentimentSummary chatGptSummary)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        try
        {
            var response = await _container.UpsertItemAsync(chatGptSummary, new PartitionKey(chatGptSummary.PartitionKey));
            _logger.LogInformation("Successfully stored ChatGPT summary for {Location}", chatGptSummary.Location);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing ChatGPT summary for {Location}", chatGptSummary.Location);
            return false;
        }
    }

    public async Task<bool> StoreComparisonResultAsync(SentimentComparisonResult comparisonResult)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        try
        {
            var response = await _container.UpsertItemAsync(comparisonResult, new PartitionKey(comparisonResult.PartitionKey));
            _logger.LogInformation("Successfully stored comparison result for {Location}", comparisonResult.Location);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing comparison result for {Location}", comparisonResult.Location);
            return false;
        }
    }

    public async Task<List<ChatGptSentimentResult>> GetChatGptResultsAsync(string? location = null)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        try
        {
            var query = "SELECT * FROM c WHERE c.reviewId != null AND c.reasoning != null";
            if (!string.IsNullOrEmpty(location))
            {
                query += " AND c.location = @location";
            }

            var queryDefinition = new QueryDefinition(query);
            if (!string.IsNullOrEmpty(location))
            {
                queryDefinition.WithParameter("@location", location);
            }

            var iterator = _container.GetItemQueryIterator<ChatGptSentimentResult>(queryDefinition);
            var results = new List<ChatGptSentimentResult>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ChatGPT results");
            return new List<ChatGptSentimentResult>();
        }
    }

    public async Task<ChatGptSentimentSummary?> GetChatGptSummaryAsync(string location)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        try
        {
            var query = "SELECT * FROM c WHERE c.location = @location AND c.aiSummary != null";
            var queryDefinition = new QueryDefinition(query).WithParameter("@location", location);
            var iterator = _container.GetItemQueryIterator<ChatGptSentimentSummary>(queryDefinition);

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                return response.FirstOrDefault();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ChatGPT summary for {Location}", location);
            return null;
        }
    }

    public async Task<SentimentComparisonResult?> GetComparisonResultAsync(string location)
    {
        if (_container == null)
        {
            await InitializeAsync();
        }

        try
        {
            var query = "SELECT * FROM c WHERE c.location = @location AND c.comparisonMetrics != null";
            var queryDefinition = new QueryDefinition(query).WithParameter("@location", location);
            var iterator = _container.GetItemQueryIterator<SentimentComparisonResult>(queryDefinition);

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                return response.FirstOrDefault();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comparison result for {Location}", location);
            return null;
        }
    }
}

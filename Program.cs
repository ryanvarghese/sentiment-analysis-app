using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Azure.AI.OpenAI;
using SentimentAnalysis.Services;
using SentimentAnalysis.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// DI registrations
builder.Services.AddSingleton(provider =>
{
    var configuration = builder.Configuration;
    var connectionString = configuration["CosmosDb:ConnectionString"];
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Cosmos DB connection string is not configured");
    }
    var cosmosClientOptions = new CosmosClientOptions()
    {
        SerializerOptions = new CosmosSerializationOptions()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };
    return new CosmosClient(connectionString, cosmosClientOptions);
});

builder.Services.AddSingleton<TextAnalyticsClient>(provider =>
{
    var configuration = builder.Configuration;
    var endpoint = configuration["AzureCognitiveServices:Endpoint"];
    var apiKey = configuration["AzureCognitiveServices:ApiKey"];
    if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException("Azure Cognitive Services endpoint and API key must be configured");
    }
    return new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
});

builder.Services.AddSingleton<OpenAIClient>(provider =>
{
    var configuration = builder.Configuration;
    var apiKey = configuration["OpenAI:ApiKey"];
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException("OpenAI API key must be configured");
    }
    return new OpenAIClient(apiKey);
});

builder.Services.AddScoped<ICsvParserService, CsvParserService>();
builder.Services.AddScoped<ICosmosDbService, CosmosDbService>();
builder.Services.AddScoped<ISentimentAnalysisService, SentimentAnalysisService>();
builder.Services.AddScoped<IChatGptSentimentService, ChatGptSentimentService>();
builder.Services.AddScoped<ISentimentComparisonService, SentimentComparisonService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Ensure Cosmos resources exist
using (var scope = app.Services.CreateScope())
{
    var cosmos = scope.ServiceProvider.GetRequiredService<ICosmosDbService>();
    await cosmos.InitializeAsync();
}

// No need for uploads directory - we store everything in Cosmos DB

app.MapGet("/", async (HttpContext context, ICosmosDbService cosmos) =>
{
    // Get all unique locations from Cosmos DB (using filtered data)
    var allReviews = await cosmos.GetReviewsWithFilterAsync();
    var locations = allReviews.Select(r => r.Location).Distinct().OrderBy(l => l).ToList();

    var sb = new System.Text.StringBuilder();
    sb.Append("<html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'>");
    sb.Append("<title>Sentiment Analyzer</title>");
    sb.Append("<style>");
    sb.Append("body{font-family:Segoe UI,Arial,sans-serif;background:#f6f7fb;color:#1f2937;margin:0}");
    sb.Append(".container{max-width:960px;margin:40px auto;padding:0 16px}");
    sb.Append(".card{background:#fff;border-radius:12px;box-shadow:0 8px 24px rgba(0,0,0,.08);padding:20px;margin-bottom:20px}");
    sb.Append("h1{font-size:24px;margin:0 0 12px}");
    sb.Append(".muted{color:#6b7280}");
    sb.Append(".row{display:flex;gap:16px;flex-wrap:wrap}");
    sb.Append(".btn{appearance:none;border:0;border-radius:10px;padding:10px 14px;background:#2563eb;color:#fff;cursor:pointer;font-weight:600}");
    sb.Append(".btn.secondary{background:#111827;color:#fff}");
    sb.Append(".btn:disabled{opacity:.5;cursor:not-allowed}");
    sb.Append("input[type=file]{padding:10px;border:1px solid #e5e7eb;border-radius:10px;background:#fff}");
    sb.Append("table{width:100%;border-collapse:separate;border-spacing:0 8px}");
    sb.Append("th,td{text-align:left;padding:12px 14px;background:#fff}");
    sb.Append("th{color:#374151;background:#f3f4f6}");
    sb.Append("tr td:first-child{border-radius:10px 0 0 10px}");
    sb.Append("tr td:last-child{border-radius:0 10px 10px 0}");
    sb.Append(".right{float:right}");
    sb.Append("a{color:#2563eb;text-decoration:none}");
    sb.Append("</style></head><body>");
    sb.Append("<div class='container'>");
    sb.Append("<div class='card' style='background:#ff6b6b;color:white;text-align:center;font-weight:bold;'>");
    sb.Append("🧪 CI/CD TEST - DEPLOYMENT VERIFIED! 🚀");
    sb.Append("</div>");
    sb.Append("<div class='card'>");
    sb.Append("<h1>Import CSV Data</h1>");
    sb.Append("<p class='muted'>Upload an Apple-Location.csv file to import data into Cosmos DB.</p>");
    sb.Append("<form method='post' enctype='multipart/form-data' action='/upload' class='row'>");
    sb.Append("<input type='file' name='file' accept='.csv' required />");
    sb.Append("<button class='btn' type='submit'>Import to Database</button>");
    sb.Append("</form>");
    sb.Append("</div>");

    sb.Append("<div class='card'>");
    sb.Append("<h1>Analyze Imported Data</h1>");
    if (locations.Count == 0)
    {
        sb.Append("<p class='muted'>No data imported yet. Upload a CSV file first.</p>");
    }
    else
    {
        sb.Append("<p class='muted'>Click ChatGPT for AI summary, Azure for traditional analysis, or Hybrid for Azure + AI summary.</p>");
        sb.Append("<table><tr><th>Location</th><th style='width:420px'>Actions</th></tr>");
        foreach (var location in locations)
        {
            var safe = System.Net.WebUtility.HtmlEncode(location);
            sb.Append($"<tr><td>{safe}</td><td>");
            sb.Append($"<form method='get' action='/compare' style='display:inline-block;margin-right:8px'>");
            sb.Append($"<input type='hidden' name='location' value='{safe}'/>");
            sb.Append($"<button class='btn' type='submit'>ChatGPT</button>");
            sb.Append("</form>");
            sb.Append($"<form method='get' action='/hybrid' style='display:inline-block;margin-right:8px'>");
            sb.Append($"<input type='hidden' name='location' value='{safe}'/>");
            sb.Append($"<button class='btn' type='submit'>Hybrid</button>");
            sb.Append("</form>");
            sb.Append($"<form method='get' action='/results' style='display:inline-block'>");
            sb.Append($"<input type='hidden' name='location' value='{safe}'/>");
            sb.Append($"<button class='btn secondary' type='submit'>Azure</button>");
            sb.Append("</form>");
            sb.Append("</td></tr>");
        }
        sb.Append("</table>");
    }
    sb.Append("</div>");
    sb.Append("</div></body></html>");
    await context.Response.WriteAsync(sb.ToString());
});

app.MapPost("/upload", async (HttpRequest request, ICsvParserService csvParser, ICosmosDbService cosmos) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Invalid form content type");
    }

    var form = await request.ReadFormAsync();
    var file = form.Files["file"];
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("No file uploaded");
    }

    // Parse CSV directly from uploaded stream and import to Cosmos DB
    using var stream = file.OpenReadStream();
    var fileName = file.FileName;
    var reviews = await csvParser.ParseCsvStreamAsync(stream, fileName);
    if (!reviews.Any())
    {
        return Results.BadRequest("No reviews found in CSV");
    }

    var stored = await cosmos.StoreReviewsAsync(reviews);
    if (!stored)
    {
        return Results.Problem("Failed to store reviews in Cosmos DB");
    }

    return Results.Redirect("/");
});

// Results page (separate page for analysis output)
app.MapGet("/results", async (HttpRequest request, ICosmosDbService cosmos, ISentimentAnalysisService sentiment) =>
{
    var location = request.Query["location"].ToString();
    if (string.IsNullOrWhiteSpace(location))
    {
        return Results.BadRequest("No location specified");
    }
    
    // Get reviews from Cosmos DB for this location (filtered to last 12 months, max 1000)
    var reviews = await cosmos.GetReviewsWithFilterAsync(location);
    if (!reviews.Any())
    {
        return Results.NotFound($"No reviews found for location: {location}");
    }

    // Perform sentiment analysis on the imported reviews
    var results = await sentiment.AnalyzeReviewsAsync(reviews);
    if (results.Any())
    {
        await cosmos.StoreSentimentResultsAsync(results);
    }

    // Generate summary for the location
    var summary = await sentiment.GenerateSummaryAsync(location, results);
    await cosmos.StoreSentimentSummaryAsync(summary);

    var summaryHtml = new System.Text.StringBuilder();
    summaryHtml.Append("<html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'>");
    summaryHtml.Append("<title>Results - Sentiment Analyzer</title>");
    summaryHtml.Append("<style>");
    summaryHtml.Append("body{font-family:Segoe UI,Arial,sans-serif;background:#f6f7fb;color:#1f2937;margin:0}");
    summaryHtml.Append(".container{max-width:960px;margin:40px auto;padding:0 16px}");
    summaryHtml.Append(".card{background:#fff;border-radius:12px;box-shadow:0 8px 24px rgba(0,0,0,.08);padding:20px;margin-bottom:20px}");
    summaryHtml.Append("h1{font-size:24px;margin:0 0 12px}");
    summaryHtml.Append(".badge{display:inline-block;padding:4px 10px;border-radius:999px;font-weight:700}");
    summaryHtml.Append(".pos{background:#dcfce7;color:#166534}.neg{background:#fee2e2;color:#991b1b}.neu{background:#e5e7eb;color:#374151}");
    summaryHtml.Append(".muted{color:#6b7280}");
    summaryHtml.Append(".list{display:flex;gap:8px;flex-wrap:wrap}");
    summaryHtml.Append(".chip{background:#f3f4f6;border-radius:999px;padding:6px 10px}");
    summaryHtml.Append(".btn{appearance:none;border:0;border-radius:10px;padding:10px 14px;background:#111827;color:#fff;cursor:pointer;font-weight:600}");
    summaryHtml.Append("a{color:#2563eb;text-decoration:none}");
    summaryHtml.Append("</style></head><body>");
    summaryHtml.Append("<div class='container'>");
    summaryHtml.Append("<div class='card'>");
    summaryHtml.Append($"<h1>Sentiment Analysis Results</h1><p class='muted'>Location: {System.Net.WebUtility.HtmlEncode(location)}</p>");
    
    var sentimentClass = summary.OverallSentiment switch { "Positive" => "pos", "Negative" => "neg", _ => "neu" };
    summaryHtml.Append($"<div class='card'><h2 style='margin-top:0'>{System.Net.WebUtility.HtmlEncode(location)} <span class='badge {sentimentClass}'>{summary.OverallSentiment}</span></h2>");
    summaryHtml.Append($"<p class='muted'>Total Reviews: {summary.TotalReviews} · + {summary.PositiveCount} · - {summary.NegativeCount} · neutral {summary.NeutralCount} · Avg confidence {summary.AverageConfidence:P2}</p>");
    if (summary.TopPros.Any())
    {
        summaryHtml.Append("<div><strong>Top Pros</strong><div class='list'>" + string.Join("", summary.TopPros.Select(p => $"<span class='chip'>{System.Net.WebUtility.HtmlEncode(p)}</span>")) + "</div></div>");
    }
    if (summary.TopCons.Any())
    {
        summaryHtml.Append("<div style='margin-top:8px'><strong>Top Cons</strong><div class='list'>" + string.Join("", summary.TopCons.Select(p => $"<span class='chip'>{System.Net.WebUtility.HtmlEncode(p)}</span>")) + "</div></div>");
    }
    summaryHtml.Append("</div>");
    summaryHtml.Append("<a class='btn' href='/'>&larr; Back</a>");
    summaryHtml.Append("</div></div></body></html>");

    return Results.Content(summaryHtml.ToString(), "text/html");
});

// Azure + ChatGPT results endpoint
app.MapGet("/compare", async (HttpRequest request, ICosmosDbService cosmos, ISentimentAnalysisService azureService, IChatGptSentimentService chatGptService) =>
{
    var location = request.Query["location"].ToString();
    if (string.IsNullOrWhiteSpace(location))
    {
        return Results.BadRequest("No location specified");
    }
    
    // Get reviews from Cosmos DB for this location (filtered to last 12 months, max 1000)
    var reviews = await cosmos.GetReviewsWithFilterAsync(location);
    if (!reviews.Any())
    {
        return Results.NotFound($"No reviews found for location: {location}");
    }

    // Run ChatGPT analysis
    var chatGptResults = await chatGptService.AnalyzeReviewsAsync(reviews);
    var chatGptSummary = await chatGptService.GenerateSummaryAsync(location, chatGptResults);

    // Store results
    if (chatGptResults.Any())
    {
        await cosmos.StoreChatGptResultsAsync(chatGptResults);
    }
    await cosmos.StoreChatGptSummaryAsync(chatGptSummary);

    var resultsHtml = new System.Text.StringBuilder();
    resultsHtml.Append("<html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'>");
    resultsHtml.Append("<title>ChatGPT Analysis Results - Sentiment Analyzer</title>");
    resultsHtml.Append("<style>");
    resultsHtml.Append("body{font-family:Segoe UI,Arial,sans-serif;background:#f6f7fb;color:#1f2937;margin:0}");
    resultsHtml.Append(".container{max-width:960px;margin:40px auto;padding:0 16px}");
    resultsHtml.Append(".card{background:#fff;border-radius:12px;box-shadow:0 8px 24px rgba(0,0,0,.08);padding:20px;margin-bottom:20px}");
    resultsHtml.Append("h1{font-size:24px;margin:0 0 12px}");
    resultsHtml.Append("h2{font-size:20px;margin:16px 0 8px}");
    resultsHtml.Append(".badge{display:inline-block;padding:4px 10px;border-radius:999px;font-weight:700}");
    resultsHtml.Append(".pos{background:#dcfce7;color:#166534}.neg{background:#fee2e2;color:#991b1b}.neu{background:#e5e7eb;color:#374151}");
    resultsHtml.Append(".muted{color:#6b7280}");
    resultsHtml.Append(".list{display:flex;gap:8px;flex-wrap:wrap}");
    resultsHtml.Append(".chip{background:#f3f4f6;border-radius:999px;padding:6px 10px}");
    resultsHtml.Append(".btn{appearance:none;border:0;border-radius:10px;padding:10px 14px;background:#111827;color:#fff;cursor:pointer;font-weight:600}");
    resultsHtml.Append("a{color:#2563eb;text-decoration:none}");
    resultsHtml.Append("</style></head><body>");
    resultsHtml.Append("<div class='container'>");
    resultsHtml.Append("<div class='card'>");
    resultsHtml.Append($"<h1>ChatGPT Analysis Results</h1><p class='muted'>Location: {System.Net.WebUtility.HtmlEncode(location)}</p>");
    
    // ChatGPT Results Only
    var chatGptSentimentClass = chatGptSummary.OverallSentiment switch { "Positive" => "pos", "Negative" => "neg", _ => "neu" };
    resultsHtml.Append($"<div class='card'><h2>{System.Net.WebUtility.HtmlEncode(location)} <span class='badge {chatGptSentimentClass}'>{chatGptSummary.OverallSentiment}</span></h2>");
    resultsHtml.Append($"<p class='muted'>Total Reviews: {chatGptSummary.TotalReviews} · + {chatGptSummary.PositiveCount} · - {chatGptSummary.NegativeCount} · neutral {chatGptSummary.NeutralCount} · Avg confidence: {chatGptSummary.AverageConfidence:P1}</p>");
    if (chatGptSummary.TopPros.Any())
    {
        resultsHtml.Append("<div><strong>Top Pros:</strong><div class='list'>" + string.Join("", chatGptSummary.TopPros.Select(p => $"<span class='chip'>{System.Net.WebUtility.HtmlEncode(p)}</span>")) + "</div></div>");
    }
    if (chatGptSummary.TopCons.Any())
    {
        resultsHtml.Append("<div style='margin-top:8px'><strong>Top Cons:</strong><div class='list'>" + string.Join("", chatGptSummary.TopCons.Select(p => $"<span class='chip'>{System.Net.WebUtility.HtmlEncode(p)}</span>")) + "</div></div>");
    }
    if (!string.IsNullOrEmpty(chatGptSummary.AiSummary))
    {
        var formattedSummary = ConvertMarkdownToHtml(chatGptSummary.AiSummary);
        resultsHtml.Append($"<div style='margin-top:12px'><strong>AI Summary:</strong><p>{formattedSummary}</p></div>");
    }
    resultsHtml.Append("</div>");

    resultsHtml.Append("<a class='btn' href='/'>&larr; Back</a>");
    resultsHtml.Append("</div></div></body></html>");

    return Results.Content(resultsHtml.ToString(), "text/html");
});

// Hybrid endpoint: Azure aggregates + single ChatGPT summary
app.MapGet("/hybrid", async (HttpRequest request, ICosmosDbService cosmos, ISentimentAnalysisService azureService, IChatGptSentimentService chatGptService) =>
{
    var location = request.Query["location"].ToString();
    if (string.IsNullOrWhiteSpace(location))
    {
        return Results.BadRequest("No location specified");
    }

    // Get reviews from Cosmos DB for this location (filtered to last 12 months, max 1000)
    var reviews = await cosmos.GetReviewsWithFilterAsync(location);
    if (!reviews.Any())
    {
        return Results.NotFound($"No reviews found for location: {location}");
    }

    // Azure across all selected reviews
    var azureResults = await azureService.AnalyzeReviewsAsync(reviews);
    var azureSummary = await azureService.GenerateSummaryAsync(location, azureResults);

    // Build a small set of representative quotes (simple heuristic: top 12 recent non-empty)
    var quotes = reviews
        .Where(r => !string.IsNullOrWhiteSpace(r.ReviewContent))
        .OrderByDescending(r => r.ReviewDate)
        .Take(12)
        .Select(r => new { r.StarRating, r.ReviewContent })
        .ToList();

    // Convert Azure results to ChatGPT-friendly inputs by creating minimal ChatGPT results
    var pseudoChatGptResults = azureResults.Select(r => new ChatGptSentimentResult
    {
        ReviewId = r.ReviewId,
        Location = r.Location,
        ReviewContent = r.ReviewContent,
        AuthorName = r.AuthorName,
        StarRating = r.StarRating,
        Sentiment = r.Sentiment.ToLowerInvariant(),
        Confidence = r.Confidence,
        Pros = new List<string>(),
        Cons = new List<string>(),
        KeyPoints = new List<string>(),
        AnalysisDate = r.AnalysisDate
    }).ToList();

    var chatGptSummary = await chatGptService.GenerateSummaryAsync(location, pseudoChatGptResults);

    // Store
    if (azureResults.Any()) await cosmos.StoreSentimentResultsAsync(azureResults);
    await cosmos.StoreSentimentSummaryAsync(azureSummary);
    await cosmos.StoreChatGptSummaryAsync(chatGptSummary);

    // Render
    var sb = new System.Text.StringBuilder();
    sb.Append("<html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'>");
    sb.Append("<title>Hybrid Results - Sentiment Analyzer</title>");
    sb.Append("<style>");
    sb.Append("body{font-family:Segoe UI,Arial,sans-serif;background:#f6f7fb;color:#1f2937;margin:0}");
    sb.Append(".container{max-width:1200px;margin:40px auto;padding:0 16px}");
    sb.Append(".card{background:#fff;border-radius:12px;box-shadow:0 8px 24px rgba(0,0,0,.08);padding:20px;margin-bottom:20px}");
    sb.Append("h1{font-size:24px;margin:0 0 12px}");
    sb.Append(".badge{display:inline-block;padding:4px 10px;border-radius:999px;font-weight:700}");
    sb.Append(".pos{background:#dcfce7;color:#166534}.neg{background:#fee2e2;color:#991b1b}.neu{background:#e5e7eb;color:#374151}");
    sb.Append(".muted{color:#6b7280}");
    sb.Append(".list{display:flex;gap:8px;flex-wrap:wrap}");
    sb.Append(".chip{background:#f3f4f6;border-radius:999px;padding:6px 10px}");
    sb.Append(".btn{appearance:none;border:0;border-radius:10px;padding:10px 14px;background:#111827;color:#fff;cursor:pointer;font-weight:600}");
    sb.Append("a{color:#2563eb;text-decoration:none}");
    sb.Append("</style></head><body>");
    sb.Append("<div class='container'>");
    sb.Append("<div class='card'>");
    sb.Append($"<h1>Hybrid Results</h1><p class='muted'>Location: {System.Net.WebUtility.HtmlEncode(location)}</p>");

    var aClass = azureSummary.OverallSentiment switch { "Positive" => "pos", "Negative" => "neg", _ => "neu" };
    sb.Append($"<div class='card'><h2>Azure Aggregates <span class='badge {aClass}'>{azureSummary.OverallSentiment}</span></h2>");
    sb.Append($"<p class='muted'>Total Reviews: {azureSummary.TotalReviews} · + {azureSummary.PositiveCount} · - {azureSummary.NegativeCount} · neutral {azureSummary.NeutralCount} · Avg confidence {azureSummary.AverageConfidence:P1}</p>");
    if (azureSummary.TopPros.Any()) sb.Append("<div><strong>Top Pros</strong><div class='list'>" + string.Join("", azureSummary.TopPros.Select(p => $"<span class='chip'>{System.Net.WebUtility.HtmlEncode(p)}</span>")) + "</div></div>");
    if (azureSummary.TopCons.Any()) sb.Append("<div style='margin-top:8px'><strong>Top Cons</strong><div class='list'>" + string.Join("", azureSummary.TopCons.Select(p => $"<span class='chip'>{System.Net.WebUtility.HtmlEncode(p)}</span>")) + "</div></div>");
    sb.Append("</div>");

    var gClass = chatGptSummary.OverallSentiment switch { "Positive" => "pos", "Negative" => "neg", _ => "neu" };
    sb.Append($"<div class='card'><h2>ChatGPT Summary <span class='badge {gClass}'>{chatGptSummary.OverallSentiment}</span></h2>");
    if (!string.IsNullOrEmpty(chatGptSummary.AiSummary))
    {
        var formatted = ConvertMarkdownToHtml(chatGptSummary.AiSummary);
        sb.Append($"<div style='margin-top:8px'><strong>AI Summary:</strong><p>{formatted}</p></div>");
    }
    sb.Append("</div>");

    sb.Append("<a class='btn' href='/'>&larr; Back</a>");
    sb.Append("</div></div></body></html>");
    return Results.Content(sb.ToString(), "text/html");
});

// Helper function to convert markdown to HTML
static string ConvertMarkdownToHtml(string markdown)
{
    if (string.IsNullOrEmpty(markdown))
        return string.Empty;
    
    // Convert **text** to <strong>text</strong>
    var html = System.Text.RegularExpressions.Regex.Replace(
        markdown, 
        @"\*\*(.*?)\*\*", 
        "<strong>$1</strong>"
    );
    
    // Convert *text* to <em>text</em>
    html = System.Text.RegularExpressions.Regex.Replace(
        html, 
        @"\*(.*?)\*", 
        "<em>$1</em>"
    );
    
    // Convert line breaks to <br>
    html = html.Replace("\n", "<br>");
    
    return html;
}

// Configure for Azure App Service
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();

# Sentiment Analysis Data Processor

This application parses CSV files from the `Datasets` folder and stores the data in Azure Cosmos DB with an additional `location` column extracted from the filename.

## Features

- Parses CSV files with pattern `Apple-{Location}.csv`
- Extracts location information from filenames
- Stores data in Azure Cosmos DB with location as partition key
- Handles batch processing for large datasets
- Comprehensive logging and error handling

## Prerequisites

- .NET 8.0 SDK
- Azure Cosmos DB account
- CSV files in the `Datasets` folder following the naming pattern: `Apple-{Location}.csv`

## Setup

1. **Configure Azure Cosmos DB Connection**
   - Open `appsettings.json`
   - Replace `YOUR_COSMOS_DB_CONNECTION_STRING_HERE` with your actual Cosmos DB connection string
   - Optionally modify the database and container names

2. **Prepare CSV Files**
   - Place your CSV files in the `Datasets` folder
   - Ensure files follow the naming pattern: `Apple-{Location}.csv`
   - CSV files should have the following columns:
     - Review date
     - Author name
     - Star rating
     - Review content

3. **Install Dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

## Configuration

The application uses the following configuration in `appsettings.json`:

```json
{
  "CosmosDb": {
    "ConnectionString": "YOUR_COSMOS_DB_CONNECTION_STRING_HERE",
    "DatabaseId": "SentimentAnalysisDB",
    "ContainerId": "Reviews"
  },
  "Datasets": {
    "Path": "Datasets"
  }
}
```

## Data Structure

The application stores data in Cosmos DB with the following structure:

```json
{
  "id": "unique-guid",
  "reviewDate": "9/3/2025",
  "authorName": "Nick Ar",
  "starRating": 1,
  "reviewContent": "Review text...",
  "location": "Alderwood",
  "partitionKey": "Alderwood"
}
```

## Location Extraction

The application extracts location information from filenames using the pattern `Apple-{Location}.csv`:
- `Apple-Alderwood.csv` → Location: "Alderwood"
- `Apple-Bellevue Square.csv` → Location: "Bellevue Square"
- `Apple-Southcenter.csv` → Location: "Southcenter"
- `Apple-University Village.csv` → Location: "University Village"

## Error Handling

- Invalid CSV files are logged and skipped
- Duplicate records are handled with upsert operations
- Network issues are retried automatically
- Comprehensive logging for troubleshooting

## Logging

The application provides detailed logging including:
- File processing progress
- Data parsing statistics
- Cosmos DB operations
- Error details and stack traces

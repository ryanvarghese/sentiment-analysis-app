# Use the official .NET 8 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SentimentAnalysis.csproj", "."]
RUN dotnet restore "SentimentAnalysis.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "SentimentAnalysis.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SentimentAnalysis.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SentimentAnalysis.dll"]



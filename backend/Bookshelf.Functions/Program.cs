using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

// Logi funkcji -> Application Insights (appi-bookshelf).
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// SDK Application Insights domyślnie przepuszcza tylko Warning i wyżej.
// Usuwamy tę regułę, żeby LogInformation z funkcji był widoczny w App Insights.
builder.Logging.Services.Configure<LoggerFilterOptions>(options =>
{
    var rule = options.Rules.FirstOrDefault(r =>
        r.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
    if (rule is not null) options.Rules.Remove(rule);
});

// Tabela historii transakcji: to samo konto Storage co kolejka (ustawienie StorageConnection).
var storageConnection = builder.Configuration["StorageConnection"];
var history = new TableClient(storageConnection, "TransactionHistory");
history.CreateIfNotExists(); // w Azure już istnieje (krok 1.1), w Azurite powstanie
builder.Services.AddSingleton(history);

// Miniatury: odczyt z covers, zapis do thumbnails
var blobs = new BlobServiceClient(storageConnection);
blobs.GetBlobContainerClient("thumbnails").CreateIfNotExists();
builder.Services.AddSingleton(blobs);

// Powiadomienia: Cosmos DB, kontener z ustawienia CosmosContainer (notifications w Azure, notifications-dev lokalnie).
// Brak CosmosConnection albo błędny format -> funkcja dalej zapisuje historię, a powiadomienia pomija.
// Błędny format nie może wywrócić całego Function App (historia i miniatury przestałyby działać).
var cosmosConnection = builder.Configuration["CosmosConnection"];
Container? notifications = null;
if (!string.IsNullOrWhiteSpace(cosmosConnection))
{
    try
    {
        var cosmos = new CosmosClient(cosmosConnection);
        notifications = cosmos.GetContainer(
            builder.Configuration["CosmosDatabase"] ?? "bookshelf",
            builder.Configuration["CosmosContainer"] ?? "notifications");
    }
    catch (Exception ex)
    {
        // Tylko typ wyjątku: komunikat może zawierać fragment klucza
        Console.Error.WriteLine($"CosmosConnection ma nieprawidłowy format ({ex.GetType().Name}) — powiadomienia wyłączone");
    }
}
builder.Services.AddSingleton(new Bookshelf.Functions.NotificationsContainer(notifications));

builder.Build().Run();
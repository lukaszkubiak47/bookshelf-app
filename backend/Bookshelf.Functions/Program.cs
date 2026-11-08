using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Azure.Data.Tables;
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
var history = new TableClient(builder.Configuration["StorageConnection"], "TransactionHistory");
history.CreateIfNotExists(); // w Azure już istnieje (krok 1.1), w Azurite powstanie
builder.Services.AddSingleton(history);

builder.Build().Run();
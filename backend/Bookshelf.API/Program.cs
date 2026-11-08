using Bookshelf.API.Configuration;
using Bookshelf.API.Data;
using Bookshelf.API.Middleware;
using Bookshelf.API.RateLimiting;
using Bookshelf.API.Services;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Bookshelf.API.Services.Storage;
using Bookshelf.API.Services.Events;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// KONFIGURACJA 
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<GoogleBooksSettings>(builder.Configuration.GetSection("GoogleBooks"));

// BAZA DANYCH
// EnableRetryOnFailure: Azure SQL zwraca chwilowe bledy (np. wybudzanie bazy free tier po auto-pauzie) -> EF ponawia zapytanie.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure()));

// CORS — frontend (osobna aplikacja w App Service) ma inny adres niz API.
// Lista dozwolonych adresow w konfiguracji: Cors:AllowedOrigins (w Azure: App Setting Cors__AllowedOrigins__0).
const string FrontendCorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// REDIS — cache rozproszony + licznik limitu prób, wspólne dla wszystkich instancji API.
// Brak ConnectionStrings:Redis (lokalny dev) -> zamienniki w pamięci procesu, aplikacja działa bez Redisa.
builder.Services.AddMemoryCache();
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
    redisOptions.AbortOnConnectFail = false; // chwilowy brak Redisa przy starcie nie wywraca API
    var redis = ConnectionMultiplexer.Connect(redisOptions);

    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConnectionMultiplexerFactory = () => Task.FromResult<IConnectionMultiplexer>(redis);
        options.InstanceName = "bookshelf:";
    });
    builder.Services.AddSingleton<IRateLimitStore, RedisRateLimitStore>();
}
else
{
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSingleton<IRateLimitStore, InMemoryRateLimitStore>();
}

builder.Services.AddSingleton<ICacheService, CacheService>();

// STORAGE — kolejka zdarzeń transakcji (historia w Functions, w T3 powiadomienia).
// Brak ConnectionStrings:Storage -> zamiennik, który tylko loguje; API działa bez Azure.
var storageConnection = builder.Configuration.GetConnectionString("Storage");

if (!string.IsNullOrWhiteSpace(storageConnection))
{
    // Base64 jest wymagane: QueueTrigger w Functions oczekuje Base64,
    // bez tego komunikaty lądują w kolejce transaction-events-poison.
    var queue = new QueueClient(storageConnection, "transaction-events",
        new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });

    // Pomijane przy "dotnet ef" (migracje) — narzędzie uruchamia Program.cs, a Azurite nie musi wtedy działać
    if (!EF.IsDesignTime)
        queue.CreateIfNotExists(); // w Azure już istnieje (utworzona w portalu), w Azurite powstanie przy starcie
    builder.Services.AddSingleton(queue);
    builder.Services.AddSingleton<IEventPublisher, QueueEventPublisher>();

    // Obrazy: kontenery tworzone przy starcie (w Azure już istnieją, w Azurite powstaną).
    var blobService = new BlobServiceClient(storageConnection);
    if (!EF.IsDesignTime)
    {
        blobService.GetBlobContainerClient("avatars").CreateIfNotExists();
        blobService.GetBlobContainerClient("covers").CreateIfNotExists();
    }
    builder.Services.AddSingleton(blobService);
    builder.Services.AddSingleton<IImageStorage, BlobImageStorage>();

    // Miniatury: zadania do kolejki image-jobs, wyniki w kontenerze thumbnails (robi je funkcja GenerateThumbnail).
    // Fabryka zamiast AddSingleton(imageJobs): w DI jest już QueueClient dla transaction-events.
    var imageJobs = new QueueClient(storageConnection, "image-jobs",
        new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
    if (!EF.IsDesignTime)
    {
        imageJobs.CreateIfNotExists();
        blobService.GetBlobContainerClient("thumbnails").CreateIfNotExists();
    }
    builder.Services.AddSingleton<IImageJobQueue>(sp =>
        new QueueImageJobQueue(imageJobs, sp.GetRequiredService<ILogger<QueueImageJobQueue>>()));

    // Historia transakcji: czytamy tabelę, którą wypełnia funkcja ProcessTransactionEvent
    var history = new TableClient(storageConnection, "TransactionHistory");
    if (!EF.IsDesignTime)
        history.CreateIfNotExists();
    builder.Services.AddSingleton(history);
}
else
{
    builder.Services.AddSingleton<IEventPublisher, LoggingEventPublisher>();
    builder.Services.AddSingleton<IImageStorage, UnavailableImageStorage>();
    builder.Services.AddSingleton<IImageJobQueue, NoImageJobQueue>();
}
// Za App Service ruch przechodzi przez front-endy Azure: prawdziwy adres klienta i schemat są w X-Forwarded-*.
// Adresów front-endów nie znamy, więc czyścimy listy zaufanych proxy (domyślny ForwardLimit = 1 bierze ostatni, dopisany przez Azure).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// SERWISY (DI)
builder.Services.AddHttpClient();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserBookService, UserBookService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IGoogleBooksService, GoogleBooksService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IStatsService, StatsService>();

// AUTENTYKACJA JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero // Eliminuje 5-min tolerancję
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Wklej JWT token (bez prefiksu 'Bearer ')"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// AUTO-MIGRACJA — gdy flaga RunMigrations=true (docker-compose oraz App Settings w Azure).
// Lokalny dev (LocalDB) NIE ustawia flagi -> migracje dalej recznie: dotnet ef database update.
if (app.Configuration.GetValue<bool>("RunMigrations"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    const int maxAttempts = 10;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            db.Database.Migrate();           // tworzy baze jesli brak + aplikuje migracje + seed
            logger.LogInformation("Migracje zastosowane (proba {Attempt}/{Max}).", attempt, maxAttempts);
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex, "Baza niegotowa (proba {Attempt}/{Max}) — ponawiam za 5s...", attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

// Globalna obsługa wyjątków: MUSI być pierwszym middleware w pipeline, więć tego proszę NIE RUSZAĆ!!!!! 
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseForwardedHeaders(); // zaraz po obsłudze wyjątków: adres IP klienta (limit prób) i HTTPS za proxy Azure

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // serwowanie avatarów

app.UseCors(FrontendCorsPolicy); // przed uwierzytelnianiem, zeby preflight (OPTIONS) nie dostawal 401

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

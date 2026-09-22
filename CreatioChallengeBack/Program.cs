using System.Reflection;

// Load environment variables from .env if present. Do NOT commit .env.
try
{
    // Try common locations: current directory and parent folders, and AppContext.BaseDirectory.
    var candidates = new List<string>();
    try { candidates.Add(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), ".env")); } catch { }
    try { candidates.Add(System.IO.Path.Combine(AppContext.BaseDirectory, ".env")); } catch { }

    // Add parent directories up to 5 levels from current directory
    var dir = System.IO.Directory.GetCurrentDirectory();
    for (int i = 0; i < 5 && !string.IsNullOrEmpty(dir); i++)
    {
        candidates.Add(System.IO.Path.Combine(dir, ".env"));
        dir = System.IO.Directory.GetParent(dir)?.FullName;
    }

    var found = candidates.FirstOrDefault(System.IO.File.Exists);
    if (!string.IsNullOrEmpty(found))
    {
        DotNetEnv.Env.Load(found);
    }
    else
    {
        // Best-effort: attempt default Load (will silently do nothing if missing)
        DotNetEnv.Env.Load();
    }
}
catch
{
    // Ignore any error loading .env; process can continue if env vars set by other means.
}

var builder = WebApplication.CreateBuilder(args);

// Allow local .env values to configure the Creatio tenant without storing them in appsettings.
var creatioBaseUrl = Environment.GetEnvironmentVariable("BASE_URL");
var creatioTokenUrl = Environment.GetEnvironmentVariable("TOKEN_URL");
var creatioClientId = Environment.GetEnvironmentVariable("CLIENT_ID");
var creatioClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET")
    ?? Environment.GetEnvironmentVariable("CREATIO_CLIENT_SECRET");
if (!string.IsNullOrWhiteSpace(creatioBaseUrl))
{
    builder.Configuration["Creatio:BaseUrl"] = creatioBaseUrl;
}
if (!string.IsNullOrWhiteSpace(creatioTokenUrl))
{
    builder.Configuration["Creatio:TokenEndpoint"] = creatioTokenUrl;
}
if (!string.IsNullOrWhiteSpace(creatioClientId))
{
    builder.Configuration["Creatio:ClientId"] = creatioClientId;
}
if (!string.IsNullOrWhiteSpace(creatioClientSecret))
{
    builder.Configuration["Creatio:ClientSecret"] = creatioClientSecret;
}

// Configuration and services
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks();

// OpenAPI / Swagger - disabled to avoid loading Swashbuckle/OpenAPI assemblies with mismatched versions
// builder.Services.AddEndpointsApiExplorer();


// Configure Creatio integration and related services
builder.Services.Configure<CreatioChallengeBack.Config.CreatioOptions>(builder.Configuration.GetSection("Creatio"));
builder.Services.AddHttpClient();
// Register token service as a singleton so cached access tokens are shared across requests
builder.Services.AddSingleton<CreatioChallengeBack.Interfaces.IOAuthTokenService, CreatioChallengeBack.Services.OAuthTokenService>();
// Ensure concrete ITokenService is registered so the factory resolving ICreatioApiClient can obtain it.
// Defensive registration in case other DI registrations missed this service.
builder.Services.AddSingleton<CreatioChallengeBack.Services.ITokenService, CreatioChallengeBack.Services.TokenService>();
// Register Creatio API client so controllers can consume it via DI. Use Singleton lifetime so DI can
// resolve its dependencies (including the singleton token service) when activating the API client.
// Register CreatioApiClient using a factory to ensure dependencies are resolved correctly
builder.Services.AddSingleton<CreatioChallengeBack.Services.ICreatioApiClient>(sp =>
{
    var httpFactory = sp.GetRequiredService<System.Net.Http.IHttpClientFactory>();
    var tokenSvc = sp.GetRequiredService<CreatioChallengeBack.Services.ITokenService>();
    var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CreatioChallengeBack.Services.CreatioApiClient>>();
    return new CreatioChallengeBack.Services.CreatioApiClient(httpFactory, tokenSvc, logger);
});

// HttpClient para Creatio OData requests
builder.Services.AddHttpClient("creatio-odata", (sp, client) =>
{
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CreatioChallengeBack.Config.CreatioOptions>>().Value;
    if (!string.IsNullOrEmpty(opts.BaseUrl))
    {
        client.BaseAddress = new Uri(opts.BaseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Registrar implementación real del cliente OData (sin mocks)
builder.Services.AddScoped<CreatioChallengeBack.Interfaces.ICreatioClient, CreatioChallengeBack.Clients.CreatioODataClient>();

// CORS - allow Angular origin (configurable)
var allowedOrigins = builder.Configuration.GetValue<string>("Creatio:AllowedOrigins") ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries))
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware pipeline
app.UseHttpsRedirection();
app.UseCors();

// Global error handling for Creatio/http errors
app.UseMiddleware<CreatioChallengeBack.Middleware.ApiExceptionHandlingMiddleware>();


app.UseAuthorization();



app.MapHealthChecks("/health");

        app.MapControllers();



app.Run();

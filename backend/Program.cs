using System.Text.Json;
using backend.Services;
using DefaultNamespace;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// En producción, leer secretos de variables de entorno
if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddEnvironmentVariables("VIBELINK_");

    // Fly.io inyecta DATABASE_URL automáticamente
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        // Convertir postgres:// URL a connection string de Npgsql
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var connectionString =
            $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Disable";
        builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
    }

    // Mapear secrets de Fly.io a la estructura de configuracion del app
    var envJwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
    if (!string.IsNullOrEmpty(envJwtKey))
        builder.Configuration["Jwt:Key"] = envJwtKey;

    var envTmdbKey = Environment.GetEnvironmentVariable("TMDB_API_KEY");
    if (!string.IsNullOrEmpty(envTmdbKey))
        builder.Configuration["ExternalApis:TmdbApiKey"] = envTmdbKey;

    var envTwitchId = Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID");
    if (!string.IsNullOrEmpty(envTwitchId))
        builder.Configuration["ExternalApis:TwitchClientId"] = envTwitchId;

    var envTwitchSecret = Environment.GetEnvironmentVariable("TWITCH_CLIENT_SECRET");
    if (!string.IsNullOrEmpty(envTwitchSecret))
        builder.Configuration["ExternalApis:TwitchClientSecret"] = envTwitchSecret;
}

builder.Services.AddEndpointsApiExplorer();

// Solo Swagger en desarrollo
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen();
}

builder.Services.AddControllers();

// Health checks
builder
    .Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not configured"),
        name: "postgres",
        tags: new[] { "db", "ready" }
    );

// CORS configuración
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy(
            "AllowAll",
            policy =>
            {
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }
        );
    }
    else
    {
        // En producción, restringir orígenes
        var allowedOrigins =
            builder.Configuration["Cors:AllowedOrigins"]?.Split(',')
            ?? new[] { "https://vibelink.app" };

        options.AddPolicy(
            "AllowAll",
            policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        );
    }
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<CompatibilityService>();
builder.Services.AddScoped<AuthService>();

// ContentService para TMDB y RAWG (APIs externas)
builder.Services.AddHttpClient<ContentService>();

// JWT Authentication
var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured");

builder
    .Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.ASCII.GetBytes(jwtKey)
                ),
            };
    });

// Rate limiting para producción
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<
            HttpContext,
            string
        >(httpContext =>
            System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                }
            )
        );
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });
}

var app = builder.Build();

// Aplicar migraciones automáticamente en producción
if (!app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"WARNING: Database migration failed: {ex.Message}");
        Console.WriteLine("The app will start anyway - migrations may need manual attention.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Security headers en producción
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.Use(
        async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            await next();
        }
    );

    app.UseRateLimiter();
}

// No usar HTTPS redirect - Fly.io maneja SSL externamente (SSL termination)
// El contenedor siempre recibe HTTP en el puerto interno
app.UseCors("AllowAll");

// Serve frontend static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoints
app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(
                new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        duration = e.Value.Duration.TotalMilliseconds,
                    }),
                }
            );
            await context.Response.WriteAsync(result);
        },
    }
);

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") }
);

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = _ => false, // Solo verifica que la app responda
    }
);

app.MapControllers();

// SPA fallback - serve index.html for all non-API routes
app.MapFallbackToFile("index.html");

app.Run();

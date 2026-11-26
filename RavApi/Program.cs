using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using RavApi.Data;
using RavApi.Infrastructure;
using RavApi.Logging; // our in-memory log store (with Seq + GetSince)

LocalEnvFileLoader.TryLoad(Directory.GetCurrentDirectory());

var builder = WebApplication.CreateBuilder(args);

// Docker Compose secrets are mounted as files whose names are configuration keys.
// Add this provider last so mounted secrets override JSON/environment placeholders.
if (Directory.Exists("/run/secrets"))
{
    builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);
}

var dataProtectionKeyDirectory = builder.Configuration["DataProtection:KeyDirectory"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeyDirectory))
{
    var dataProtectionBuilder = builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyDirectory));
    var certificatePath = builder.Configuration["DataProtection:CertificatePath"];
    var certificatePassword = builder.Configuration["DataProtection:CertificatePassword"]?.TrimEnd((char)13, (char)10);

    if (builder.Environment.IsProduction() &&
        (string.IsNullOrWhiteSpace(certificatePath) ||
         string.IsNullOrWhiteSpace(certificatePassword) ||
         !File.Exists(certificatePath)))
    {
        throw new InvalidOperationException("Production Data Protection keys require a configured encryption certificate and password.");
    }

    if (!string.IsNullOrWhiteSpace(certificatePath) &&
        !string.IsNullOrWhiteSpace(certificatePassword))
    {
        X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12FromFile(
            certificatePath,
            certificatePassword,
            X509KeyStorageFlags.EphemeralKeySet);
        if (!certificate.HasPrivateKey)
        {
            throw new InvalidOperationException("The Data Protection certificate must include its private key.");
        }

        dataProtectionBuilder.ProtectKeysWithCertificate(certificate);
    }
}

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 256 * 1024;
});

// ---------- DB ----------
var databaseConnectionString = DatabaseConnectionStringFactory.Resolve(builder.Configuration);
var databaseTlsServerName = builder.Configuration["RAV_DATABASE_TLS_SERVER_NAME"];
if (!string.IsNullOrWhiteSpace(builder.Configuration["RAV_DATABASE_TUNNEL_LOCAL_PORT"])
    && !string.Equals(databaseTlsServerName, "postgres", StringComparison.Ordinal))
{
    throw new InvalidOperationException("The Local API SSH tunnel requires the verified PostgreSQL TLS name 'postgres'.");
}

builder.Services.AddDbContext<AppDbContext>(opts =>
{
    opts.UseNpgsql(
        databaseConnectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);

            if (!string.IsNullOrWhiteSpace(databaseTlsServerName))
            {
                npgsqlOptions.ConfigureDataSource(dataSource =>
                    dataSource.UseSslClientAuthenticationOptionsCallback(options => options.TargetHost = databaseTlsServerName));
            }
        });
});

// ---------- Controllers + JSON (camelCase) ----------
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ---------- CORS ----------
builder.Services.AddCors(opts =>
{
    string[] allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    opts.AddPolicy("client", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

// ---------- Request handling ----------
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<RavApi.Infrastructure.ApiExceptionHandler>();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();

    string[] configuredProxies = builder.Configuration
        .GetSection("ForwardedHeaders:KnownProxies")
        .Get<string[]>() ?? Array.Empty<string>();

    foreach (string configuredProxy in configuredProxies)
    {
        if (IPAddress.TryParse(configuredProxy, out IPAddress? address))
        {
            options.KnownProxies.Add(address);
        }
    }
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        GetRateLimitPartition(context),
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------- JWT ----------
string? jwtKey = builder.Configuration["Jwt:Key"];
string? jwtIssuer = builder.Configuration["Jwt:Issuer"];
string? jwtAudience = builder.Configuration["Jwt:Audience"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT signing key is not configured or is too short.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("JWT issuer and audience must be configured.");
}

var signingKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = true;
        o.SaveToken = false;
        o.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                string? sessionIdValue = context.Principal?.FindFirst("sid")?.Value;
                if (!Guid.TryParse(sessionIdValue, out Guid sessionId))
                {
                    context.Fail("Session claim is missing.");
                    return;
                }

                if (!int.TryParse(
                        context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier),
                        out int tokenUserId))
                {
                    context.Fail("User claim is missing.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                bool active = await db.UserSessions.AnyAsync(session =>
                    session.Id == sessionId
                    && session.UserId == tokenUserId
                    && session.RevokedAtUtc == null
                    && session.ExpiresAtUtc > DateTimeOffset.UtcNow,
                    context.HttpContext.RequestAborted);
                if (!active)
                {
                    context.Fail("Session is revoked or expired.");
                }
            }
        };
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    })
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DedicatedServerAuthenticationHandler>(
        DedicatedServerAuthenticationHandler.SchemeName,
        _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin", policy => policy
        .RequireAuthenticatedUser()
        .RequireRole("admin"));
    options.AddPolicy("dedicatedServer", policy => policy
        .AddAuthenticationSchemes(DedicatedServerAuthenticationHandler.SchemeName)
        .RequireAuthenticatedUser()
        .RequireClaim(DedicatedServerAuthenticationHandler.ServerIdClaim));
});
builder.Services.AddScoped<DedicatedServerCredentialValidator>();
builder.Services.AddSingleton<JwtTokenService>();

// ❌ no global logging providers (only explicit events we add ourselves)

var app = builder.Build();

app.UseExceptionHandler();
app.UseForwardedHeaders();
app.UseCors("client");
app.UseRateLimiter();

// ---------- Static files (wwwroot) ----------
app.UseStaticFiles();

// ---------- Swagger UI ----------
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RAV API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/admin"));

app.MapGet("/health/live", () => Results.Ok(new { status = "live" }))
    .AllowAnonymous()
    .WithTags("Health");

app.MapGet("/health/ready", async (AppDbContext db, CancellationToken cancellationToken) =>
{
    try
    {
        if (await db.Database.CanConnectAsync(cancellationToken))
        {
            return Results.Ok(new { status = "ready" });
        }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        throw;
    }
    catch (Exception exception)
    {
        app.Logger.LogWarning(
            "Readiness probe could not connect to PostgreSQL ({ExceptionType}).",
            exception.GetType().Name);
    }

    return Results.Json(new { status = "not_ready" }, statusCode: StatusCodes.Status503ServiceUnavailable);
})
    .AllowAnonymous()
    .WithTags("Health");

// ---------- Auto-migrate + Seed (explicit minimal events) ----------
InMemoryLogStore.Add(new LogEvent { Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Server starting..." });

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    bool applyMigrations = app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
    bool runSeed = app.Configuration.GetValue<bool>("Database:RunSeedOnStartup");
    if (applyMigrations)
    {
        db.Database.Migrate();
        InMemoryLogStore.Add(new LogEvent
        {
            Level = Microsoft.Extensions.Logging.LogLevel.Information,
            Message = "PostgreSQL database migrated."
        });
    }
    else if (app.Environment.IsDevelopment())
    {
        try
        {
            string[] pendingMigrations = db.Database.GetPendingMigrations().ToArray();
            if (pendingMigrations.Length > 0)
            {
                throw new InvalidOperationException(
                    "Local API startup blocked: rav_staging schema is behind this build by "
                    + pendingMigrations.Length.ToString(CultureInfo.InvariantCulture)
                    + " EF Core migration(s). No migrations or seed data were applied.");
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "Local API startup blocked: rav_staging schema compatibility could not be verified. No migrations or seed data were applied.",
                exception);
        }
    }

    if (runSeed)
    {
        SeedData.Initialize(db, app.Configuration.GetValue<bool>("Database:SeedDevelopmentFixtures"));
        InMemoryLogStore.Add(new LogEvent { Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Seed data ensured." });
    }
}

InMemoryLogStore.Add(new LogEvent { Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Server started." });

// ===================== ADMIN ROUTES =====================

// /admin — minimal static HTML
app.MapGet("/admin", async context =>
{
    var webRoot = app.Environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
    var adminIndex = Path.Combine(webRoot, "admin", "index.html");

    if (!System.IO.File.Exists(adminIndex))
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Admin UI not found. Place file at /wwwroot/admin/index.html");
        return;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(adminIndex);
}).RequireAuthorization("admin");

// /admin/logs — logs page (HTML)
app.MapGet("/admin/logs", async context =>
{
    var webRoot = app.Environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
    var logsHtml = Path.Combine(webRoot, "admin", "logs.html");

    if (!System.IO.File.Exists(logsHtml))
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Logs UI not found. Place file at /wwwroot/admin/logs.html");
        return;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(logsHtml);
}).RequireAuthorization("admin");

// common JSON options
var camel = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

// /admin/logs/stream — SSE that sends ONLY new events since lastSeq
app.MapGet("/admin/logs/stream", async context =>
{
    context.Response.Headers["Content-Type"] = "text/event-stream";
    context.Response.Headers["Cache-Control"] = "no-cache";
    context.Response.Headers["X-Accel-Buffering"] = "no";

    var abort = context.RequestAborted;

    // 1) send an initial snapshot once
    long lastSeq = 0;
    var initial = InMemoryLogStore.GetRecent(200);
    if (initial.Count > 0)
    {
        lastSeq = initial[^1].Seq;
        var payload = JsonSerializer.Serialize(initial, camel);
        await context.Response.WriteAsync($"data: {payload}\n\n", abort);
        await context.Response.Body.FlushAsync(abort);
    }

    // 2) then stream only deltas
    try
    {
        while (!abort.IsCancellationRequested)
        {
            await Task.Delay(1000, abort);

            var delta = InMemoryLogStore.GetSince(lastSeq, 200);
            if (delta.Count > 0)
            {
                lastSeq = delta[^1].Seq;
                var payload = JsonSerializer.Serialize(delta, camel);
                await context.Response.WriteAsync($"data: {payload}\n\n", abort);
                await context.Response.Body.FlushAsync(abort);
            }
        }
    }
    catch (OperationCanceledException) { }
}).RequireAuthorization("admin");

// /admin/logs/snapshot — one-time JSON (last N)
app.MapGet("/admin/logs/snapshot", () =>
{
    var recent = InMemoryLogStore.GetRecent(200);
    return Results.Json(recent, camel);
}).RequireAuthorization("admin");

// /admin/logs/clear — clear buffer (and record the action as a new event)
app.MapPost("/admin/logs/clear", () =>
{
    InMemoryLogStore.Clear();
    InMemoryLogStore.Add(new LogEvent { Level = Microsoft.Extensions.Logging.LogLevel.Information, Message = "Log buffer cleared." });
    return Results.Ok(new { ok = true, cleared = true });
}).RequireAuthorization("admin");

app.Run();

static string GetRateLimitPartition(HttpContext context)
{
    return context.Connection.RemoteIpAddress?.ToString() ?? "unknown-client";
}

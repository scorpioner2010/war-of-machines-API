using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WarOfMachines.Data;

var builder = WebApplication.CreateBuilder(args);

// ---------- DB (PostgreSQL) ----------
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? builder.Configuration["DATABASE_URL"];
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string not configured.");

static string EnsureSsl(string cs)
{
    if (cs.IndexOf("sslmode", StringComparison.OrdinalIgnoreCase) >= 0 ||
        cs.IndexOf("Ssl Mode", StringComparison.OrdinalIgnoreCase) >= 0)
        return cs;

    return cs.Contains("://")
        ? (cs.Contains("?") ? cs + "&sslmode=require" : cs + "?sslmode=require")
        : cs + ";Ssl Mode=Require";
}
connectionString = EnsureSsl(connectionString);

builder.Services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(connectionString));

// ---------- Controllers + JSON (camelCase) ----------
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ---------- CORS (Unity / dev) ----------
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("any", p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------- JWT ----------
string? jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT Key not configured.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("any");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WarOfMachines API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ---------- Auto-migrate + Seed ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    SeedData.Initialize(db);
}

app.Run();

using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WarOfMachines.Data;

var builder = WebApplication.CreateBuilder(args);

// --- DB (PostgreSQL) ---
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? builder.Configuration["DATABASE_URL"];
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string not configured.");
}

// ensure sslmode=require (Render)
static string EnsureSsl(string cs)
{
    if (cs.IndexOf("sslmode", StringComparison.OrdinalIgnoreCase) >= 0 ||
        cs.IndexOf("Ssl Mode", StringComparison.OrdinalIgnoreCase) >= 0)
    {
        return cs;
    }

    return cs.Contains("://")
        ? (cs.Contains("?") ? cs + "&sslmode=require" : cs + "?sslmode=require")
        : cs + ";Ssl Mode=Require";
}
connectionString = EnsureSsl(connectionString);

builder.Services.AddDbContext<AppDbContext>(opts => { opts.UseNpgsql(connectionString); });

// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- JWT ---
string? jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT Key not configured.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WarOfMachines API v1");
    c.RoutePrefix = "swagger"; // Swagger за адресою /swagger
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- Auto-migrate + Seed ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Ініціалізуємо стартові дані (vehicles, тестовий адмін)
    SeedData.Initialize(db);
}

app.Run();

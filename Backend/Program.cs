using Microsoft.EntityFrameworkCore;
using Backend.Api.Data;
using System.Text.Json;
using Backend.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Récupère les origines autorisées depuis config ou fallback sur localhost
var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(';') 
                     ?? new[] {
                         "http://localhost:5173",
                          "https://cat-match-five.vercel.app"
                        };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", p => p
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Choix DB : SQL Server si DefaultConnection est défini, sinon SQLite
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(defaultConn))
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(defaultConn, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(); // tolérance aux timeouts Azure
        }));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlite("Data Source=catsImages.db"));
}

var app = builder.Build();

// Migration + seed au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.CatsImages.Any())
    {
        var client = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
        var response = await client.GetAsync("https://conseil.latelier.co/data/cats.json");
        if (response.IsSuccessStatusCode)
        {
            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object 
                && root.TryGetProperty("images", out JsonElement imagesList) 
                && imagesList.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in imagesList.EnumerateArray())
                {
                    string? url = null;
                    string? externalId = null;

                    if (item.TryGetProperty("url", out var catUrl) && catUrl.ValueKind == JsonValueKind.String)
                    {
                        url = catUrl.GetString();
                    }
                    if (item.TryGetProperty("id", out var catId) && catId.ValueKind == JsonValueKind.String)
                    {
                        externalId = catId.GetString();
                    }

                    if (string.IsNullOrEmpty(url)) continue;
                    if (await db.CatsImages.AnyAsync(c => c.Url == url)) continue; // éviter doublons

                    db.CatsImages.Add(new CatsEntity
                    {
                        Url = url,
                        ExternalId = externalId ?? "",
                        Score = 0,
                        Id = Guid.NewGuid()
                    });
                }
                await db.SaveChangesAsync();
            }
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultCors");
app.MapControllers();
app.Run();

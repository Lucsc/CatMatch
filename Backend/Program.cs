using Microsoft.EntityFrameworkCore;
using Backend.Api.Data;
using System.Text.Json;
using Backend.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDev", p => p
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=catsImages.db"));

var app = builder.Build();

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

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("images", out JsonElement imagesList) && imagesList.ValueKind == JsonValueKind.Array)
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
                    if (await db.CatsImages.AnyAsync(c => c.Url == url)) continue; // Unique URL check

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

app.UseCors("LocalDev");
app.MapControllers();
app.Run();

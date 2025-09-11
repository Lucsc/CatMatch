

using System.Text.Json;
using Backend.Api.Data;
using Backend.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;

        public CatsController(AppDbContext dbContext, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            FetchAndStoreCatImage().Wait();
        }

        [HttpPost]
        public async Task<IActionResult> FetchAndStoreCatImage()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://conseil.latelier.co/data/cats.json");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error while fetching Cats Images.");
            }

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array && root.TryGetProperty("images", out JsonElement imagesList) && imagesList.ValueKind == JsonValueKind.Array)
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
                    if (await _dbContext.CatsImages.AnyAsync(c => c.Url == url)) continue; // Unique URL check

                    _dbContext.CatsImages.Add(new CatsEntity
                    {
                        Url = url,
                        ExternalId = externalId ?? "",
                        Score = 0,
                        Id = Guid.NewGuid()
                    });
                }
                await _dbContext.SaveChangesAsync();
                return Ok("Cat images fetched and stored successfully.");
            }

            return BadRequest("Invalid JSON Structure.");
        }
    }
}
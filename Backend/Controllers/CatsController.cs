

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
        }

        [HttpPost("StoreCatImage")]
        public async Task<IActionResult> StoreCatImage([FromBody] object body)
        {
            if (body == null)
            {
                return BadRequest("Request body is required.");
            }

            using var doc = JsonDocument.Parse(body.ToString() ?? "");
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

        [HttpGet]
        public async Task<IActionResult> GetAllCats()
        {
            var catsList = await _dbContext.CatsImages.OrderByDescending(c => c.Score).ToListAsync();
            return Ok(catsList);
        }

        [HttpPost("vote/{winnerId}")]
        public async Task<IActionResult> VoteCat(string winnerId)
        {
            if (string.IsNullOrEmpty(winnerId) || !Guid.TryParse(winnerId, out Guid catGuid))
            {
                return BadRequest("Invalid or missing 'winnerId'.");
            }

            var cat = await _dbContext.CatsImages.FirstOrDefaultAsync(c => c.Id == catGuid);
            if (cat == null)
            {
                return NotFound("Cat not found.");
            }

            cat.Score += 1;
            await _dbContext.SaveChangesAsync();

            return Ok("Vote recorded successfully.");
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetTwoRandomsCats()
        {
            var pairCats = await _dbContext.CatsImages.OrderBy(c => EF.Functions.Random()).Take(2).ToListAsync();
            if (pairCats.Count < 2)
            {
                return BadRequest("Not enough cat images available.");
            }
            return Ok(pairCats);
        }

        [HttpGet("matchesCount")]
        public async Task<IActionResult> GetMatchesCount()
        {
            var count = await _dbContext.CatsImages.Where(x => x.Score > 0).Select(x => x.Score).SumAsync();
            return Ok(new { Count = count });
        }
    }
}
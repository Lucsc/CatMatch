

using System.Text.Json;
using Backend.Api.Data;
using Backend.Api.Models;
using Backend.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IHubContext<MonitoringHub> _hubContext;

        public CatsController(AppDbContext dbContext, IHttpClientFactory httpClientFactory, IHubContext<MonitoringHub> hubContext)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
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
        public async Task<IActionResult> GetAllCats([FromQuery] string? period)
        {
            if (period == null || period.Equals("all", StringComparison.CurrentCultureIgnoreCase))
            {
                var catsList = await _dbContext.CatsImages.OrderByDescending(c => c.Score).ToListAsync();
                return Ok(catsList);
            }

            DateTime fromDate = period.ToLower() switch
            {
                "last hour" => DateTime.UtcNow.AddHours(-1),
                "last day" => DateTime.UtcNow.AddDays(-1),
                "last week" => DateTime.UtcNow.AddDays(-7),
                "last month" => DateTime.UtcNow.AddMonths(-1),
                "last year" => DateTime.UtcNow.AddYears(-1),
                _ => DateTime.MinValue
            };

            var votesQuery = _dbContext.Votes.AsQueryable();
            if (fromDate != DateTime.MinValue)
            {
                votesQuery = votesQuery.Where(v => v.CreatedAt >= fromDate);
            }

            var catVotes = await votesQuery
                .GroupBy(v => v.WinnerId)
                .Select(g => new { CatId = g.Key, Votes = g.Count() })
                .OrderByDescending(g => g.Votes)
                .ToListAsync();

            var catIds = catVotes.Select(cv => cv.CatId).ToList();
            var cats = await _dbContext.CatsImages.Where(c => catIds.Contains(c.Id.ToString())).ToListAsync();

            var result = catVotes
                .Join(cats, cv => cv.CatId, c => c.Id.ToString(), (cv, c) => new
                {
                    c.Id,
                    c.Url,
                    c.ExternalId,
                    c.Score,
                    cv.Votes
                })
                .OrderByDescending(x => x.Votes)
                .ToList();

            return Ok(result);
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
            _dbContext.Votes.Add(new VoteEntity
            {
                Id = Guid.NewGuid().ToString(),
                WinnerId = cat.Id.ToString(),
                WinnerImageUrl = cat.Url,
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("MonitoringUpdate", new
            {
                Type = "Vote",
                cat.Id,
                cat.Url,
                NewScore = cat.Score,
                Timestamp = DateTime.UtcNow
            });

            return Ok("Vote recorded successfully.");
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetTwoRandomsCats()
        {
            List<CatsEntity> pairCats;
            if (_dbContext.Database.ProviderName?.Contains("Sqlite") == true)
            {
                pairCats = await _dbContext.CatsImages.OrderBy(c => EF.Functions.Random()).Take(2).ToListAsync();
            }
            else
            {
                pairCats = await _dbContext.CatsImages.OrderBy(c => Guid.NewGuid()).Take(2).ToListAsync();
            }

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
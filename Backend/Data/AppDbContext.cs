using Microsoft.EntityFrameworkCore;
using Backend.Api.Models;
using Backend.Models.Api;

namespace Backend.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } // Constructor

        public DbSet<CatsEntity> CatsImages { get; set; }

        public DbSet<VoteEntity> Votes { get; set; }
    }
}
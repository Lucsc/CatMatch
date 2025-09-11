using Microsoft.EntityFrameworkCore;
using Backend.Api.Models;

namespace Backend.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } // Constructor

        public DbSet<CatsEntity> CatsImages { get; set; } // DbSet for CatsEntity
    }
}
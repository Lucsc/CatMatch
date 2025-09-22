using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Api
{
    public class VoteEntity
    {
        [Key]
        public required string Id { get; set; } = Guid.NewGuid().ToString();
        public required string WinnerId { get; set; } = Guid.NewGuid().ToString();
        public required string WinnerImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
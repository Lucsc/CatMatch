namespace Backend.Models.Api
{
    public class VoteEntity
    {
        public required Guid Id { get; set; }
        public required Guid WinnerId { get; set; }
        public required string WinnerImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
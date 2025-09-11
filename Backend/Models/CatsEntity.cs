namespace Backend.Api.Models
{
    public class CatsEntity
    {
        public required Guid Id { get; set; }

        public string Url { get; set; } = "";

        public string ExternalId { get; set; } = "";

        public int Score { get; set; } = 0;
    }
}
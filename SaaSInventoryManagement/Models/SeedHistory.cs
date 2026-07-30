namespace SaaSInventoryManagement.Models
{
    public class SeedHistory
    {
        public int Id { get; set; }

        public string SeedKey { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public string Notes { get; set; }
    }
}

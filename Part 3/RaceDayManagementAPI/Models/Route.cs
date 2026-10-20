namespace RaceDayManagementAPI.Models
{
    public class Route
    {
        public int RouteID { get; set; }
        public int CategoryID { get; set; }
        public string? MapUrl { get; set; }
        public int? ElevationGain { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public Category? Category { get; set; }
    }
}

namespace RaceDayManagementAPI.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = "run"; // run, walk, cycle
        public string Status { get; set; } = "draft";  // draft, published, cancelled, completed
        public string? BannerUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Organiser? Organiser { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}

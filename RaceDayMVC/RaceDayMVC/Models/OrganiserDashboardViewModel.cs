namespace RaceDayMVC.Models
{
    public class OrganiserDashboardViewModel
    {
        public EventViewModel Event { get; set; } = new();
        public int TotalEnrolments { get; set; }
    }

    public class CreateEventViewModel
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public string Location { get; set; } = "";
        public string EventType { get; set; } = "run";
        public string? Status { get; set; }
        public IFormFile? BannerImage { get; set; }
    }
}
using System.Text.Json.Serialization;

namespace RaceDayManagementAPI.Models
{
    public class Organiser
    {
        public int OrganiserID { get; set; }
        public int UserID { get; set; }
        public string? CompanyName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public List<Event> Events { get; set; } = new List<Event>();
        [JsonIgnore]
        public List<Result> Results { get; set; } = new List<Result>();
    }
}

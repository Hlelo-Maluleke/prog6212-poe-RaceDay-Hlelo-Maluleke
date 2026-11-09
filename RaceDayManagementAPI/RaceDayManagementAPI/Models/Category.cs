using System.Text.Json.Serialization;

namespace RaceDayManagementAPI.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DistanceKm { get; set; }
        public decimal EntryFee { get; set; } = 0;
        public int? MaxParticipants { get; set; }
        public TimeSpan? StartTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [JsonIgnore]
        public Event? Event { get; set; }
        public Route? Route { get; set; }
        [JsonIgnore]
        public List<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}

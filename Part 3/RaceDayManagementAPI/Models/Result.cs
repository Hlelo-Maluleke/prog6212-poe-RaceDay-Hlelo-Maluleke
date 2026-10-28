using System.Text.Json.Serialization;

namespace RaceDayManagementAPI.Models
{
    public class Result
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public int OrganiserID { get; set; }
        public int? Position { get; set; }
        public int? FinishTimeSeconds { get; set; }
        public string? Status { get; set; } // finished, dnf(did not finish), dsq(did not start)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [JsonIgnore]
        public Enrolment? Enrolment { get; set; }
        [JsonIgnore]
        public Organiser? Organiser { get; set; }
    }
}

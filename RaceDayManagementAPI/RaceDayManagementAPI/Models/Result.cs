namespace RaceDayManagementAPI.Models
{
    public class Result
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public int OrganiserID { get; set; }
        public int? Position { get; set; }
        public int? FinishTimeSeconds { get; set; }
        public string? Status { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Enrolment? Enrolment { get; set; }
        public Organiser? Organiser { get; set; }
    }
}

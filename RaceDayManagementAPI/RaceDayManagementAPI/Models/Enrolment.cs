namespace RaceDayManagementAPI.Models
{
    public class Enrolment
    {
        public int EnrolmentID { get; set; }
        public int ParticipantID { get; set; }
        public int CategoryID { get; set; }
        public DateTime EnrolmentDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "pending"; // pending, confirmed, withdrawn
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Participant? Participant { get; set; }
        public Category? Category { get; set; }
        public Result? Result { get; set; }
    }
}

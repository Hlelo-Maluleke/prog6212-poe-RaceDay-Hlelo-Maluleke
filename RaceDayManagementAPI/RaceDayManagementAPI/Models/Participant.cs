namespace RaceDayManagementAPI.Models
{
    public class Participant
    {
        public int ParticipantID { get; set; }
        public int UserID { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? IDNumber { get; set; }
        public string? EmergencyContact { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public User? User { get; set; }
        public List<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}

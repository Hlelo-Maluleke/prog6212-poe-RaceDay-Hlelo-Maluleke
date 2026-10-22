namespace RaceDayMVC.Models
{
    public class EventViewModel
    {
        public int EventID { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public string Location { get; set; } = "";
        public string EventType { get; set; } = "";
        public string Status { get; set; } = "";
        public string? Organiser { get; set; }
        public string? BannerUrl { get; set; } 
    }

    public class EventDetailViewModel : EventViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new();
    }

    public class CategoryViewModel
    {
        public int CategoryID { get; set; }
        public string Name { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public decimal EntryFee { get; set; }
        public int? MaxParticipants { get; set; }
        public RouteViewModel? Route { get; set; }
    }

    public class RouteViewModel
    {
        public string? MapUrl { get; set; }
        public int? ElevationGain { get; set; }
        public string? Description { get; set; }
    }

    public class EnrolmentViewModel
    {
        public int EnrolmentID { get; set; }
        public string Status { get; set; } = "";
        public DateTime EnrolmentDate { get; set; }
        public EventViewModel? Event { get; set; }
        public CategoryViewModel? Category { get; set; }
        public ResultSummaryViewModel? Result { get; set; }
        public ParticipantProfileViewModel? Participant { get; set; }
    }

    public class ResultViewModel
    {
        public int ResultID { get; set; }
        public int? Position { get; set; }
        public int? FinishTimeSeconds { get; set; }
        public string? Status { get; set; }
        public EventViewModel? Event { get; set; }
        public CategoryViewModel? Category { get; set; }
    }

    public class ResultSummaryViewModel
    {
        public int? FinishTimeSeconds { get; set; }
        public int? Position { get; set; }
    }

    public class ParticipantProfileViewModel
    {
        public int ParticipantID { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? IDNumber { get; set; }
        public string? EmergencyContact { get; set; }
        public int UserID { get; set; }
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
    }
}

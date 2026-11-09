using System.Security.Cryptography;
using System.Text;

namespace RaceDayManagementAPI.Models
{
    public class RaceDayDB
    {
        public static List<User> Users { get; set; } = new List<User>();
        public static List<Organiser> Organisers { get; set; } = new List<Organiser>();
        public static List<Participant> Participants { get; set; } = new List<Participant>();
        public static List<Event> Events { get; set; } = new List<Event>();
        public static List<Category> Categories { get; set; } = new List<Category>();
        public static List<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
        public static List<Result> Results { get; set; } = new List<Result>();
        public static List<Route> Routes { get; set; } = new List<Route>();

        public static int NextUserID { get; set; } = 1;
        public static int NextOrganiserID { get; set; } = 1;
        public static int NextParticipantID { get; set; } = 1;
        public static int NextEventID { get; set; } = 1;
        public static int NextCategoryID { get; set; } = 1;
        public static int NextEnrolmentID { get; set; } = 1;
        public static int NextResultID { get; set; } = 1;
        public static int NextRouteID { get; set; } = 1;

        // ---------- Helper Method: Hash Password ----------
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}

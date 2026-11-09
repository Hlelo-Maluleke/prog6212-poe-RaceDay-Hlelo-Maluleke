namespace RaceDayMVC.Models
{
    public class LoginViewModel
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RegisterViewModel
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }
        public string? IDNumber { get; set; }
        public string? EmergencyContact { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; } = "";
        public int UserID { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsOrganiser { get; set; }
        public bool IsParticipant { get; set; }
    }
}

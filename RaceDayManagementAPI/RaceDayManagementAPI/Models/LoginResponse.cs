namespace RaceDayManagementAPI.Models
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsOrganiser { get; set; }
        public bool IsParticipant { get; set; }
    }
}

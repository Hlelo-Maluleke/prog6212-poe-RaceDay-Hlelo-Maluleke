using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayManagementAPI.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
     //   [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
     //   [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

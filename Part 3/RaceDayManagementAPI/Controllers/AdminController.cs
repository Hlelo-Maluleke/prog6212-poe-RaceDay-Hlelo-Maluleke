using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayManagementAPI.Data;
using RaceDayManagementAPI.Models;
using RaceDayManagementAPI.Services;
using System.Security.Cryptography;
using System.Text;

namespace RaceDayManagementAPI.Controllers
{
    /// <summary>
    /// Administration controller for system-wide user and organiser management.
    /// Provides endpoints for viewing all users, creating organisers, and managing permissions.
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly RaceDayDbContext _db;

        public AdminController(JwtService jwtService, RaceDayDbContext db)
        {
            _jwtService = jwtService;
            _db = db;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetAllUsers()
        {
            var users = _db.Users.Select(u => new
            {
                u.UserID,
                u.Email,
                u.FullName,
                u.Phone,
                IsOrganiser = _db.Organisers.Any(o => o.UserID == u.UserID),
                IsParticipant = _db.Participants.Any(p => p.UserID == u.UserID),
                u.CreatedAt
            }).ToList();

            return Ok(users);
        }

        [HttpGet("users/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetUserById(int id)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new
            {
                user.UserID,
                user.Email,
                user.FullName,
                user.Phone,
                IsOrganiser = _db.Organisers.Any(o => o.UserID == user.UserID),
                IsParticipant = _db.Participants.Any(p => p.UserID == user.UserID),
                user.CreatedAt,
                user.UpdatedAt
            });
        }

        [HttpPost("organisers")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult CreateOrganiser([FromBody] CreateOrganiserRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email and password are required." });

            var existingUser = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
                return Conflict(new { message = "Email already registered." });

            var passwordHash = HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Phone = request.Phone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            _db.SaveChanges();

            var organiser = new Organiser
            {
                UserID = user.UserID,
                CompanyName = request.CompanyName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Organisers.Add(organiser);
            _db.SaveChanges();

            return CreatedAtAction(nameof(GetUserById), new { id = user.UserID }, new
            {
                user.UserID,
                user.Email,
                user.FullName,
                user.Phone,
                organiser.CompanyName,
                user.CreatedAt
            });
        }

        // 4. DELETE USER (Admin)
        [HttpDelete("users/{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // Remove subtype records using EF Core DbContext instead of static RaceDayDB
            var organisers = _db.Organisers.Where(o => o.UserID == id).ToList();
            _db.Organisers.RemoveRange(organisers);

            var participants = _db.Participants.Where(p => p.UserID == id).ToList();
            _db.Participants.RemoveRange(participants);

            _db.Users.Remove(user);
            _db.SaveChanges();

            return NoContent();
        }
    }

    public class CreateOrganiserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
    }
}
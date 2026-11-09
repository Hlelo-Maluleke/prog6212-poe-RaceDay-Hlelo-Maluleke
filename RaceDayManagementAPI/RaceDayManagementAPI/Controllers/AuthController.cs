using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDayManagementAPI.Data;
using RaceDayManagementAPI.Models;
using RaceDayManagementAPI.Services;
using System.Security.Cryptography;
using System.Text;

namespace RaceDayManagementAPI.Controllers
{
    /// <summary>
    /// Authentication controller for user registration and login.
    /// Handles JWT token generation for authorized API access.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly RaceDayDbContext _db;

        public AuthController(JwtService jwtService, RaceDayDbContext db)
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

        /// <summary>
        /// Registers a new participant account.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            var existingUser = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return Conflict(new { message = "Email already registered." });
            }

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

            var participant = new Participant
            {
                UserID = user.UserID,
                DateOfBirth = request.DateOfBirth,
                IDNumber = request.IDNumber,
                EmergencyContact = request.EmergencyContact,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Participants.Add(participant);
            _db.SaveChanges();

            return CreatedAtAction(nameof(Register), new { id = user.UserID }, new
            {
                user.UserID,
                user.Email,
                user.FullName,
                user.Phone,
                user.CreatedAt
            });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token for API access.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var passwordHash = HashPassword(request.Password);
            if (user.PasswordHash != passwordHash)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var isOrganiser = _db.Organisers.Any(o => o.UserID == user.UserID);
            var isParticipant = _db.Participants.Any(p => p.UserID == user.UserID);

            var token = _jwtService.GenerateToken(
                user.UserID,
                user.Email,
                user.FullName,
                isOrganiser,
                isParticipant
            );

            return Ok(new LoginResponse
            {
                Token = token,
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                IsOrganiser = isOrganiser,
                IsParticipant = isParticipant
            });
        }
    }
}

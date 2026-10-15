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
        /// <remarks>
        /// Creates a new user and automatically registers them as a participant.
        /// Organisers are pre-seeded in the system and can only be created by administrators.
        /// </remarks>
        /// <param name="request">Registration request containing email, password, and participant details</param>
        /// <returns>
        /// Returns 201 Created with the new user details if registration is successful.
        /// Returns 400 Bad Request if required fields are missing.
        /// Returns 409 Conflict if email is already registered.
        /// </returns>
        /// <response code="201">User and participant successfully created</response>
        /// <response code="400">Email and password are required; missing required fields</response>
        /// <response code="409">Email already registered</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            // Validate
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            // Check if email already exists
            var existingUser = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return Conflict(new { message = "Email already registered." });
            }

            // Hash password
            var passwordHash = HashPassword(request.Password);

            // Create User
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

            // Create Participant (automatically - organisers are pre-seeded)
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

            // Return user object (without password)
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
        /// <remarks>
        /// Verifies the provided email and password, then generates a JWT token valid for subsequent API calls.
        /// The token includes claims for user roles (organiser/participant).
        /// </remarks>
        /// <param name="request">Login credentials (email and password)</param>
        /// <returns>
        /// Returns 200 OK with JWT token and user details if authentication is successful.
        /// Returns 401 Unauthorized if email or password is invalid.
        /// </returns>
        /// <response code="200">Authentication successful; JWT token returned</response>
        /// <response code="401">Invalid email or password</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Find user by email
            var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Verify password
            var passwordHash = HashPassword(request.Password);
            if (user.PasswordHash != passwordHash)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Check if user is an organiser
            var isOrganiser = _db.Organisers.Any(o => o.UserID == user.UserID);
            var isParticipant = _db.Participants.Any(p => p.UserID == user.UserID);

            // Generate JWT token
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


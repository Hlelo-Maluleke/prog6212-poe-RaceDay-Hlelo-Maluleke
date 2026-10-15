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

        /// <summary>
        /// Retrieves a list of all users in the system with their roles.
        /// </summary>
        /// <remarks>
        /// Returns all registered users along with their roles (organiser/participant status).
        /// Requires authentication.
        /// </remarks>
        /// <returns>List of all users with UserID, Email, FullName, Phone, IsOrganiser, IsParticipant, and CreatedAt</returns>
        /// <response code="200">Successfully retrieved all users</response>
        /// <response code="401">User is not authenticated</response>
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

        /// <summary>
        /// Retrieves a specific user by ID with detailed information and roles.
        /// </summary>
        /// <remarks>
        /// Returns comprehensive user information including organiser/participant status.
        /// Requires authentication.
        /// </remarks>
        /// <param name="id">The unique identifier of the user</param>
        /// <returns>User details including UserID, Email, FullName, Phone, IsOrganiser, IsParticipant, timestamps</returns>
        /// <response code="200">User found and returned</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="404">User with specified ID not found</response>
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

        /// <summary>
        /// Creates a new organiser account with associated user record.
        /// </summary>
        /// <remarks>
        /// Registers a new event organiser in the system. This endpoint creates both a User and Organiser record.
        /// Note: This endpoint allows anonymous access for initial system setup; restrict in production.
        /// </remarks>
        /// <param name="request">Organiser creation request with email, password, company name, and contact details</param>
        /// <returns>Returns 201 Created with the new organiser details if successful</returns>
        /// <response code="201">Organiser and user successfully created</response>
        /// <response code="400">Email and password are required; missing required fields</response>
        /// <response code="409">Email already registered in system</response>
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
            var user = RaceDayDB.Users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // Remove subtype records
            RaceDayDB.Organisers.RemoveAll(o => o.UserID == id);
            RaceDayDB.Participants.RemoveAll(p => p.UserID == id);
            RaceDayDB.Users.Remove(user);

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
using Microsoft.EntityFrameworkCore;
using RaceDayManagementAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace RaceDayManagementAPI.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<RaceDayManagementAPI.Models.Route> Routes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Hash the default password for seeding
            var defaultPasswordHash = HashPassword("password123");

            // SEED USERS (5 total: 2 organisers + 3 participants)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserID = 1,
                    Email = "thabo@runningclub.co.za",
                    PasswordHash = defaultPasswordHash,
                    FullName = "Thabo Mokoena",
                    Phone = "082 123 4567",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    UserID = 2,
                    Email = "susan@capetowncycles.co.za",
                    PasswordHash = defaultPasswordHash,
                    FullName = "Susan van der Merwe",
                    Phone = "083 987 6543",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    UserID = 3,
                    Email = "lindiwe@runsa.co.za",
                    PasswordHash = defaultPasswordHash,
                    FullName = "Lindiwe Nkosi",
                    Phone = "076 555 1234",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    UserID = 4,
                    Email = "michael@cyclezone.co.za",
                    PasswordHash = defaultPasswordHash,
                    FullName = "Michael Jacobs",
                    Phone = "072 444 5678",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    UserID = 5,
                    Email = "sipho@fitness.co.za",
                    PasswordHash = defaultPasswordHash,
                    FullName = "Sipho Dlamini",
                    Phone = "073 333 9012",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                }
            );

            // SEED ORGANISERS (2 total – pre-seeded)
            modelBuilder.Entity<Organiser>().HasData(
                new Organiser
                {
                    OrganiserID = 1,
                    UserID = 1,
                    CompanyName = "Soweto Running Club",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                },
                new Organiser
                {
                    OrganiserID = 2,
                    UserID = 2,
                    CompanyName = "Cape Town Cycle Tours",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                }
            );

            // SEED PARTICIPANTS (3 total – self-registered users)
            modelBuilder.Entity<Participant>().HasData(
                new Participant
                {
                    ParticipantID = 1,
                    UserID = 3,
                    DateOfBirth = new DateTime(1985, 6, 15),
                    IDNumber = "8506151234089",
                    EmergencyContact = "Sipho Mokoena - 082 111 2222",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                },
                new Participant
                {
                    ParticipantID = 2,
                    UserID = 4,
                    DateOfBirth = new DateTime(1992, 11, 22),
                    IDNumber = "9211225678012",
                    EmergencyContact = "Sarah Jacobs - 083 444 5555",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                },
                new Participant
                {
                    ParticipantID = 3,
                    UserID = 5,
                    DateOfBirth = new DateTime(1998, 3, 1),
                    IDNumber = "9803019012345",
                    EmergencyContact = "Nomsa Dlamini - 076 666 7777",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                }
            );
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
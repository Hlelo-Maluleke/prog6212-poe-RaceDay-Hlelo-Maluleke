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

            // Common seed date
            var seedDate = new DateTime(2026, 1, 1);

            // SEED EVENTS (3 total)
            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    EventID = 1,
                    OrganiserID = 1,
                    Name = "Soweto Marathon",
                    Description = "One of South Africa's most iconic road races through the heart of Soweto.",
                    EventDate = new DateTime(2026, 11, 15),
                    StartTime = new TimeSpan(6, 0, 0),
                    Location = "Soweto, Johannesburg",
                    EventType = "run",
                    Status = "published",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Event
                {
                    EventID = 2,
                    OrganiserID = 2,
                    Name = "Cape Town Cycle Tour",
                    Description = "The world's largest timed cycle race along Chapman's Peak.",
                    EventDate = new DateTime(2026, 3, 8),
                    StartTime = new TimeSpan(7, 30, 0),
                    Location = "Cape Town CBD",
                    EventType = "cycle",
                    Status = "published",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Event
                {
                    EventID = 3,
                    OrganiserID = 1,
                    Name = "Durban Community Parkrun",
                    Description = "Weekly free community walking/running event at Durban Botanic Gardens.",
                    EventDate = new DateTime(2026, 10, 25),
                    StartTime = new TimeSpan(8, 0, 0),
                    Location = "Durban Botanic Gardens",
                    EventType = "walk",
                    Status = "published",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                }
            );

            // SEED CATEGORIES (7 total)
            modelBuilder.Entity<Category>().HasData(
                // Soweto Marathon (Event 1)
                new Category
                {
                    CategoryID = 1,
                    EventID = 1,
                    Name = "10km Road Run",
                    Description = "Short distance for beginners",
                    DistanceKm = 10.00m,
                    EntryFee = 150.00m,
                    MaxParticipants = 500,
                    StartTime = new TimeSpan(6, 15, 0),
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Category
                {
                    CategoryID = 2,
                    EventID = 1,
                    Name = "21km Half Marathon",
                    Description = "Intermediate distance",
                    DistanceKm = 21.10m,
                    EntryFee = 250.00m,
                    MaxParticipants = 300,
                    StartTime = new TimeSpan(6, 0, 0),
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Category
                {
                    CategoryID = 3,
                    EventID = 1,
                    Name = "42km Full Marathon",
                    Description = "The ultimate challenge",
                    DistanceKm = 42.20m,
                    EntryFee = 350.00m,
                    MaxParticipants = 200,
                    StartTime = new TimeSpan(6, 0, 0),
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                // Cape Town Cycle Tour (Event 2)
                new Category
                {
                    CategoryID = 4,
                    EventID = 2,
                    Name = "40km Fun Ride",
                    Description = "Casual cycle for families",
                    DistanceKm = 40.00m,
                    EntryFee = 100.00m,
                    MaxParticipants = 1000,
                    StartTime = new TimeSpan(8, 0, 0),
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Category
                {
                    CategoryID = 5,
                    EventID = 2,
                    Name = "109km Classic",
                    Description = "Full Cape Town Cycle Tour",
                    DistanceKm = 109.00m,
                    EntryFee = 300.00m,
                    MaxParticipants = 1500,
                    StartTime = new TimeSpan(7, 30, 0),
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                // Durban Community Parkrun (Event 3 - FREE!)
                new Category
                {
                    CategoryID = 6,
                    EventID = 3,
                    Name = "5km Community Walk",
                    Description = "Free family-friendly walk",
                    DistanceKm = 5.00m,
                    EntryFee = 0.00m,
                    MaxParticipants = 200,
                    StartTime = new TimeSpan(8, 0, 0),
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Category
                {
                    CategoryID = 7,
                    EventID = 3,
                    Name = "5km Community Run",
                    Description = "Free run for fitness enthusiasts",
                    DistanceKm = 5.00m,
                    EntryFee = 0.00m,
                    MaxParticipants = 150,
                    StartTime = new TimeSpan(8, 0, 0),
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                }
            );

            // SEED ROUTES (3 total)
            modelBuilder.Entity<RaceDayManagementAPI.Models.Route>().HasData(
                new RaceDayManagementAPI.Models.Route
                {
                    RouteID = 1,
                    CategoryID = 2,
                    MapUrl = "https://maps.example.com/soweto-21km-route",
                    ElevationGain = 180,
                    Description = "Scenic route through Soweto landmarks including Mandela House",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new RaceDayManagementAPI.Models.Route
                {
                    RouteID = 2,
                    CategoryID = 5,
                    MapUrl = "https://maps.example.com/ctct-109km-route",
                    ElevationGain = 850,
                    Description = "Challenging route with Chapman's Peak Drive and Constantia climbs",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new RaceDayManagementAPI.Models.Route
                {
                    RouteID = 3,
                    CategoryID = 6,
                    MapUrl = "https://maps.example.com/durban-5km-route",
                    ElevationGain = 25,
                    Description = "Flat, family-friendly route through Durban Botanic Gardens",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                }
            );

            // Fixed dates for enrolment records
            var enrolmentDate1 = new DateTime(2026, 2, 1);
            var enrolmentDate2 = new DateTime(2026, 1, 15);
            var enrolmentDate3 = new DateTime(2026, 3, 1);

            // SEED ENROLMENTS (8 total)
            modelBuilder.Entity<Enrolment>().HasData(
                // Lindiwe (Participant 1)
                new Enrolment
                {
                    EnrolmentID = 1,
                    ParticipantID = 1,
                    CategoryID = 2,
                    Status = "confirmed",
                    EnrolmentDate = enrolmentDate1,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Enrolment
                {
                    EnrolmentID = 2,
                    ParticipantID = 1,
                    CategoryID = 5,
                    Status = "confirmed",
                    EnrolmentDate = enrolmentDate2,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Enrolment
                {
                    EnrolmentID = 3,
                    ParticipantID = 1,
                    CategoryID = 6,
                    Status = "confirmed",
                    EnrolmentDate = enrolmentDate3,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                // Michael (Participant 2)
                new Enrolment
                {
                    EnrolmentID = 4,
                    ParticipantID = 2,
                    CategoryID = 1,
                    Status = "confirmed",
                    EnrolmentDate = enrolmentDate1,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Enrolment
                {
                    EnrolmentID = 5,
                    ParticipantID = 2,
                    CategoryID = 5,
                    Status = "confirmed",
                    EnrolmentDate = enrolmentDate2,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Enrolment
                {
                    EnrolmentID = 6,
                    ParticipantID = 2,
                    CategoryID = 7,
                    Status = "pending",
                    EnrolmentDate = enrolmentDate3,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                // Sipho (Participant 3)
                new Enrolment
                {
                    EnrolmentID = 7,
                    ParticipantID = 3,
                    CategoryID = 3,
                    Status = "confirmed",
                    EnrolmentDate = enrolmentDate1,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Enrolment
                {
                    EnrolmentID = 8,
                    ParticipantID = 3,
                    CategoryID = 4,
                    Status = "confirmed",
                    EnrolmentDate = enrolmentDate2,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                }
            );

            // SEED RESULTS (4 total)
            modelBuilder.Entity<Result>().HasData(
                new Result
                {
                    ResultID = 1,
                    EnrolmentID = 1,
                    OrganiserID = 1,
                    FinishTimeSeconds = 6300,
                    Position = 42,
                    Status = "finished",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Result
                {
                    ResultID = 2,
                    EnrolmentID = 2,
                    OrganiserID = 2,
                    FinishTimeSeconds = 14520,
                    Position = 156,
                    Status = "finished",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Result
                {
                    ResultID = 3,
                    EnrolmentID = 4,
                    OrganiserID = 1,
                    FinishTimeSeconds = 3600,
                    Position = 18,
                    Status = "finished",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Result
                {
                    ResultID = 4,
                    EnrolmentID = 7,
                    OrganiserID = 1,
                    FinishTimeSeconds = 16200,
                    Position = 89,
                    Status = "finished",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
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
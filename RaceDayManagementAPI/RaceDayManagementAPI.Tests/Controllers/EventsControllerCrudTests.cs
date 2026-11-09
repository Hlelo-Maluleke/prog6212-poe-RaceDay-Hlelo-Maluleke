using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayManagementAPI.Controllers;
using RaceDayManagementAPI.Data;
using RaceDayManagementAPI.Models;
using RaceDayManagementAPI.Tests.Fakes;
using RaceDayManagementAPI.Tests.Fixtures;
using Xunit;

namespace RaceDayManagementAPI.Tests.Controllers
{
    /// <summary>
    /// 5 CRUD tests for EventsController:
    /// GET all, GET by ID, POST, PUT, DELETE.
    /// Each test uses a fresh in-memory database so tests are isolated.
    /// </summary>
    public class EventsControllerCrudTests : IDisposable
    {
        private readonly RaceDayDbContext _db;
        private readonly EventsController _controller;
        private readonly JwtServiceFixture _jwtFixture;

        public EventsControllerCrudTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new RaceDayDbContext(options);
            SeedTestData();

            _jwtFixture = new JwtServiceFixture();
            var fakeBlobService = new FakeBlobService();

            _controller = new EventsController(
                _jwtFixture.JwtService,
                _db,
                fakeBlobService);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        /// <summary>
        /// Seed known test data:
        /// - User 1 = organiser (Thabo) linked to Organiser 1
        /// - User 2 = participant
        /// - 2 events (IDs 1 and 2) owned by Organiser 1
        /// </summary>
        private void SeedTestData()
        {
            var seedDate = new DateTime(2026, 1, 1);

            _db.Users.AddRange(
                new User
                {
                    UserID = 1,
                    Email = "organiser@test.com",
                    PasswordHash = "x",
                    FullName = "Test Organiser",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new User
                {
                    UserID = 2,
                    Email = "participant@test.com",
                    PasswordHash = "x",
                    FullName = "Test Participant",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                }
            );

            _db.Organisers.Add(new Organiser
            {
                OrganiserID = 1,
                UserID = 1,
                CompanyName = "Test Club",
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

            _db.Participants.Add(new Participant
            {
                ParticipantID = 1,
                UserID = 2,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

            _db.Events.AddRange(
                new Event
                {
                    EventID = 1,
                    OrganiserID = 1,
                    Name = "Test Marathon",
                    Description = "Original marathon",
                    EventDate = new DateTime(2026, 6, 1),
                    StartTime = new TimeSpan(6, 0, 0),
                    Location = "Johannesburg",
                    EventType = "run",
                    Status = "published",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new Event
                {
                    EventID = 2,
                    OrganiserID = 1,
                    Name = "Test Cycle Tour",
                    EventDate = new DateTime(2026, 7, 1),
                    Location = "Cape Town",
                    EventType = "cycle",
                    Status = "published",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                }
            );

            _db.SaveChanges();
        }

        // TEST 1: GET ALL EVENTS
        [Fact(DisplayName = "GET_ReadEvents_ReturnsAllEvents")]
        public void GET_ReadEvents_ReturnsAllEvents()
        {
            // Arrange — no auth needed, public endpoint
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            var result = _controller.GetEvents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        // TEST 2: GET EVENT BY ID
        [Fact(DisplayName = "GET_ReadEventById_ReturnsEvent")]
        public void GET_ReadEventById_ReturnsEvent()
        {
            // Arrange
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            var result = _controller.GetEvent(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        // TEST 3: POST — Create Event
        [Fact(DisplayName = "POST_CreateEvent_CreatesEvent")]
        public void POST_CreateEvent_CreatesEvent()
        {
            // Arrange — log in as the organiser (UserID = 1)
            var claims = _jwtFixture.CreateOrganizerClaims(1);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claims };

            var request = new CreateEventRequest
            {
                Name = "New Test Race",
                Description = "A brand new test race",
                EventDate = new DateTime(2026, 8, 15),
                StartTime = new TimeSpan(6, 30, 0),
                Location = "Durban",
                EventType = "run",
                Status = "draft"
            };

            // Act
            var result = _controller.CreateEvent(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);
            Assert.Equal(3, _db.Events.Count()); // 2 seeded + 1 new
        }

        // TEST 4: PUT — Update Event
        [Fact(DisplayName = "PUT_UpdateEvent_UpdatesEvent")]
        public void PUT_UpdateEvent_UpdatesEvent()
        {
            // Arrange — log in as the organiser (UserID = 1)
            var claims = _jwtFixture.CreateOrganizerClaims(1);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claims };

            var request = new UpdateEventRequest
            {
                Name = "Updated Marathon Name",
                Status = "cancelled"
            };

            // Act — update event ID 1 (seeded)
            var result = _controller.UpdateEvent(1, request);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var updated = _db.Events.Find(1);
            Assert.NotNull(updated);
            Assert.Equal("Updated Marathon Name", updated!.Name);
            Assert.Equal("cancelled", updated.Status);
        }

        // TEST 5: DELETE — Remove Event
        [Fact(DisplayName = "DELETE_RemoveEvent_DeletesEvent")]
        public void DELETE_RemoveEvent_DeletesEvent()
        {
            // Arrange — log in as the organiser (UserID = 1)
            var claims = _jwtFixture.CreateOrganizerClaims(1);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claims };

            // Act — delete event ID 1
            var result = _controller.DeleteEvent(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(_db.Events.Find(1));
            Assert.Equal(1, _db.Events.Count()); 
        }
    }
}
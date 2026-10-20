using Microsoft.EntityFrameworkCore;
using RaceDayManagementAPI.Data;
using RaceDayManagementAPI.Models;

namespace RaceDayManagementAPI.Tests.Fixtures
{
    // Fixture for managing RaceDayDB test data and cleanup
    public class RaceDayDBFixture : IDisposable
    {
        public RaceDayDbContext DbContext { get; private set; }

        public RaceDayDBFixture()
        {
            // Create in-memory database for tests
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: "RaceDayDbTest_" + Guid.NewGuid())
                .Options;

            DbContext = new RaceDayDbContext(options);
            DbContext.Database.EnsureCreated();
        }


        public void Dispose()
        {
            DbContext?.Dispose();
        }
    }
}

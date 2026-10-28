using Microsoft.AspNetCore.Http;
using RaceDayManagementAPI.Services;

namespace RaceDayManagementAPI.Tests.Fakes
{
    public class FakeBlobService : IBlobService
    {
        public Task<string> UploadAsync(IFormFile file, string fileName)
        {
            // Return a fake URL for testing
            return Task.FromResult($"https://fake-blob.test/{fileName}");
        }

        public Task DeleteAsync(string fileName)
        {
            // Do nothing in tests
            return Task.CompletedTask;
        }
    }
}
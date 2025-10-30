using System;
using System.Threading;
using System.Threading.Tasks;
using ExportWeb.Background;
using ExportWeb.Data;
using ExportWeb.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Web.Tests
{
    public class ExportManagerTests
    {
        private ExportDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ExportDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ExportDbContext(options);
        }

        [Fact]
        public async Task EnqueueAsync_Should_CreateJob_And_Enqueue()
        {
            // Arrange
            var db = CreateDbContext();
            var channel = new ExportChannel();
            var manager = new ExportManager(db, channel);

            // Act
            var jobId = await manager.EnqueueAsync<DummyHandler>(new());

            // Assert
            var job = await db.ExportJobs.FindAsync(jobId);
            Assert.NotNull(job);
            Assert.Equal(0, (int)job.Status); // Pending = 0
        }

        private class DummyHandler : ExportWeb.Interfaces.IExportHandler
        {
            public Task<string> GenerateAsync(System.Collections.Generic.Dictionary<string, string> p, string d, CancellationToken t)
                => Task.FromResult(System.IO.Path.Combine(d, "dummy.txt"));
        }
    }
}

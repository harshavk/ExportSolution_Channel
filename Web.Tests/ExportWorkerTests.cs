using System;
using System.Threading;
using System.Threading.Tasks;
using ExportWeb.Background;
using ExportWeb.Data;
using ExportWeb.Models;
using ExportWeb.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Web.Tests
{
    public class ExportWorkerTests
    {
        private IServiceProvider BuildServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ExportDbContext>(opt =>
                opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddSingleton<IExportChannel, ExportChannel>();
            services.AddScoped<IExportService, FakeExportService>();
            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task Worker_Should_Process_Job_And_Set_Completed()
        {
            // Arrange
            var sp = BuildServices();
            var db = sp.GetRequiredService<ExportDbContext>();
            var channel = sp.GetRequiredService<IExportChannel>();

            var job = new ExportJob { FileName = "test.csv" };
            db.ExportJobs.Add(job);
            await db.SaveChangesAsync();

            await channel.EnqueueAsync(job.JobId);
            var logger = Mock.Of<ILogger<ExportWorker>>();
            var worker = new ExportWorker(sp, channel, logger);

            // Act
            var cts = new CancellationTokenSource();
            await worker.StartAsync(cts.Token);
            // Allow some time for processing
            await Task.Delay(500);
            // Stop the worker
            await worker.StopAsync(cts.Token);

            var jobAfter = await db.ExportJobs.FindAsync(job.JobId);

            // Assert
            Assert.Equal(ExportStatus.Completed, jobAfter.Status);
        }

        private class FakeExportService : IExportService
        {
            public Task GenerateAsync(ExportJob job, CancellationToken token)
            {
                var dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Exports");
                System.IO.Directory.CreateDirectory(dir);
                job.FilePath = System.IO.Path.Combine(dir, "fake.csv");
                System.IO.File.WriteAllText(job.FilePath, "ok");
                return Task.CompletedTask;
            }
        }
    }
}

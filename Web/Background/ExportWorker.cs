using System.Text.Json;
using ExportWeb.Data;
using ExportWeb.Interfaces;
using ExportWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ExportWeb.Background
{
    public class ExportWorker : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly IExportChannel _channel;
        private readonly ILogger<ExportWorker> _logger;
        private readonly SemaphoreSlim _semaphore = new(2); // Max parallel exports

        public ExportWorker(IServiceProvider sp, IExportChannel channel, ILogger<ExportWorker> logger)
        {
            _sp = sp;
            _channel = channel;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExportWorker started");

            await foreach (var jobId in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                await _semaphore.WaitAsync(stoppingToken);
                _ = ProcessJobAsync(jobId, stoppingToken).ContinueWith(t => _semaphore.Release());
            }
        }

        private async Task ProcessJobAsync(Guid jobId, CancellationToken token)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ExportDbContext>();
                var exportService = scope.ServiceProvider.GetRequiredService<IExportService>();

                var job = await db.ExportJobs.FirstOrDefaultAsync(j => j.JobId == jobId, token);
                if (job == null) return;

                job.Status = ExportStatus.Running;
                await db.SaveChangesAsync(token);

                // Optionally, you can resolve a specific handler by job.HandlerType
                // For demo we call a single ExportService, but in real usage
                // you can instantiate the handler based on job.HandlerType
                await exportService.GenerateAsync(job, token);

                job.Status = ExportStatus.Completed;
                job.CompletedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(token);

                _logger.LogInformation("Export completed {JobId}", jobId);
            }
            catch (Exception ex)
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ExportDbContext>();
                var job = await db.ExportJobs.FindAsync(jobId);
                if (job != null)
                {
                    job.Status = ExportStatus.Failed;
                    job.ErrorMessage = ex.ToString();
                    await db.SaveChangesAsync();
                }
                _logger.LogError(ex, "Export job {JobId} failed", jobId);
            }
        }
    }
}

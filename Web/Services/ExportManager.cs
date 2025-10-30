using System.Text.Json;
using ExportWeb.Background;
using ExportWeb.Data;
using ExportWeb.Interfaces;
using ExportWeb.Models;

namespace ExportWeb.Services
{
    public interface IExportManager
    {
        Task<Guid> EnqueueAsync<THandler>(Dictionary<string, string> parameters) where THandler : IExportHandler;
    }

    public class ExportManager : IExportManager
    {
        private readonly ExportDbContext _db;
        private readonly IExportChannel _channel;

        public ExportManager(ExportDbContext db, IExportChannel channel)
        {
            _db = db;
            _channel = channel;
        }

        public async Task<Guid> EnqueueAsync<THandler>(Dictionary<string, string> parameters) where THandler : IExportHandler
        {
            var job = new ExportJob
            {
                HandlerType = typeof(THandler).AssemblyQualifiedName,
                ParametersJson = JsonSerializer.Serialize(parameters),
                Status = ExportStatus.Pending
            };

            _db.ExportJobs.Add(job);
            await _db.SaveChangesAsync();

            await _channel.EnqueueAsync(job.JobId);
            return job.JobId;
        }
    }
}

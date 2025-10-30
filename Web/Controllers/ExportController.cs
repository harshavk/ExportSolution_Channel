using ExportWeb.Data;
using ExportWeb.Models;
using ExportWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExportWeb.Controllers
{
    [Route("export")]
    public class ExportController : Controller
    {
        private readonly ExportDbContext _db;
        private readonly IExportManager _manager;
        private readonly IWebHostEnvironment _env;

        public ExportController(ExportDbContext db, IExportManager manager, IWebHostEnvironment env)
        {
            _db = db;
            _manager = manager;
            _env = env;
        }

        [HttpPost("start/{handlerName}")]
        public async Task<IActionResult> Start(string handlerName)
        {
            // For convenience, map handlerName to a registered handler type by convention
            // e.g. handlerName = "UsersExportHandler"
            var handlerType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name.Equals(handlerName, StringComparison.OrdinalIgnoreCase)
                                     && typeof(ExportWeb.Interfaces.IExportHandler).IsAssignableFrom(t));

            if (handlerType == null) return BadRequest("Handler not found");

            // create parameters as needed; for demo empty
            var parameters = new Dictionary<string, string>();

            // Use reflection to call generic method EnqueueAsync<THandler>
            var method = typeof(IExportManager).GetMethod(nameof(IExportManager.EnqueueAsync))!;
            var generic = method.MakeGenericMethod(handlerType);
            var task = (Task<Guid>)generic.Invoke(_manager, new object[] { parameters })!;
            var jobId = await task;

            return Json(new { jobId });
        }

        [HttpGet("status/{id}")]
        public async Task<IActionResult> Status(Guid id)
        {
            var job = await _db.ExportJobs.FirstOrDefaultAsync(j => j.JobId == id);
            if (job == null) return NotFound();
            return Json(new { job.Status, job.FileName, job.FilePath, job.RequestedAt, job.CompletedAt, job.ErrorMessage });
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var list = await _db.ExportJobs.OrderByDescending(j => j.RequestedAt).Take(100).ToListAsync();
            return Json(list);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var job = await _db.ExportJobs.FindAsync(id);
            if (job == null || job.Status != ExportStatus.Completed) return NotFound();

            if (!System.IO.File.Exists(job.FilePath)) return NotFound("File missing");

            var bytes = await System.IO.File.ReadAllBytesAsync(job.FilePath);
            return File(bytes, "application/octet-stream", job.FileName);
        }
    }
}

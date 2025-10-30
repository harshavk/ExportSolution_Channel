using ExportWeb.Models;

namespace ExportWeb.Services
{
    public interface IExportService
    {
        Task GenerateAsync(ExportJob job, CancellationToken token);
    }

    public class ExportService : IExportService
    {
        private readonly IWebHostEnvironment _env;

        public ExportService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task GenerateAsync(ExportJob job, CancellationToken token)
        {
            // For demo purposes, we only create a CSV file with dummy data.
            // Replace this logic with Aspose or CsvHelper streaming implementation.
            var exportDir = Path.Combine(_env.ContentRootPath, "App_Data", "Exports");
            Directory.CreateDirectory(exportDir);

            var fileName = $"{job.JobId}.csv";
            var fullPath = Path.Combine(exportDir, fileName);
            job.FileName = fileName;
            job.FilePath = fullPath;

            await Task.Run(() =>
            {
                using var sw = new StreamWriter(fullPath);
                sw.WriteLine("Id,Name,Value");
                for (int i = 1; i <= 100000; i++)
                {
                    sw.WriteLine($"{i},Item-{i},Value-{i}");
                }
            }, token);
        }
    }
}

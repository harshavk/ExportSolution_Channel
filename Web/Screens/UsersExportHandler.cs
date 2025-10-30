using ExportWeb.Interfaces;

namespace ExportWeb.Screens
{
    public class UsersExportHandler : IExportHandler
    {
        public async Task<string> GenerateAsync(Dictionary<string, string> parameters, string exportDirectory, CancellationToken token)
        {
            var fileName = $"Users_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            var path = Path.Combine(exportDirectory, fileName);
            Directory.CreateDirectory(exportDirectory);

            await Task.Run(() =>
            {
                using var sw = new StreamWriter(path);
                sw.WriteLine("Id,Name,Email");
                for (int i = 1; i <= 50000; i++)
                {
                    sw.WriteLine($"{i},User-{i},user{i}@example.com");
                }
            }, token);

            return path;
        }
    }
}

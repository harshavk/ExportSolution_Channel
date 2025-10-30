namespace ExportWeb.Interfaces
{
    public interface IExportHandler
    {
        /// <summary>
        /// Generate the export file and return the full file path.
        /// </summary>
        Task<string> GenerateAsync(Dictionary<string, string> parameters, string exportDirectory, CancellationToken token);
    }
}

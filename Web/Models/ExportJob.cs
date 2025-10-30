using System.ComponentModel.DataAnnotations;

namespace ExportWeb.Models
{
    public enum ExportStatus { Pending = 0, Running = 1, Completed = 2, Failed = 3 }

    public class ExportJob
    {
        [Key]
        public Guid JobId { get; set; } = Guid.NewGuid();

        public string HandlerType { get; set; }

        public string ParametersJson { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public ExportStatus Status { get; set; } = ExportStatus.Pending;

        public string ErrorMessage { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}

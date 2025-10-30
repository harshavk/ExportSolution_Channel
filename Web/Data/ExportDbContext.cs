using ExportWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ExportWeb.Data
{
    public class ExportDbContext : DbContext
    {
        public ExportDbContext(DbContextOptions<ExportDbContext> options) : base(options) { }

        public DbSet<ExportJob> ExportJobs { get; set; }
    }
}

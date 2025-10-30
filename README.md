# ExportSolution (Generic Export Framework)

This is a minimal .NET 8 Web MVC style project that demonstrates a generic, extensible export framework using:
- System.Threading.Channels (in-process queue)
- BackgroundService worker with concurrency limits
- EF Core persistence for export jobs
- Pluggable export handlers

Folder layout:
- Web/ (main project)
  - Controllers/ExportController.cs
  - Background/ExportChannel.cs
  - Background/ExportWorker.cs
  - Services/ExportService.cs
  - Services/ExportManager.cs
  - Interfaces/IExportHandler.cs
  - Data/ExportDbContext.cs
  - Models/ExportJob.cs
  - Screens/UsersExportHandler.cs
  - Views/Export/List.cshtml
  - Program.cs
  - appsettings.json
  - Web.csproj

Build & run:
1. Move the `Web` folder into your solution or open this folder as a project.
2. Update the connection string in `appsettings.json`.
3. Run migrations (or create one) for the `ExportDbContext`.
   Example:
     dotnet ef migrations add InitExportJobs -p Web -s Web
     dotnet ef database update -p Web -s Web
4. Run:
     dotnet run --project Web

This package is a starting point — replace placeholder generation logic (CSV writer) with your Aspose or CsvHelper streaming code.


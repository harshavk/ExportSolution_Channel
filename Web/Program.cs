using ExportWeb.Background;
using ExportWeb.Data;
using ExportWeb.Services;
using ExportWeb.Screens;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// EF Core - configure your connection string in appsettings.json
builder.Services.AddDbContext<ExportDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Channel + manager + worker
builder.Services.AddSingleton<ExportWeb.Background.IExportChannel, ExportWeb.Background.ExportChannel>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IExportManager, ExportManager>();
builder.Services.AddHostedService<ExportWorker>();

// Register handlers (add your handlers here)
builder.Services.AddScoped<UsersExportHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

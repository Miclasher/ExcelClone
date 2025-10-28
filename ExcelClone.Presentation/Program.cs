using ExcelClone.Domain.Repository;
using ExcelClone.Infrastructure;
using ExcelClone.Services;
using ExcelClone.Services.Abstractions;

namespace ExcelClone.Presentation;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();

        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<GoogleDriveRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var credentialsPath = config["GoogleDrive:CredentialsPath"];
            var folderId = config["GoogleDrive:FolderId"];

            return new GoogleDriveRepository(credentialsPath, folderId);
        });
        builder.Services.AddSingleton<ITableRepository, HybridTableRepository>();
        builder.Services.AddScoped<ITableService, TableService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}

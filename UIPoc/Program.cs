using Microsoft.EntityFrameworkCore;
using Radzen;
using UIPooc.Components;
using UIPooc.Data;
using UIPooc.Services;

namespace UIPooc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            builder.Services.AddDbContext<HoldingsDbContext>(options => 
                options
                .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                //.UseAsyncSeeding(async (context, _, cancellationToken) =>
                //    {
                //        await ((HoldingsDbContext)context).SeedDefaultUserAsync();
                //    }
                //)
            );
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IExcelImportService, ExcelImportService>();
            builder.Services.AddRadzenComponents();

            WebApplication app = builder.Build();

            await InitializeDatabaseAsync(app);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

            app.Run();
        }

        static async Task InitializeDatabaseAsync(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<HoldingsDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    var connectionString = dbContext.Database.GetConnectionString();
                    logger.LogInformation("Connection String: {ConnectionString}", connectionString);
                    logger.LogInformation("Attempting to connect to database and apply migrations...");

                    // EF Core will automatically create the database if it doesn't exist when running migrations
                    await dbContext.Database.MigrateAsync();
                    await dbContext.SeedDefaultUserAsync();
                    logger.LogInformation("Database created/updated and migrations applied successfully!");

                }
                catch (Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    logger.LogError(sqlEx, "SQL Connection Error (Error Number: {ErrorNumber}): {Message}", 
                        sqlEx.Number, sqlEx.Message);
                    logger.LogWarning("Troubleshooting steps:");
                    logger.LogWarning("1. Check if LocalDB is installed: sqllocaldb info");
                    logger.LogWarning("2. Create LocalDB instance: sqllocaldb create mssqllocaldb");
                    logger.LogWarning("3. Start LocalDB instance: sqllocaldb start mssqllocaldb");
                    logger.LogWarning("4. Verify instance: sqllocaldb info mssqllocaldb");
                    logger.LogWarning("Application will continue, but database features will not work.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while initializing the database.");
                    logger.LogWarning("Application will continue, but database features may not work correctly.");
                }
            }
        }

        
    }
}

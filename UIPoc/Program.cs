using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Radzen.Blazor.Markdown;
using Radzen.Blazor.Rendering;
using System.Runtime.ConstrainedExecution;
using UIPooc.Components;
using UIPooc.Data;
using UIPooc.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


//Singleton: This creates only one instance of a class during the application's lifecycle. Every time you request this class, you get the same instance.
//Use it for classes that are expensive to create or maintain a common state throughout the application, like a database connection.
//Transient: Every time you request a transient class, a new instance is created.This is useful for lightweight,
//stateless services where each operation requires a clean and independent instance.
//Scoped: Scoped instances are created once per client request.In a web application, for example,
//a new instance is created for each HTTP request but is shared across that request.
//Use it for services that need to maintain state within a request but not beyond it, like shopping cart in an e - commerce site.

namespace UIPooc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            builder.Services.AddDbContext<HoldingsDbContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                //.UseAsyncSeeding(async (context, _, cancellationToken) =>
                //    {
                //        await ((HoldingsDbContext)context).SeedDefaultUserAsync();
                //    }
                //)
            );
            builder.Services.AddScoped<IModelService, ModelService>();
            builder.Services.AddScoped<IImportService, ImportService>();
            builder.Services.AddScoped<IExportService, ExportService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddHttpClient<IFinanceService, FinanceService>();
            builder.Services.AddHostedService<EquityMarketSyncDaemon>();
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
            using (IServiceScope scope = app.Services.CreateScope())
            {
                HoldingsDbContext dbContext = scope.ServiceProvider.GetRequiredService<HoldingsDbContext>();
                ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

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

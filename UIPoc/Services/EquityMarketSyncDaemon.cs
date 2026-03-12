using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UIPooc.Data;
using UIPooc.Models;

namespace UIPooc.Services
{
    public class EquityMarketSyncDaemon : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EquityMarketSyncDaemon> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

        private List<Equity> _equities;
        public EquityMarketSyncDaemon(
            IServiceProvider serviceProvider,
            ILogger<EquityMarketSyncDaemon> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _equities = new List<Equity>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EquityMarketSyncService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncEquityMarketsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while syncing equity markets.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("EquityMarketSyncService is stopping.");
        }

        private async Task SyncEquityMarketsAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            HoldingsDbContext dbContext = scope.ServiceProvider.GetRequiredService<HoldingsDbContext>();
            IFinanceService financeService = scope.ServiceProvider.GetRequiredService<IFinanceService>();

            try
            {
                // Get all unique symbols from Equity table
                //var equities = await dbContext.Equities
                //    .Select(e => new { e.Symbol, e.Market, e.Currency })
                //    .Distinct()
                //    .ToListAsync(cancellationToken);

                if (_equities.Count == 0)
                {
                    this._equities = await dbContext.Equities.Distinct().ToListAsync(cancellationToken);
                }

                if (!_equities.Any())
                {
                    _logger.LogInformation("No equities found to sync.");
                    return;
                }

                _logger.LogInformation("Syncing {Count} unique equity symbols...", _equities.Count);

                int successCount = 0;
                int failureCount = 0;

                //foreach (Equity equity in equities)
                //{

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                Equity equity = this._equities[0];

                try
                {
                    // GetQuoteAndCacheAsync will automatically add to EquityMarket if not exists
                    // and update if it already exists
                    EquityMarket? equityMarket = await financeService.GetQuoteAndCacheAsync(equity.Symbol, equity.Market);
                    this._equities.RemoveAt(0);

                    if (equityMarket != null)
                    {
                        _logger.LogDebug("Successfully synced {Symbol} ({Market})", equity.Symbol, equity.Market);
                        successCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to fetch quote for {Symbol} ({Market})", equity.Symbol, equity.Market);
                        failureCount++;
                    }

                    // Small delay to avoid overwhelming the API
                    await Task.Delay(100, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing {Symbol} ({Market})", equity.Symbol, equity.Market);
                    failureCount++;
                }
                //}

                _logger.LogInformation("Equity market sync completed: {Success} successful, {Failures} failed", successCount, failureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SyncEquityMarketsAsync");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EquityMarketSyncService is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Radzen.Blazor.Rendering;
using UIPooc.Data;
using UIPooc.Models;

namespace UIPooc.Services
{
    public readonly struct StockPriceSnapshot(decimal price, DateTime lastUpdated)
    {
        public decimal Price { get; } = price;
        public DateTime LastUpdated { get; } = lastUpdated;
    }

    public class EquityMarketSyncDaemon : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EquityMarketSyncDaemon> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);
        //public static readonly Dictionary<string, StockPriceSnapshot> _priceCache = new (StringComparer.OrdinalIgnoreCase);
        public static readonly Dictionary<string, TickerPriceEntity> _priceCache = new(StringComparer.OrdinalIgnoreCase);
        public static readonly Dictionary<string, FullStockPriceEntity> _fullStockPriceCache = new(StringComparer.OrdinalIgnoreCase);

        private List<Equity> _equities;
        public EquityMarketSyncDaemon(IServiceProvider serviceProvider, ILogger<EquityMarketSyncDaemon> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _equities = new List<Equity>();
        }

        static readonly TimeOnly TRADING_START_UTC = TimeOnly.Parse("14:30");
        static readonly TimeOnly TRADING_FINISH_UTC = TimeOnly.Parse("21:00");
        static readonly int  TICKER_CACHE_DURATION_MINUTES = 120; // 2 hours
        static readonly int SYMBOL_FULL_PRICE_CACHE_DURATION_MINUTES = 480; // 4 hours


        /// <summary>
        /// Pre-market session: 4:00 a.m. – 9:30 a.m. ET.  (09:00 to 14:30 UTC)
        // After-hours session: 4:00 p.m. – 8:00 p.m.ET.   (21:00 to 01:00 UTC)
        // Overnight trading: Some platforms allow trading between 8:00 p.m.and 4:00 a.m.ET.
        // Regular trading hours: 9:30 a.m. – 4:00 p.m ET.   (14:30 to 21:00 UTC)
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="live"></param>
        /// <returns></returns>
        /// 

        static public bool IsTradingTime()
        {
            DateTime now = DateTime.UtcNow;
            DayOfWeek day = now.DayOfWeek;

            if ((day == DayOfWeek.Saturday) || (day == DayOfWeek.Sunday))
            {
                return false;
            }

            TimeOnly nowTimeOnly = TimeOnly.FromDateTime(now);
            if (nowTimeOnly < TRADING_START_UTC || nowTimeOnly > TRADING_FINISH_UTC)
            {
                return false;
            }

            return true;
        }

        static public async Task<TickerPriceEntity> RequestTickerPriceAsync(string ticker, string market = "US", bool live = false)
        {
            if (!live && EquityMarketSyncDaemon._priceCache.TryGetValue(ticker, out var snapshot))
            {
                if (!IsTradingTime())
                {
                    return snapshot;
                }

                if ((DateTime.UtcNow - snapshot.LastUpdated).TotalMinutes < TICKER_CACHE_DURATION_MINUTES)
                {
                    return snapshot;
                }
            }

            if (market == "CDN" && !ticker.EndsWith(".TO"))
            {
                ticker += ".TO";
            }

            TickerPriceEntity result = await YahooHttpClient.GetYhTickerPriceAsync(ticker);

            if (!string.IsNullOrEmpty(result.Error))
            {
                return result;
            }

            EquityMarketSyncDaemon._priceCache[ticker] = result;
            return result;
        }

        static public async Task<FullStockPriceEntity> RequestFullStockPriceAsync(string symbol, string market = "US", bool live = false)
        {
            if (!live && EquityMarketSyncDaemon._fullStockPriceCache.TryGetValue(symbol, out var snapshot))
            {
                if (!IsTradingTime())
                {
                    return snapshot;
                }

                if ((DateTime.UtcNow - snapshot.LastUpdated).TotalMinutes < SYMBOL_FULL_PRICE_CACHE_DURATION_MINUTES)
                {
                    return snapshot;
                }
            }

            if (market == "CDN")
            {
                symbol += ".TO";
            }

            FullStockPriceEntity result = await YahooHttpClient.GetYhFullStockPrice(symbol);
            EquityMarketSyncDaemon._fullStockPriceCache[symbol] = result;
            return result;
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
            //IFinanceService financeService = scope.ServiceProvider.GetRequiredService<IFinanceService>();

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
                    //EquityMarket? equityMarket = await financeService.GetQuoteAndCacheAsync(equity.Symbol, equity.Market);

                    FullStockPriceEntity? fullStockPrice = await RequestFullStockPriceAsync(equity.Symbol);
                    this._equities.RemoveAt(0);

                    //if (equityMarket != null)
                    //{
                    //    _logger.LogDebug("Successfully synced {Symbol} ({Market})", equity.Symbol, equity.Market);
                    //    successCount++;
                    //}
                    //else
                    //{
                    //    _logger.LogWarning("Failed to fetch quote for {Symbol} ({Market})", equity.Symbol, equity.Market);
                    //    failureCount++;
                    //}

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

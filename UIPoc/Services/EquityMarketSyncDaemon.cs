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
        //private readonly IModelService _modelService;
        private readonly ILogger<EquityMarketSyncDaemon> _logger;
        //public static readonly Dictionary<string, StockPriceSnapshot> _priceCache = new (StringComparer.OrdinalIgnoreCase);
        public static readonly Dictionary<string, TickerPriceEntity>    _priceCache = new(StringComparer.OrdinalIgnoreCase);
        public static readonly Dictionary<string, FullStockPriceEntity> _fullStockPriceCache = new(StringComparer.OrdinalIgnoreCase);

        public static readonly Dictionary<string, Equity> _equity = new(StringComparer.OrdinalIgnoreCase);

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
            DateTime currentTime = DateTime.UtcNow;
            DayOfWeek day = currentTime.DayOfWeek;

            if ((day == DayOfWeek.Saturday) || (day == DayOfWeek.Sunday))
            {
                return false;
            }

            TimeOnly nowTimeOnly = TimeOnly.FromDateTime(currentTime);
            if (nowTimeOnly < TRADING_START_UTC || nowTimeOnly > TRADING_FINISH_UTC)
            {
                return false;
            }

            return true;
        }

        static public bool IsEquityUpToDate(DateTime equityDateTime)
        {
            if (equityDateTime.Date != DateTime.UtcNow.Date)
            {
                return false;
            }

            DayOfWeek day = equityDateTime.DayOfWeek;

            if ((day == DayOfWeek.Saturday) || (day == DayOfWeek.Sunday))
            {
                return true;
            }

            // Weekday
            TimeOnly equityTimeOnly = TimeOnly.FromDateTime(equityDateTime);

            if (equityTimeOnly < TRADING_START_UTC || equityTimeOnly > TRADING_FINISH_UTC)
            {
                return true;
            }

            if (equityTimeOnly - TRADING_START_UTC < TimeSpan.FromHours(4))
            {
                return true;
            }

            return false;
        }


        static public async Task<TickerPriceEntity> RequestTickerPriceAsync(string ticker, bool live = false)
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

            //if (market == "CDN" && !ticker.EndsWith(".TO"))
            //{
            //    ticker += ".TO";
            //}

            TickerPriceEntity result = await YahooHttpClient.GetYhTickerPriceAsync(ticker);

            if (!string.IsNullOrEmpty(result.Error))
            {
                return result;
            }

            EquityMarketSyncDaemon._priceCache[ticker] = result;
            return result;
        }

        static public async Task<FullStockPriceEntity> RequestFullStockPriceAsync(string symbol, bool live = false)
        {
            if (!live && EquityMarketSyncDaemon._fullStockPriceCache.TryGetValue(symbol, out var cached))
            {
                if (!IsTradingTime())
                {
                    return cached;
                }

                if ((DateTime.UtcNow - cached.LastUpdated).TotalMinutes < SYMBOL_FULL_PRICE_CACHE_DURATION_MINUTES)
                {
                    return cached;
                }
            }

            FullStockPriceEntity result = await YahooHttpClient.GetYhFullStockPrice(symbol);
            EquityMarketSyncDaemon._fullStockPriceCache[symbol] = result;
            return result;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EquityMarketSyncService is starting.");
            
            await PereodicTask(stoppingToken, TimeSpan.FromMinutes(2));

            _logger.LogInformation("EquityMarketSyncService is stopping.");
        }

        private async Task PereodicTask(CancellationToken stoppingToken, TimeSpan delay)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (IsTradingTime())
                    {
                        //
                    }
                    else // Only sync during non-trading hours to avoid hitting API rate limits and to ensure we get the closing price on weekedays
                    {
                        await UpdateEquityOneInTimeAsync(stoppingToken);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while syncing equity markets.");
                }

                await Task.Delay(delay, stoppingToken);
            }
        }


        private async Task UpdateEquityOneInTimeAsync(CancellationToken cancellationToken)
        {
            //HoldingsDbContext dbContext = scope.ServiceProvider.GetRequiredService<HoldingsDbContext>();
            //IFinanceService financeService = scope.ServiceProvider.GetRequiredService<IFinanceService>();

            using IServiceScope scope = _serviceProvider.CreateScope();
            IModelService modelService = scope.ServiceProvider.GetRequiredService<IModelService>();

            try
            {
                // Get all unique symbols from Equity table
                //var equities = await dbContext.Equities
                //    .Select(e => new { e.Symbol, e.Market, e.Currency })
                //    .Distinct()
                //    .ToListAsync(cancellationToken);

                if (_equities.Count == 0)
                {
                    this._equities = await modelService.GetAllEquitiesAsync();
                }

                if (!_equities.Any())
                {
                    _logger.LogInformation("No equities found to sync.");
                    return;
                }

                _logger.LogInformation("Syncing {Count} unique equity symbols...", _equities.Count);

                //int successCount = 0;
                //int failureCount = 0;

                //foreach (Equity equity in equities)
                //{

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                Equity equity = this._equities[0];

                await EtlEquityAsync(cancellationToken, modelService, equity);

                this._equities.RemoveAt(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EtlEquityAsync");
                //failureCount++;
            }

            //_logger.LogInformation("Equity market sync completed: {Success} successful, {Failures} failed", successCount, failureCount);
        }

        private async Task<EquityMarket> AddEquityMarketAsync(CancellationToken cancellationToken, IModelService modelService, string symbol)
        {
            FullStockPriceEntity? fullStockPrice = await RequestFullStockPriceAsync(symbol);

            EquityMarket equityMarket = new EquityMarket
            {
                Symbol = symbol
            };

            fullStockPrice.ToDatabaseEquityMarket(equityMarket);

            //using IServiceScope scope = _serviceProvider.CreateScope();
            //IModelService _modelService = scope.ServiceProvider.GetRequiredService<IModelService>();
            await modelService.CreateEquityMarketAsync(equityMarket);

            return equityMarket;
        }


        private async Task EtlEquityAsync(CancellationToken cancellationToken, IModelService modelService, Equity equity)
        {
            try
            {
                var equityMarket = await modelService.GetEquityMarketBySymbolAsync(equity.Symbol);

                if (equityMarket == null)
                {
                    equityMarket = await AddEquityMarketAsync(cancellationToken, modelService, equity.Symbol);
                }
                else
                {
                    if( !IsEquityUpToDate(equityMarket.LastUpdated) )
                    {
                        FullStockPriceEntity? fullStockPrice = await RequestFullStockPriceAsync(equity.Symbol);
                        fullStockPrice.ToDatabaseEquityMarket(equityMarket);
                        await modelService.UpdateEquityAsync(equity);
                    }
                }


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
                //failureCount++;
            }
            //}
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EquityMarketSyncService is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}

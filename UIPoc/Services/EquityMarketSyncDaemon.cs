using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Radzen.Blazor.Rendering;
using UIPooc.Data;
using UIPooc.Models;
using UIPooc.Utils;
using UIPooc.Yahoo;

namespace UIPooc.Services;

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




    static public async Task<TickerPriceEntity> RequestTickerPriceAsync(string ticker, bool canUseCache = true)
    {
        if (canUseCache && EquityMarketSyncDaemon._priceCache.TryGetValue(ticker, out var _cachedPrice))
        {
            if (!TimeUtils.IsTradingTime())
            {
                return _cachedPrice;
            }

            if (!TimeUtils.IsTicketPriceCacheExpired(_cachedPrice.LastUpdated))
            {
                return _cachedPrice;
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

    static public async Task<FullStockPriceEntity> RequestFullStockPriceAsync(string symbol, bool canUseCache = true)
    {
        if (canUseCache && EquityMarketSyncDaemon._fullStockPriceCache.TryGetValue(symbol, out var _cachedFullStockPrice))
        {
            if (!TimeUtils.IsTradingTime())
            {
                return _cachedFullStockPrice;
            }

            if (!TimeUtils.IsFullStockPriceCacheExpired(_cachedFullStockPrice.LastUpdated))
            {
                return _cachedFullStockPrice;
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
                if (TimeUtils.IsTradingTime())
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
                if( !TimeUtils.IsEquityUpToDate(equityMarket.LastUpdated) )
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


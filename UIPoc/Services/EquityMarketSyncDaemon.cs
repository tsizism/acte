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
                    // Only update the price cache during trading hours to ensure we have the most up-to-date prices for any real-time features,
                    // but avoid doing the full sync which is more resource intensive and not necessary during trading hours
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

            Equity? equity = this._equities.FirstOrDefault();
            //Equity? equity = this._equities.Find(e => e.Symbol.Equals("ENB", StringComparison.OrdinalIgnoreCase));

            // Handle case where equity is not found (should not happen since we are iterating over the list)

            string marketSymbol = EquityUtils.GetSymbolAdjustedToMarket(equity!);
            await this.UpdateEquityAsync(cancellationToken, modelService, marketSymbol);

            this._equities.RemoveAt(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EtlEquityAsync");
            //failureCount++;
        }

        //_logger.LogInformation("Equity market sync completed: {Success} successful, {Failures} failed", successCount, failureCount);
    }

    //private async Task<EquityMarket> AddEquityMarketAsync(CancellationToken cancellationToken, IModelService modelService, string symbol)
    //{
    //    FullStockPriceEntity? fullStockPrice = await RequestFullStockPriceAsync(symbol);

    //    EquityMarket equityMarket = new EquityMarket
    //    {
    //        Symbol = symbol
    //    };

    //    fullStockPrice.ToDatabaseEquityMarket(equityMarket);

    //    //using IServiceScope scope = _serviceProvider.CreateScope();
    //    //IModelService _modelService = scope.ServiceProvider.GetRequiredService<IModelService>();
    //    await modelService.CreateEquityMarketAsync(equityMarket);

    //    return equityMarket;
    //}


    //string marketSymbol = EquityUtils.GetSymbolAdjustedToMarket(equity);

    private async Task UpdateEquityAsync(CancellationToken cancellationToken, IModelService modelService, string marketSymbol)
    {
        try
        {
            EquityMarket? equityMarket = await modelService.GetEquityMarketBySymbolAsync(marketSymbol);

            if (true) // equityMarket == null || !TimeUtils.IsEquityUpToDate(equityMarket.LastUpdated))
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                IFinanceService financeService = scope.ServiceProvider.GetRequiredService<IFinanceService>();
                FullStockPriceEntity? fullStockPrice = await financeService.RequestFullStockPriceAsync(marketSymbol);

                //FullStockPriceEntity? fullStockPrice = await RequestFullStockPriceAsync(marketSymbol);

                bool newEquityMarket = equityMarket == null;
                equityMarket ??= new EquityMarket {Symbol = marketSymbol };

                fullStockPrice.ToDatabaseEquityMarket(equityMarket);

                if (newEquityMarket)
                {
                    await modelService.CreateEquityMarketAsync(equityMarket);
                }
                else
                {
                    await modelService.UpdateEquityMarketAsync(equityMarket);
                }
            }



            //if (equityMarket == null)
            //{
            //    //equityMarket = await AddEquityMarketAsync(cancellationToken, modelService, marketSymbol);

            //    FullStockPriceEntity? fullStockPrice = await RequestFullStockPriceAsync(marketSymbol);

            //    equityMarket = new EquityMarket
            //    {
            //        Symbol = marketSymbol
            //    };

            //    fullStockPrice.ToDatabaseEquityMarket(equityMarket);

            //    //using IServiceScope scope = _serviceProvider.CreateScope();
            //    //IModelService _modelService = scope.ServiceProvider.GetRequiredService<IModelService>();
            //    await modelService.CreateEquityMarketAsync(equityMarket);

            //}
            //else
            //{
            //    if( !TimeUtils.IsEquityUpToDate(equityMarket.LastUpdated) )
            //    {
            //        FullStockPriceEntity? fullStockPrice = await RequestFullStockPriceAsync(equity.Symbol);
            //        fullStockPrice.ToDatabaseEquityMarket(equityMarket);
            //        await modelService.UpdateEquityMarketAsync(equityMarket);
            //        //await modelService.UpdateEquityAsync(equity);
            //    }
            //}


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
            _logger.LogError(ex, "Error syncing {Symbol} ({Market})", marketSymbol, marketSymbol);
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


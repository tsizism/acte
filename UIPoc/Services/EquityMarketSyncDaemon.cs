using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Radzen.Blazor.Markdown;
using Radzen.Blazor.Rendering;
using System.Collections;
using System.Timers;
using UIPooc.Data;
using UIPooc.Models;
using UIPooc.Utils;
using UIPooc.Yahoo;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace UIPooc.Services;


/*
For your case, I would usually combine:
1.	Component Diagram → high-level architecture 
2.	Activity Diagram → scheduling logic/runtime flow

Good tools
•	draw.io (diagrams.net) 
•	Lucidchart 
•	PlantUML 
•	Mermaid


Write Component Diagram using mermaid for software scheduler of 3 workers. Trading worker - time (9:30 -16:00 est), Overnight worker- Between trading days, OffTrading worker – weekend or holiday
Scheduler maintains cache which is a dictionary with market ticker as key. Value has Market previous day price, current price, market currency, Update Time, day high day low and TTL. 
Trading worker checks cache with Trading TTL, database with Trading TTL, last resort queries Yh client for Stock Price (short result) and update cache and db.
Overnight worker checks cache with Overnight TTL, database with Overnight TTL, last resort queries Yh client for Full Stock Price (full result) and update cache and db.
OffTrading worker – checks Stock Full Information and updates summary,52 weeks, 50day, 200day etc info in stock market table

flowchart TD
    Start([Ticker Price Request])
    MarketOpen{Market Open?}
    Trading["Trading Worker"]
    Overnight{Trading Day
    After Close?}
    OvernightWorker["Overnight Worker"]
    OffTrading["OffTrading Worker"]
    End([Process Complete])
    Start --> MarketOpen
    MarketOpen -->|Yes| Trading
    MarketOpen -->|No| Overnight
    Overnight -->|Yes| OvernightWorker
    Overnight -->|No| OffTrading
    Trading --> End
    OvernightWorker --> End
    OffTrading --> End

Write Activity Diagram using mermaid for software scheduler of 3 workers. Trading worker - time (9:30 -16:00 est), Overnight worker- Between trading days, OffTrading worker – weekend or holiday
Scheduler maintains cache which is a dictionary with market ticker as key. Value has Market previous day price, current price, market currency, Update Time, day high day low and TTL. 
Trading worker checks cache with Trading TTL, database with Trading TTL, last resort queries Yh client for Stock Price (short result) and update cache and db.
Overnight worker checks cache with Overnight TTL, database with Overnight TTL, last resort queries Yh client for Full Stock Price (full result) and update cache and db.
OffTrading worker – checks Stock Full Information and updates summary,52 weeks, 50day, 200day etc info in stock market table

---
config:
  layout: fixed
---
flowchart TD
    Start([Ticker Price Request])
    SelectWorker{Current Time?}
    Trading["Trading Worker
    09:30 - 16:00 EST"]
    Overnight["Overnight Worker
    Between Trading Days"]
    OffTrading["OffTrading Worker
    Weekend / Holiday"]
    Start --> SelectWorker
    SelectWorker -->|Market Open| Trading
    SelectWorker -->|After Close| Overnight
    SelectWorker -->|Weekend/Holiday| OffTrading

    %% Trading Worker Flow
    Trading --> TCache{"Cache Entry
    Trading TTL Valid?"}
    TCache -->|Yes| ReturnTrading["Return Price"]
    TCache -->|No| TDB{"Database Entry
    Trading TTL Valid?"}
    TDB -->|Yes| UpdateTradingCache["Refresh Cache"]
    UpdateTradingCache --> ReturnTrading
    TDB -->|No| TYH["Yahoo Finance 
    Get Stock Price (Short)"]
    TYH --> UpdateTrading["Update Cache & DB"]
    UpdateTrading --> ReturnTrading

    %% Overnight Worker Flow
    Overnight --> OCache{"Cache Entry
    Overnight TTL Valid?"}
    OCache -->|Yes| ReturnOvernight["Return Full Price"]
    OCache -->|No| ODB{"Database Entry
    Overnight TTL Valid?"}
    ODB -->|Yes| UpdateOvernightCache["Refresh Cache"]
    UpdateOvernightCache --> ReturnOvernight
    ODB -->|No| OYH["Yahoo Finance
    Get Stock Price (Medium)"]
    OYH --> UpdateOvernight["Update Cache & DB"]
    UpdateOvernight --> ReturnOvernight

    %% OffTrading Worker Flow
    OffTrading --> FullInfo["Yahoo Finance
    Get Full Stock Information"]
    FullInfo --> UpdateMarketTable["Update Stock Table
    • Summary
    • 52 Week High/Low
    • 50 Day Average
    • 200 Day Average
    • Market Statistics"]
    UpdateMarketTable --> End([Complete])
    ReturnTrading --> End
    ReturnOvernight --> End

*/

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
    public static readonly Dictionary<string, YhStockPriceResult>    _priceCache = new(StringComparer.OrdinalIgnoreCase);
    public static readonly Dictionary<string, YhGetFullStockPriceResult> _fullStockPriceCache = new(StringComparer.OrdinalIgnoreCase);

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
            await this.CreateOrUpdateMarketEquityAsync(cancellationToken, modelService, marketSymbol);  // Needed?

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

    private async Task CreateOrUpdateMarketEquityAsync(CancellationToken cancellationToken, IModelService modelService, string marketSymbol)
    {
        try
        {
            EquityMarket? equityMarket = await modelService.GetEquityMarketBySymbolAsync(marketSymbol);

            if (true) // equityMarket == null || !TimeUtils.IsEquityUpToDate(equityMarket.LastUpdated))
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                IFinanceService financeService = scope.ServiceProvider.GetRequiredService<IFinanceService>();
                YhGetFullStockPriceResult? fullStockPrice = await financeService.RequestFullStockPriceAsync(marketSymbol);

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


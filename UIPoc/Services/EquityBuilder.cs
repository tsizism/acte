using UIPooc.Models;
using UIPooc.Utils;
using UIPooc.Yahoo;

namespace UIPooc.Services;

public class EquityBuilder
{
    private readonly YhHttpClient _yhHttpClient;
    private readonly IModelService _modelService;
    private readonly ILogger<EquityBuilder> _logger;
    private EquityMarket? _equityMarket;

    public EquityBuilder(YhHttpClient yhHttpClient, IModelService modelService)
    {
        _yhHttpClient = yhHttpClient;
        _modelService = modelService;
        _logger = new Logger<EquityBuilder>(new LoggerFactory());
        _equityMarket = null;
    }

    /// <summary>
    /// Populates <see cref="EquityMarket"/> from the database, falling back to the in-memory cache
    /// when the data is still fresh.
    /// </summary>
    public async Task<EquityBuilder> WithEquitySymbol(string symbol) //, string market)
    {
        _equityMarket = null;
        if (FinanceService._equityMarketCache.TryGetValue(symbol, out var cached))
        {
            _logger.LogDebug("EquityMarket for {Symbol} resolved from cache.", symbol);

            if (TimeUtils.IsUpToDateWithMarket(cached.LastUpdated))
            {
                _equityMarket = cached;
                _logger.LogDebug("EquityMarket for {Symbol} is up-to-date and will be used.", symbol);
                return this;
            }
            else
            {
                _equityMarket = null;
                FinanceService._equityMarketCache.Remove(symbol);
                //_logger.LogDebug("EquityMarket for {Symbol} is outdated and will be refreshed from the database.", symbol);
            }
        }

        _equityMarket = await _modelService.GetEquityMarketBySymbolAsync(symbol);

        // Not in cashe or cache is outdated, fetch from database
        if (_equityMarket != null && TimeUtils.IsUpToDateWithMarket(_equityMarket.LastUpdated))
        {
            FinanceService._equityMarketCache[symbol] = _equityMarket;
            _logger.LogDebug("EquityMarket for {Symbol} resolved from cache.", symbol);
            return this;
        }

        YhGetResultStockSummaryDetails stockPriceInfo = await _yhHttpClient.GetSymbolFullPriceAsync(symbol);
        var equityMarket = stockPriceInfo.PopulateEquityMarket(new EquityMarket { Symbol = symbol });
        _equityMarket = await _modelService.UpsertEquityMarketAsync(equityMarket);
        FinanceService._equityMarketCache[symbol] = _equityMarket;

        return this;
    }

    public async Task<EquityBuilder> WithEquityMarket(Equity equity)
    {
        if (_equityMarket == null || EquityUtils.GetSymbolAdjustedToMarket(equity.Symbol, equity.Market) != _equityMarket.Symbol)
        {
            throw new InvalidOperationException("EquityMarket must be set before setting Equity.");
        }

        equity.LastUpdated = _equityMarket.LastUpdated;
        equity.MarketPrice = _equityMarket.CurrentPrice;

        return this;
    }

    public async Task<EquityBuilder> WithExchangeRate(Equity equity, decimal? exchangeRate = null)
    {
        equity.CurrentPrice = exchangeRate == null ? equity.MarketPrice : equity.MarketPrice * exchangeRate.Value;
      
        return this;
    }




    /// <summary>
    /// Tries to populate the stock price info from the in-memory short- or long-info cache.
    /// </summary>
    //public EquityBuilder WithEquityFromCache(bool isShortInfo = false)
    //{
    //    var cache = isShortInfo
    //        ? FinanceService._shortInfoCache
    //        : FinanceService._longInfoCache;

    //    if (cache.TryGetValue(_equity!.Symbol, out var cached))
    //    {
    //        _stockPriceInfo = cached;
    //        _logger.LogDebug("Stock price info for {Symbol} resolved from {CacheType} cache.", _equity!.Symbol, isShortInfo ? "short" : "long");
    //    }
    //    return this;
    //}

    /// <summary>
    /// Fetches the stock price info from the Yahoo Finance REST API and updates the cache.
    /// </summary>
    //public async Task<EquityBuilder> WithEquityFromRestAPI(bool isShortStockInfo = false)
    //{
    //    YhStockPriceInfo info;

    //    if (isShortStockInfo)
    //        info = new YhStockPriceInfo(await _yhHttpClient.YhGetStockPriceOnlyAsync(_equity!.Symbol));
    //    else
    //        info = new YhStockPriceInfo(await _yhHttpClient.GetSymbolFullPriceAsync(_equity!.Symbol));

    //    if (!string.IsNullOrEmpty(info.Error))
    //    {
    //        _logger.LogWarning("Failed to fetch stock price for {Symbol} from REST API: {Error}", _equity!.Symbol, info.Error);
    //        return this;
    //    }

    //    _stockPriceInfo = info;

    //    var cache = isShortStockInfo
    //        ? FinanceService._shortInfoCache
    //        : FinanceService._longInfoCache;

    //    cache[symbol] = _stockPriceInfo;

    //    return this;
    //}

    /// <summary>
    /// Applies the accumulated stock price info to the <see cref="Equity"/> and, if available,
    /// to the <see cref="EquityMarket"/>. Returns the populated <see cref="Equity"/>, or
    /// <c>null</c> when the required state has not been set.
    /// </summary>
    public async Task Build(Equity equity)
    {
        //if (equity == null)
        //{
        //    throw new InvalidOperationException("Equity must be set before calling Build.");
        //    //_logger.LogWarning("EquityBuilder.Build called without a target Equity.");
        //}

        await _modelService.UpdateEquityAsync(equity);

        //if (_stockPriceInfo == null)
        //{
        //    _logger.LogWarning("EquityBuilder.Build called without stock price info for {Symbol}.", _equity.Symbol);
        //    return _equity;
        //}

        //if (_stockPriceInfo.StockSummaryDetails != null)
        //{
        //    _stockPriceInfo.StockSummaryDetails.PopulateDatabaseEntity(_equity);

        //    if (_equityMarket != null)
        //        _stockPriceInfo.StockSummaryDetails.PopulateEquityMarket(_equityMarket);
        //}
        //else
        //{
        //    _stockPriceInfo.StockPrice?.PopulateDatabaseEntity(_equity);
        //}

        //return _equity;
    }
}

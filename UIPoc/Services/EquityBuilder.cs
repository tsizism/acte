using UIPooc.Models;
using UIPooc.Utils;
using UIPooc.Yahoo;

namespace UIPooc.Services;

public class EquityBuilder
{
    private readonly YhHttpClient _yhHttpClient;
    private readonly IModelService _modelService;
    private readonly ILogger<EquityBuilder> _logger;

    private Equity? _equity;
    private EquityMarket? _equityMarket;

    public EquityBuilder(YhHttpClient yhHttpClient, IModelService modelService)
    {
        _yhHttpClient = yhHttpClient;
        _modelService = modelService;
        _logger = new Logger<EquityBuilder>(new LoggerFactory());
        _equityMarket = null;
    }

    public async Task<EquityBuilder> WithEquity(Equity equity, decimal? exchangeRate = null)
    {
        if(_equityMarket == null)
        {
            throw new InvalidOperationException("EquityMarket must be set before setting Equity.");
        }

        //EquityUtils.UpdateEquityFromMarket(equity, _equityMarket, exchangeRate);

        equity.MarketPrice = _equityMarket.CurrentPrice;
        equity.CurrentPrice = exchangeRate == null ? _equityMarket.CurrentPrice : _equityMarket.CurrentPrice * exchangeRate.Value;

        equity.LastUpdated = _equityMarket.LastUpdated;

        _equity = await _modelService.UpdateEquityAsync(equity);
        return this;
    }

    /// <summary>
    /// Populates <see cref="EquityMarket"/> from the database, falling back to the in-memory cache
    /// when the data is still fresh.
    /// </summary>
    public async Task<EquityBuilder> WithEquityMarket(string symbol) //, string market)
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


                //if(TimeUtils.IsTradingTime() && !TimeUtils.IsEquityMarketCacheExpired(cached.LastUpdated))
                //{
                //    _equityMarket = cached;
                //    _logger.LogDebug("EquityMarket for {Symbol} is up-to-date and will be used.", _equity!.Symbol);
                //    return this;
                //}
                //else
                //{
                //    _logger.LogDebug("EquityMarket for {Symbol} is outdated and will be refreshed from the database.", _equity!.Symbol);
                //}
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


        //if (_equityMarket != null)
        //{
        //    FinanceService._equityMarketCache[_equity!.Symbol] = _equityMarket;
        //}
        //else
        //{
        //    _equityMarket = new EquityMarket { Symbol = _equity!.Symbol };
        //}

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
    public Equity? Build()
    {
        if (_equity == null)
        {
            _logger.LogWarning("EquityBuilder.Build called without a target Equity.");
            return null;
        }

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

        return _equity;
    }
}

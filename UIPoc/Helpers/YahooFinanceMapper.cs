using UIPooc.Models;
using UIPooc.Services;

namespace UIPooc.Helpers;

/// <summary>
/// Maps Yahoo Finance API entities to database models
/// </summary>
public static class YahooFinanceMapper
{
    #region EntityStockPrice to EquityMarket

    /// <summary>
    /// Maps EntityStockPrice (from Yahoo Finance API) to EquityMarket database model
    /// </summary>
    public static EquityMarket ToEquityMarket(this EntityYhFullStockPrice source, string market = "US")
    {
        return new EquityMarket
        {
            Symbol = source.symbol ?? string.Empty,
            Currency = source.currency ?? "USD",
            CurrentPrice = TryParseDecimal(source.regularMarketPrice),
            PreviousClose = TryParseDecimal(source.regularMarketPreviousClose),
            OpenPrice = TryParseDecimal(source.regularMarketOpen),
            DayHigh = TryParseDecimal(source.regularMarketDayHigh),
            DayLow = TryParseDecimal(source.regularMarketDayLow),
            Volume = TryParseLong(source.regularMarketVolume),
            MarketCap = TryParseNullableDecimal(source.marketCap),
            Week52High = null, // Not available in EntityStockPrice
            Week52Low = null, // Not available in EntityStockPrice
            LastUpdated = DateTime.UtcNow,
            LastTradeTime = TryParseDateTime(source.regularMarketTime),

        };
    }

    /// <summary>
    /// Maps EntityStockPrice2 (short format from Yahoo Finance API) to EquityMarket database model
    /// </summary>
    public static EquityMarket ToEquityMarket(this EntityYhStockPrice source, string market = "US")
    {
        return new EquityMarket
        {
            Symbol = source.symbol ?? string.Empty,
            Currency = source.currency ?? "USD",
            CurrentPrice = TryParseDecimal(source.price),
            PreviousClose = 0,
            OpenPrice = 0,
            DayHigh = 0,
            DayLow = 0,
            Volume = 0,
            MarketCap = TryParseNullableDecimal(source.marketCap),
            Week52High = null,
            Week52Low = null,
            LastUpdated = DateTime.UtcNow,
            LastTradeTime = null,
        };
    }

    #endregion

    #region EntityStockPrice to Equity

    /// <summary>
    /// Updates an existing Equity entity with data from EntityStockPrice
    /// </summary>
    public static void UpdateFromYahooPrice(this Equity equity, EntityYhFullStockPrice source)
    {
        equity.CurrentPrice = TryParseDecimal(source.regularMarketPrice);
        equity.Currency = source.currency ?? equity.Currency;

        // Update holding highs and lows
        var currentPrice = equity.CurrentPrice;
        if (currentPrice > equity.HoldingHigh || equity.HoldingHigh == 0)
        {
            equity.HoldingHigh = currentPrice;
            equity.HoldingHighAt = DateTime.UtcNow;
        }

        if (currentPrice < equity.HoldingLow || equity.HoldingLow == 0)
        {
            equity.HoldingLow = currentPrice;
            equity.HoldingLowAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Updates an existing Equity entity with data from EntityStockPrice2
    /// </summary>
    public static void UpdateFromYahooPrice(this Equity equity, EntityYhStockPrice source)
    {
        equity.CurrentPrice = TryParseDecimal(source.price);
        equity.Currency = source.currency ?? equity.Currency;

        // Update holding highs and lows
        var currentPrice = equity.CurrentPrice;
        if (currentPrice > equity.HoldingHigh || equity.HoldingHigh == 0)
        {
            equity.HoldingHigh = currentPrice;
            equity.HoldingHighAt = DateTime.UtcNow;
        }

        if (currentPrice < equity.HoldingLow || equity.HoldingLow == 0)
        {
            equity.HoldingLow = currentPrice;
            equity.HoldingLowAt = DateTime.UtcNow;
        }
    }

    #endregion

    #region EquityMarket to Equity

    /// <summary>
    /// Updates an existing Equity entity with data from EquityMarket cache
    /// </summary>
    public static void UpdateFromEquityMarket(this Equity equity, EquityMarket source)
    {
        equity.CurrentPrice = source.CurrentPrice;
        equity.Currency = source.Currency;
        //equity.CompanyName = source.CompanyName ?? equity.CompanyName;

        // Update holding highs and lows
        var currentPrice = equity.CurrentPrice;
        if (currentPrice > equity.HoldingHigh || equity.HoldingHigh == 0)
        {
            equity.HoldingHigh = currentPrice;
            equity.HoldingHighAt = DateTime.UtcNow;
        }

        if (currentPrice < equity.HoldingLow || equity.HoldingLow == 0)
        {
            equity.HoldingLow = currentPrice;
            equity.HoldingLowAt = DateTime.UtcNow;
        }
    }

    #endregion

    #region Helper Methods

    private static decimal TryParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        return decimal.TryParse(value, out var result) ? result : 0;
    }

    private static decimal? TryParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return decimal.TryParse(value, out var result) ? result : null;
    }

    private static long TryParseLong(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        return long.TryParse(value, out var result) ? result : 0;
    }

    private static DateTime? TryParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Handle ISO 8601 format: "2026-03-07T00:49:29.000Z"
        if (DateTime.TryParse(value, out var result))
            return result.ToUniversalTime();

        // Handle Unix timestamp
        if (long.TryParse(value, out var timestamp))
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;

        return null;
    }

    #endregion

    #region Batch Mapping

    /// <summary>
    /// Maps a collection of EntityStockPrice to EquityMarket models
    /// </summary>
    public static List<EquityMarket> ToEquityMarkets(this IEnumerable<EntityYhFullStockPrice> sources, string market = "US")
    {
        return sources.Select(s => s.ToEquityMarket(market)).ToList();
    }

    /// <summary>
    /// Maps a collection of EntityStockPrice2 to EquityMarket models
    /// </summary>
    public static List<EquityMarket> ToEquityMarkets(this IEnumerable<EntityYhStockPrice> sources, string market = "US")
    {
        return sources.Select(s => s.ToEquityMarket(market)).ToList();
    }

    /// <summary>
    /// Updates a collection of Equity entities from EquityMarket cache data
    /// </summary>
    public static void UpdateFromEquityMarkets(this IEnumerable<Equity> equities, IEnumerable<EquityMarket> marketData)
    {
        var marketDict = marketData.ToDictionary(m => m.Symbol, m => m);

        foreach (var equity in equities)
        {
            if (marketDict.TryGetValue(equity.Symbol, out var market))
            {
                equity.UpdateFromEquityMarket(market);
            }
        }
    }

    #endregion
}

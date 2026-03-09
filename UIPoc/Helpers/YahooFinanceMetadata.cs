using UIPooc.Helpers;

namespace UIPooc.Helpers;

/// <summary>
/// Metadata definitions for mapping Yahoo Finance API responses to database entities
/// These dictionaries define field mappings without depending on EntityYh structs
/// </summary>
public static class YahooFinanceMetadata
{
    /// <summary>
    /// Metadata for Yahoo Finance Short Price endpoint: /yhprice?ticker=AAPL
    /// Response format: { "symbol": "AAPL", "price": 230.13, "currency": "USD", "marketCap": 3498919591936 }
    /// Maps to: EquityMarket database model
    /// </summary>
    public static Dictionary<string, PropertyMetadata> YahooShortPriceToEquityMarket => new()
    {
        ["Symbol"] = new PropertyMetadata
        {
            SourceName = "symbol",
            ColumnName = "Symbol",
            IsRequired = true
        },
        ["CurrentPrice"] = new PropertyMetadata
        {
            SourceName = "price",
            ColumnName = "CurrentPrice",
            IsRequired = true
        },
        ["Currency"] = new PropertyMetadata
        {
            SourceName = "currency",
            ColumnName = "Currency",
            DefaultValue = "USD"
        },
        ["MarketCap"] = new PropertyMetadata
        {
            SourceName = "marketCap",
            ColumnName = "MarketCap"
        }
    };

    /// <summary>
    /// Metadata for Yahoo Finance Full Price endpoint: /price?symbol=AAPL
    /// Response format: { "price": { "symbol": "AAPL", "regularMarketPrice": 230.13, ... } }
    /// This maps the nested "price" object fields to EquityMarket database model
    /// </summary>
    public static Dictionary<string, PropertyMetadata> YahooFullPriceToEquityMarket => new()
    {
        // Required fields
        ["Symbol"] = new PropertyMetadata
        {
            SourceName = "symbol",
            ColumnName = "Symbol",
            IsRequired = true
        },
        ["CurrentPrice"] = new PropertyMetadata
        {
            SourceName = "regularMarketPrice",
            ColumnName = "CurrentPrice",
            IsRequired = true
        },

        // Basic info
        ["Currency"] = new PropertyMetadata
        {
            SourceName = "currency",
            ColumnName = "Currency",
            DefaultValue = "USD"
        },
        ["CompanyName"] = new PropertyMetadata
        {
            SourceName = "shortName",
            ColumnName = "CompanyName"
        },
        ["CompanyNameLong"] = new PropertyMetadata
        {
            SourceName = "longName",
            ColumnName = "CompanyName"
        },
        ["Exchange"] = new PropertyMetadata
        {
            SourceName = "exchange",
            ColumnName = "Exchange"
        },

        // Market data
        ["MarketCap"] = new PropertyMetadata
        {
            SourceName = "marketCap",
            ColumnName = "MarketCap"
        },
        ["OpenPrice"] = new PropertyMetadata
        {
            SourceName = "regularMarketOpen",
            ColumnName = "OpenPrice"
        },
        ["PreviousClose"] = new PropertyMetadata
        {
            SourceName = "regularMarketPreviousClose",
            ColumnName = "PreviousClose"
        },
        ["DayHigh"] = new PropertyMetadata
        {
            SourceName = "regularMarketDayHigh",
            ColumnName = "DayHigh"
        },
        ["DayLow"] = new PropertyMetadata
        {
            SourceName = "regularMarketDayLow",
            ColumnName = "DayLow"
        },
        ["Volume"] = new PropertyMetadata
        {
            SourceName = "regularMarketVolume",
            ColumnName = "Volume"
        },

        // Time fields
        ["LastTradeTime"] = new PropertyMetadata
        {
            SourceName = "regularMarketTime",
            ColumnName = "LastTradeTime",
            Format = "yyyy-MM-ddTHH:mm:ss.fffZ"
        },

        // 52-week data
        ["Week52High"] = new PropertyMetadata
        {
            SourceName = "fiftyTwoWeekHigh",
            ColumnName = "Week52High"
        },
        ["Week52Low"] = new PropertyMetadata
        {
            SourceName = "fiftyTwoWeekLow",
            ColumnName = "Week52Low"
        },

        // Financial ratios
        ["PERatio"] = new PropertyMetadata
        {
            SourceName = "trailingPE",
            ColumnName = "PERatio"
        },
        ["EPS"] = new PropertyMetadata
        {
            SourceName = "epsTrailingTwelveMonths",
            ColumnName = "EPS"
        },
        ["DividendYield"] = new PropertyMetadata
        {
            SourceName = "trailingAnnualDividendYield",
            ColumnName = "DividendYield"
        }
    };

    /// <summary>
    /// Metadata for Yahoo Finance yhf endpoint: /yhf?ticker=AAPL
    /// Response format: { "price": { "symbol": "AAPL", ... }, "summaryDetail": { ... } }
    /// This maps the nested "price" object
    /// </summary>
    public static Dictionary<string, PropertyMetadata> YahooYhfPriceToEquityMarket => new()
    {
        ["Symbol"] = new PropertyMetadata
        {
            SourceName = "symbol",
            ColumnName = "Symbol",
            IsRequired = true
        },
        ["CurrentPrice"] = new PropertyMetadata
        {
            SourceName = "regularMarketPrice",
            ColumnName = "CurrentPrice",
            IsRequired = true
        },
        ["Currency"] = new PropertyMetadata
        {
            SourceName = "currency",
            ColumnName = "Currency",
            DefaultValue = "USD"
        },
        ["CompanyName"] = new PropertyMetadata
        {
            SourceName = "shortName",
            ColumnName = "CompanyName"
        },
        ["Exchange"] = new PropertyMetadata
        {
            SourceName = "exchange",
            ColumnName = "Exchange"
        },
        ["MarketCap"] = new PropertyMetadata
        {
            SourceName = "marketCap",
            ColumnName = "MarketCap"
        },
        ["OpenPrice"] = new PropertyMetadata
        {
            SourceName = "regularMarketOpen",
            ColumnName = "OpenPrice"
        },
        ["PreviousClose"] = new PropertyMetadata
        {
            SourceName = "regularMarketPreviousClose",
            ColumnName = "PreviousClose"
        },
        ["DayHigh"] = new PropertyMetadata
        {
            SourceName = "regularMarketDayHigh",
            ColumnName = "DayHigh"
        },
        ["DayLow"] = new PropertyMetadata
        {
            SourceName = "regularMarketDayLow",
            ColumnName = "DayLow"
        },
        ["Volume"] = new PropertyMetadata
        {
            SourceName = "regularMarketVolume",
            ColumnName = "Volume"
        },
        ["LastTradeTime"] = new PropertyMetadata
        {
            SourceName = "regularMarketTime",
            ColumnName = "LastTradeTime",
            Format = "yyyy-MM-ddTHH:mm:ss.fffZ"
        }
    };

    /// <summary>
    /// Alternative metadata configuration using only essential fields for quick price updates
    /// </summary>
    public static Dictionary<string, PropertyMetadata> YahooEssentialPriceFields => new()
    {
        ["Symbol"] = new PropertyMetadata
        {
            SourceName = "symbol",
            ColumnName = "Symbol",
            IsRequired = true
        },
        ["CurrentPrice"] = new PropertyMetadata
        {
            SourceName = "regularMarketPrice",
            ColumnName = "CurrentPrice",
            IsRequired = true
        },
        ["Currency"] = new PropertyMetadata
        {
            SourceName = "currency",
            ColumnName = "Currency",
            DefaultValue = "USD"
        },
        ["Volume"] = new PropertyMetadata
        {
            SourceName = "regularMarketVolume",
            ColumnName = "Volume"
        },
        ["LastTradeTime"] = new PropertyMetadata
        {
            SourceName = "regularMarketTime",
            ColumnName = "LastTradeTime",
            Format = "yyyy-MM-ddTHH:mm:ss.fffZ"
        }
    };

    /// <summary>
    /// Metadata for mapping Yahoo Finance API data to Equity model (for direct equity updates)
    /// </summary>
    public static Dictionary<string, PropertyMetadata> YahooFullPriceToEquity => new()
    {
        ["Symbol"] = new PropertyMetadata
        {
            SourceName = "symbol",
            ColumnName = "Symbol",
            IsRequired = true
        },
        ["CurrentPrice"] = new PropertyMetadata
        {
            SourceName = "regularMarketPrice",
            ColumnName = "CurrentPrice",
            IsRequired = true
        },
        ["Currency"] = new PropertyMetadata
        {
            SourceName = "currency",
            ColumnName = "Currency",
            DefaultValue = "USD"
        },
        ["CompanyName"] = new PropertyMetadata
        {
            SourceName = "shortName",
            ColumnName = "CompanyName"
        }
    };

    /// <summary>
    /// Metadata for Yahoo Finance Quote endpoint: /v7/finance/quote?symbols=AAPL
    /// Standard Yahoo Finance free API response format
    /// </summary>
    public static Dictionary<string, PropertyMetadata> YahooQuoteToEquityMarket => new()
    {
        ["Symbol"] = new PropertyMetadata
        {
            SourceName = "symbol",
            ColumnName = "Symbol",
            IsRequired = true
        },
        ["CurrentPrice"] = new PropertyMetadata
        {
            SourceName = "regularMarketPrice",
            ColumnName = "CurrentPrice",
            IsRequired = true
        },
        ["Currency"] = new PropertyMetadata
        {
            SourceName = "currency",
            ColumnName = "Currency",
            DefaultValue = "USD"
        },
        ["CompanyName"] = new PropertyMetadata
        {
            SourceName = "longName",
            ColumnName = "CompanyName"
        },
        ["CompanyNameShort"] = new PropertyMetadata
        {
            SourceName = "shortName",
            ColumnName = "CompanyName"
        },
        ["Exchange"] = new PropertyMetadata
        {
            SourceName = "exchange",
            ColumnName = "Exchange"
        },
        ["MarketCap"] = new PropertyMetadata
        {
            SourceName = "marketCap",
            ColumnName = "MarketCap"
        },
        ["OpenPrice"] = new PropertyMetadata
        {
            SourceName = "regularMarketOpen",
            ColumnName = "OpenPrice"
        },
        ["PreviousClose"] = new PropertyMetadata
        {
            SourceName = "regularMarketPreviousClose",
            ColumnName = "PreviousClose"
        },
        ["DayHigh"] = new PropertyMetadata
        {
            SourceName = "regularMarketDayHigh",
            ColumnName = "DayHigh"
        },
        ["DayLow"] = new PropertyMetadata
        {
            SourceName = "regularMarketDayLow",
            ColumnName = "DayLow"
        },
        ["Volume"] = new PropertyMetadata
        {
            SourceName = "regularMarketVolume",
            ColumnName = "Volume"
        },
        ["Week52High"] = new PropertyMetadata
        {
            SourceName = "fiftyTwoWeekHigh",
            ColumnName = "Week52High"
        },
        ["Week52Low"] = new PropertyMetadata
        {
            SourceName = "fiftyTwoWeekLow",
            ColumnName = "Week52Low"
        },
        ["PERatio"] = new PropertyMetadata
        {
            SourceName = "trailingPE",
            ColumnName = "PERatio"
        },
        ["DividendYield"] = new PropertyMetadata
        {
            SourceName = "trailingAnnualDividendYield",
            ColumnName = "DividendYield"
        },
        ["EPS"] = new PropertyMetadata
        {
            SourceName = "epsTrailingTwelveMonths",
            ColumnName = "EPS"
        },
        ["LastTradeTime"] = new PropertyMetadata
        {
            SourceName = "regularMarketTime",
            ColumnName = "LastTradeTime"
        }
    };
}

using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using UIPooc.Attributes;
using UIPooc.Helpers;
using UIPooc.Models;


namespace UIPooc.Services;

[DbEntity(TableName = "EquityMarket", Source = "YahooFinance", Description = "Short format stock price from Yahoo Finance API")]
public struct EntityYhStockPrice
{
    [DbProperty(SourceName = "symbol", ColumnName = "Symbol", IsRequired = true, Description = "Stock ticker symbol")]
    public string symbol { get; set; }

    [DbProperty(SourceName = "price", ColumnName = "CurrentPrice", IsRequired = true, DbType = "decimal(18,2)", Description = "Current stock price")]
    public string price { get; set; }

    [DbProperty(SourceName = "currency", ColumnName = "Currency", IsRequired = true, DefaultValue = "USD", Description = "Currency code")]
    public string currency { get; set; }

    [DbProperty(SourceName = "marketCap", ColumnName = "MarketCap", DbType = "decimal(18,2)", Description = "Market capitalization")]
    public string marketCap { get; set; }
}

[DbEntity(TableName = "EquityMarket", Source = "YahooFinance", Description = "Full format stock price from Yahoo Finance API")]
public struct EntityYhFullStockPrice
{
    [DbProperty(SourceName = "currency", ColumnName = "Currency", IsRequired = true, DefaultValue = "USD")]
    public string currency { get; set; }

    [DbProperty(SourceName = "currencySymbol", Description = "Currency symbol like $, €, etc.")]
    public string currencySymbol { get; set; }

    [DbProperty(SourceName = "exchange", ColumnName = "Exchange", Description = "Stock exchange code")]
    public string exchange { get; set; }

    [DbProperty(SourceName = "exchangeDataDelayedBy", Description = "Data delay in minutes")]
    public decimal exchangeDataDelayedBy { get; set; }

    [DbProperty(SourceName = "exchangeName", Description = "Full exchange name")]
    public string exchangeName { get; set; }

    [DbProperty(SourceName = "fromCurrency", Ignore = true)]
    public string fromCurrency { get; set; }

    [DbProperty(SourceName = "lastMarket", Ignore = true)]
    public string lastMarket { get; set; }

    [DbProperty(SourceName = "longName", ColumnName = "CompanyName", Description = "Full company name")]
    public string longName { get; set; }

    [DbProperty(SourceName = "marketCap", ColumnName = "MarketCap", DbType = "decimal(18,2)")]
    public string marketCap { get; set; }

    [DbProperty(SourceName = "marketState", Description = "Market state: OPEN, CLOSED, PRE, POST")]
    public string marketState { get; set; }

    [DbProperty(SourceName = "maxAge", Ignore = true)]
    public string maxAge { get; set; }

    [DbProperty(SourceName = "postMarketChange", Description = "After-hours price change")]
    public string postMarketChange { get; set; }

    [DbProperty(SourceName = "postMarketChangePercent", Description = "After-hours change percentage")]
    public string postMarketChangePercent { get; set; }

    [DbProperty(SourceName = "postMarketPrice", Description = "After-hours price")]
    public string postMarketPrice { get; set; }

    [DbProperty(SourceName = "price", ColumnName = "CurrentPrice", IsRequired = true, DbType = "decimal(18,2)")]
    public string price { get; set; }

    [DbProperty(SourceName = "postMarketSource", Ignore = true)]
    public string postMarketSource { get; set; }

    [DbProperty(SourceName = "postMarketTime", Format = "yyyy-MM-ddTHH:mm:ss.fffZ")]
    public string postMarketTime { get; set; }

    [DbProperty(SourceName = "preMarketSource", Ignore = true)]
    public string preMarketSource { get; set; }

    [DbProperty(SourceName = "priceHint", Ignore = true)]
    public string priceHint { get; set; }

    [DbProperty(SourceName = "quoteSourceName", Description = "Data source name")]
    public string quoteSourceName { get; set; }

    [DbProperty(SourceName = "quoteType", Description = "Type: EQUITY, ETF, INDEX, etc.")]
    public string quoteType { get; set; }

    [DbProperty(SourceName = "regularMarketChange", Description = "Regular hours price change")]
    public string regularMarketChange { get; set; }

    [DbProperty(SourceName = "regularMarketChangePercent", Description = "Regular hours change percentage")]
    public string regularMarketChangePercent { get; set; }

    [DbProperty(SourceName = "regularMarketDayHigh", ColumnName = "DayHigh", DbType = "decimal(18,2)")]
    public string regularMarketDayHigh { get; set; }

    [DbProperty(SourceName = "regularMarketDayLow", ColumnName = "DayLow", DbType = "decimal(18,2)")]
    public string regularMarketDayLow { get; set; }

    [DbProperty(SourceName = "regularMarketOpen", ColumnName = "OpenPrice", DbType = "decimal(18,2)")]
    public string regularMarketOpen { get; set; }

    [DbProperty(SourceName = "regularMarketPreviousClose", ColumnName = "PreviousClose", DbType = "decimal(18,2)")]
    public string regularMarketPreviousClose { get; set; }

    [DbProperty(SourceName = "regularMarketPrice", ColumnName = "CurrentPrice", IsRequired = true, DbType = "decimal(18,2)")]
    public string regularMarketPrice { get; set; }

    [DbProperty(SourceName = "regularMarketSource", Ignore = true)]
    public string regularMarketSource { get; set; }

    [DbProperty(SourceName = "regularMarketTime", ColumnName = "LastTradeTime", Format = "yyyy-MM-ddTHH:mm:ss.fffZ")]
    public string regularMarketTime { get; set; }

    [DbProperty(SourceName = "regularMarketVolume", ColumnName = "Volume", DbType = "bigint")]
    public string regularMarketVolume { get; set; }

    [DbProperty(SourceName = "shortName", ColumnName = "CompanyName", Description = "Short company name")]
    public string shortName { get; set; }

    [DbProperty(SourceName = "symbol", ColumnName = "Symbol", IsRequired = true, IsPrimaryKey = true)]
    public string symbol { get; set; }

    [DbProperty(SourceName = "toCurrency", Ignore = true)]
    public string toCurrency { get; set; }

    [DbProperty(SourceName = "underlyingSymbol", Ignore = true)]
    public string underlyingSymbol { get; set; }
}



public class FinanceService : IFinanceService
{
    private readonly HttpClient _httpClient;
    private readonly IModelService _modelService;
    private readonly ILogger<FinanceService> _logger;
    private const string YahooFinanceBaseUrl = "https://query1.finance.yahoo.com/v8/finance";
    private const string YahooFinanceQuoteUrl = "https://query1.finance.yahoo.com/v7/finance/quote";
    private const string YahooFinanceChartUrl = "https://query1.finance.yahoo.com/v8/finance/chart";
    private const string YahooFinanceSearchUrl = "https://query1.finance.yahoo.com/v1/finance/search";

    public FinanceService(HttpClient httpClient, IModelService modelService, ILogger<FinanceService> logger)
    {
        _httpClient = httpClient;
        _modelService = modelService;
        _logger = logger;
            
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    #region Quote Operations

    public async Task<EquityMarket?> GetStockFullInformationAsync(string symbol, string market = "US")
    {
        EquityMarket dbEquityMarket = new EquityMarket() { Market = market };

        Dictionary<string, object> dict = await YahooHttpClient.GetStockFullInformationAsync(symbol);

        EquityMarket equityMarket = DbEntityMapper.PopulateFromDictionary(dbEquityMarket, dict!, YahooFinanceMetadata.YahooFullPriceToEquityMarket);

        if (equityMarket.Symbol != symbol)
        {
            _logger.LogWarning($"Symbol mismatch: expected {symbol}, got {equityMarket.Symbol}");
            return null;
        }

        return dbEquityMarket;
    }

    public async Task<EquityMarket?> GetMarketSummaryAsync(string symbol, string market = "US")
    {
        (Dictionary<string, object> priceDict, Dictionary<string, object> summaryDetailDict) = await YahooHttpClient.GetStockSummaryDetailAsync(symbol);

        Dictionary<string, PropertyMetadata> metadata = YahooFinanceMetadata.YahooFullPriceToEquityMarket;

        EquityMarket dbEquityMarket = new EquityMarket() { Market = market }; 

        EquityMarket equityMarket = DbEntityMapper.PopulateFromDictionary(dbEquityMarket, priceDict!, YahooFinanceMetadata.YahooFullPriceToEquityMarket);
        equityMarket = DbEntityMapper.PopulateFromDictionary(dbEquityMarket, summaryDetailDict!, YahooFinanceMetadata.YahooFullPriceToEquityMarket);

        if (equityMarket.Symbol != symbol)
        {             
            _logger.LogWarning($"Symbol mismatch: expected {symbol}, got {equityMarket.Symbol}");
            return null;
        }

        return dbEquityMarket;


        //try
        //{
        //    var url = $"{YahooFinanceQuoteUrl}?symbols={symbol}";
        //    var response = await _httpClient.GetAsync(url);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        _logger.LogWarning($"Failed to fetch market summary for {symbol}. Status: {response.StatusCode}");
        //        return null;
        //    }

        //    var content = await response.Content.ReadAsStringAsync();
        //    var jsonDoc = JsonDocument.Parse(content);

        //    var result = jsonDoc.RootElement
        //        .GetProperty("quoteResponse")
        //        .GetProperty("result");

        //    if (result.GetArrayLength() == 0)
        //        return null;

        //    var quote = result[0];

        //    return new MarketSummary
        //    {
        //        Symbol = GetStringValue(quote, "symbol"),
        //        ShortName = GetStringValue(quote, "shortName"),
        //        LongName = GetStringValue(quote, "longName"),
        //        RegularMarketPrice = GetDecimalValue(quote, "regularMarketPrice"),
        //        RegularMarketChange = GetDecimalValue(quote, "regularMarketChange"),
        //        RegularMarketChangePercent = GetDecimalValue(quote, "regularMarketChangePercent"),
        //        RegularMarketVolume = GetLongValue(quote, "regularMarketVolume"),
        //        MarketCap = GetNullableDecimalValue(quote, "marketCap"),
        //        FiftyTwoWeekHigh = GetNullableDecimalValue(quote, "fiftyTwoWeekHigh"),
        //        FiftyTwoWeekLow = GetNullableDecimalValue(quote, "fiftyTwoWeekLow"),
        //        TrailingPE = GetNullableDecimalValue(quote, "trailingPE"),
        //        DividendYield = GetNullableDecimalValue(quote, "trailingAnnualDividendYield"),
        //        Currency = GetStringValue(quote, "currency"),
        //        Exchange = GetStringValue(quote, "exchange")
        //    };
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, $"Error fetching market summary for {symbol}");
        //    return null;
        //}
    }


    public async Task<EquityMarket?> GetQuoteAndCacheAsync(string symbol, string market = "US")
    {
        //var quote = await GetMarketSummaryAsync(symbol, market);
        EquityMarket? quote = await GetStockFullInformationAsync(symbol, market);
        if (quote != null)
        {
            await _modelService.UpsertEquityMarketAsync(quote);
        }
        return quote;
    }

    public async Task<EquityMarket?> GetQuoteAsync(string symbol, string market = "US")
    {
        // Full stock price endpoint: https://yh-finance-complete.p.rapidapi.com/price?ticker=AAPL
        EntityYhFullStockPrice entityStockPrice = new EntityYhFullStockPrice();
        await YahooHttpClient.GetSymbolFullPriceAsync(symbol, entityStockPrice);

        // Use mapper to convert Yahoo API entity to database model
        
        
        return entityStockPrice.ToEquityMarket(market);


        //try
        //{
        //    var url = $"{YahooFinanceQuoteUrl}?symbols={symbol}";
        //    var response = await _httpClient.GetAsync(url);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        _logger.LogWarning($"Failed to fetch quote for {symbol}. Status: {response.StatusCode}");
        //        return null;
        //    }

        //    var content = await response.Content.ReadAsStringAsync();
        //    var jsonDoc = JsonDocument.Parse(content);

        //    var result = jsonDoc.RootElement
        //        .GetProperty("quoteResponse")
        //        .GetProperty("result");

        //    if (result.GetArrayLength() == 0)
        //        return null;

        //    var quote = result[0];

        //    return MapToEquityMarket(quote, market);
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, $"Error fetching quote for {symbol}");
        //    return null;
        //}
    }

    //public static void PopulateStockTickerProps(string jsonResponse, StockTickerProperties stockTickerProps)
    //{
    //    Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

    //    if (dict != null)
    //    {
    //        ApiAdapterHttpClient.PopulateEntityFromDict(stockTickerProps, dict);
    //        Console.WriteLine(stockTickerProps.ToString());
    //    }
    //}

    public async Task<List<EquityMarket>> GetQuotesAsync(List<string> symbols, string market = "US")
    {
        var results = new List<EquityMarket>();

        try
        {
            string symbolsString = string.Join(",", symbols);
            string url = $"{YahooFinanceQuoteUrl}?symbols={symbolsString}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch quotes. Status: {response.StatusCode}");
                return results;
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            var quotesArray = jsonDoc.RootElement
                .GetProperty("quoteResponse")
                .GetProperty("result");

            foreach (var quote in quotesArray.EnumerateArray())
            {
                var equityMarket = MapToEquityMarket(quote, market);
                if (equityMarket != null)
                    results.Add(equityMarket);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching multiple quotes");
        }

        return results;
    }


    public async Task<List<EquityMarket>> GetQuotesAndCacheAsync(List<string> symbols, string market = "US")
    {
        var quotes = await GetQuotesAsync(symbols, market);
            
        foreach (var quote in quotes)
        {
            try
            {
                await _modelService.UpsertEquityMarketAsync(quote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error caching quote for {quote.Symbol}");
            }
        }

        return quotes;
    }

    #endregion

    #region Historical Data

    public async Task<List<StockHistoricalData>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate, string market = "US")
    {
        var results = new List<StockHistoricalData>();

        try
        {
            var period1 = new DateTimeOffset(startDate).ToUnixTimeSeconds();
            var period2 = new DateTimeOffset(endDate).ToUnixTimeSeconds();
                
            var url = $"{YahooFinanceChartUrl}/{symbol}?period1={period1}&period2={period2}&interval=1d";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch historical data for {symbol}. Status: {response.StatusCode}");
                return results;
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            var chart = jsonDoc.RootElement
                .GetProperty("chart")
                .GetProperty("result")[0];

            var timestamps = chart.GetProperty("timestamp");
            var quotes = chart.GetProperty("indicators").GetProperty("quote")[0];
            var adjClose = chart.GetProperty("indicators").GetProperty("adjclose")[0].GetProperty("adjclose");

            int index = 0;
            foreach (var timestamp in timestamps.EnumerateArray())
            {
                var date = DateTimeOffset.FromUnixTimeSeconds(timestamp.GetInt64()).DateTime;
                    
                var historicalData = new StockHistoricalData
                {
                    Date = date,
                    Open = GetDecimalFromArray(quotes, "open", index),
                    High = GetDecimalFromArray(quotes, "high", index),
                    Low = GetDecimalFromArray(quotes, "low", index),
                    Close = GetDecimalFromArray(quotes, "close", index),
                    AdjustedClose = GetDecimalFromJsonElement(adjClose[index]),
                    Volume = GetLongFromArray(quotes, "volume", index)
                };

                results.Add(historicalData);
                index++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching historical data for {symbol}");
        }

        return results;
    }

    public async Task<List<StockHistoricalData>> GetHistoricalDataAsync(string symbol, string period = "1mo", string interval = "1d", string market = "US")
    {
        var results = new List<StockHistoricalData>();

        try
        {
            var url = $"{YahooFinanceChartUrl}/{symbol}?range={period}&interval={interval}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch historical data for {symbol}. Status: {response.StatusCode}");
                return results;
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            var chart = jsonDoc.RootElement
                .GetProperty("chart")
                .GetProperty("result")[0];

            var timestamps = chart.GetProperty("timestamp");
            var quotes = chart.GetProperty("indicators").GetProperty("quote")[0];
                
            int index = 0;
            foreach (var timestamp in timestamps.EnumerateArray())
            {
                var date = DateTimeOffset.FromUnixTimeSeconds(timestamp.GetInt64()).DateTime;
                    
                var historicalData = new StockHistoricalData
                {
                    Date = date,
                    Open = GetDecimalFromArray(quotes, "open", index),
                    High = GetDecimalFromArray(quotes, "high", index),
                    Low = GetDecimalFromArray(quotes, "low", index),
                    Close = GetDecimalFromArray(quotes, "close", index),
                    AdjustedClose = GetDecimalFromArray(quotes, "close", index),
                    Volume = GetLongFromArray(quotes, "volume", index)
                };

                results.Add(historicalData);
                index++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching historical data for {symbol}");
        }

        return results;
    }

    #endregion

    #region Market Summary


    #endregion

    #region Batch Operations

    public async Task<decimal> RequestTickerPriceAsync(string ticker)
    {
        return await YahooHttpClient.GetTickerPriceAsync(ticker);
    }

    public async Task EtlEquityPricesAsync(int holdingId)
    {
        try
        {
            var equities = await _modelService.GetEquitiesByHoldingIdAsync(holdingId);
            var symbols = equities.Select(e => e.Symbol).Distinct().ToList();

            if (symbols.Count == 0)
                return;

            List<EquityMarket> quotes = await GetQuotesAndCacheAsync(symbols);

            foreach (var equity in equities)
            {
                var quote = quotes.FirstOrDefault(q => q.Symbol == equity.Symbol);
                if (quote != null)
                {
                    equity.CurrentPrice = quote.CurrentPrice;
                        
                    // Update highs and lows if necessary
                    if (quote.CurrentPrice > equity.HoldingHigh)
                    {
                        equity.HoldingHigh = quote.CurrentPrice;
                        equity.HoldingHighAt = DateTime.UtcNow;
                    }
                        
                    if (quote.CurrentPrice < equity.HoldingLow || equity.HoldingLow == 0)
                    {
                        equity.HoldingLow = quote.CurrentPrice;
                        equity.HoldingLowAt = DateTime.UtcNow;
                    }

                    await _modelService.UpdateEquityAsync(equity);
                }
            }

            _logger.LogInformation($"Updated prices for {equities.Count} equities in holding {holdingId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating equity prices for holding {holdingId}");
            throw;
        }
    }

    public async Task UpdateAllEquityPricesAsync()
    {
        try
        {
            var holdings = await _modelService.GetAllHoldingsAsync();
                
            foreach (var holding in holdings)
            {
                await EtlEquityPricesAsync(holding.HoldingId);
            }

            _logger.LogInformation($"Updated prices for all holdings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating all equity prices");
            throw;
        }
    }

    public async Task<bool> RefreshMarketCacheAsync(string symbol, string market)
    {
        try
        {
            var quote = await GetQuoteAsync(symbol, market);
            if (quote != null)
            {
                await _modelService.UpsertEquityMarketAsync(quote);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error refreshing market cache for {symbol}");
            return false;
        }
    }

    #endregion

    #region Search


    public async Task<List<EquitySearchResult>> SearchSymbolsAsync(string query)
    {
        var results = new List<EquitySearchResult>();

        try
        {
            var url = $"{YahooFinanceSearchUrl}?q={Uri.EscapeDataString(query)}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to search for {query}. Status: {response.StatusCode}");
                return results;
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            var quotes = jsonDoc.RootElement.GetProperty("quotes");

            foreach (var quote in quotes.EnumerateArray())
            {
                results.Add(new EquitySearchResult
                {
                    Symbol = GetStringValue(quote, "symbol"),
                    Name = GetStringValue(quote, "longname") ?? GetStringValue(quote, "shortname"),
                    Exchange = GetStringValue(quote, "exchange"),
                    Type = GetStringValue(quote, "quoteType")
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching for {query}");
        }

        return results;
    }

    #endregion

    #region Helper Methods

    private EquityMarket MapToEquityMarket(JsonElement quote, string market)
    {
        return new EquityMarket
        {
            Symbol = GetStringValue(quote, "symbol"),
            Market = market,
            CompanyName = GetStringValue(quote, "longName") ?? GetStringValue(quote, "shortName"),
            Currency = GetStringValue(quote, "currency") ?? "USD",
            CurrentPrice = GetDecimalValue(quote, "regularMarketPrice"),
            PreviousClose = GetDecimalValue(quote, "regularMarketPreviousClose"),
            OpenPrice = GetDecimalValue(quote, "regularMarketOpen"),
            DayHigh = GetDecimalValue(quote, "regularMarketDayHigh"),
            DayLow = GetDecimalValue(quote, "regularMarketDayLow"),
            Volume = GetLongValue(quote, "regularMarketVolume"),
            MarketCap = GetNullableDecimalValue(quote, "marketCap"),
            Week52High = GetNullableDecimalValue(quote, "fiftyTwoWeekHigh"),
            Week52Low = GetNullableDecimalValue(quote, "fiftyTwoWeekLow"),
            PERatio = GetNullableDecimalValue(quote, "trailingPE"),
            DividendYield = GetNullableDecimalValue(quote, "trailingAnnualDividendYield"),
            EPS = GetNullableDecimalValue(quote, "epsTrailingTwelveMonths"),
            LastUpdated = DateTime.UtcNow,
            LastTradeTime = GetNullableDateTimeValue(quote, "regularMarketTime"),
            Exchange = GetStringValue(quote, "exchange")
        };
    }

    private string GetStringValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString() ?? string.Empty;
        return string.Empty;
    }

    private decimal GetDecimalValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number)
                return prop.GetDecimal();
            if (prop.ValueKind == JsonValueKind.String && decimal.TryParse(prop.GetString(), out var value))
                return value;
        }
        return 0;
    }

    private decimal? GetNullableDecimalValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number)
                return prop.GetDecimal();
            if (prop.ValueKind == JsonValueKind.String && decimal.TryParse(prop.GetString(), out var value))
                return value;
        }
        return null;
    }

    private long GetLongValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number)
                return prop.GetInt64();
            if (prop.ValueKind == JsonValueKind.String && long.TryParse(prop.GetString(), out var value))
                return value;
        }
        return 0;
    }

    private DateTime? GetNullableDateTimeValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number)
        {
            var timestamp = prop.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }
        return null;
    }

    private decimal GetDecimalFromArray(JsonElement element, string propertyName, int index)
    {
        if (element.TryGetProperty(propertyName, out var array) && array.ValueKind == JsonValueKind.Array)
        {
            var items = array.EnumerateArray().ToList();
            if (index < items.Count)
            {
                var item = items[index];
                if (item.ValueKind == JsonValueKind.Number)
                    return item.GetDecimal();
            }
        }
        return 0;
    }

    private decimal GetDecimalFromJsonElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number)
            return element.GetDecimal();
        if (element.ValueKind == JsonValueKind.String && decimal.TryParse(element.GetString(), out var value))
            return value;
        return 0;
    }

    private long GetLongFromArray(JsonElement element, string propertyName, int index)
    {
        if (element.TryGetProperty(propertyName, out var array) && array.ValueKind == JsonValueKind.Array)
        {
            var items = array.EnumerateArray().ToList();
            if (index < items.Count)
            {
                var item = items[index];
                if (item.ValueKind == JsonValueKind.Number)
                    return item.GetInt64();
            }
        }
        return 0;
    }

    

    #endregion
}

class FinanceServiceTest
{
    public static void TestTickerRestSharp()
    {
        //var client = new RestClient("https://yh-finance-complete.p.rapidapi.com/news?ticker=AAPL");
        string ticker = "AAPL";
        string urlYhComplete = $"https://yh-finance-complete.p.rapidapi.com/yhprice?ticker={ticker}";
        //RestClient client = new RestClient(urlYhComplete);
        //RestRequest request = new RestRequest(); // Method.Get);
        //request.AddHeader("x-rapidapi-key", "9b405718ddmsh954d4191ebcf658p148c17jsn58521162b938");
        //request.AddHeader("x-rapidapi-host", "yh-finance-complete.p.rapidapi.com");
        //request.AddHeader("content-type", "application/json");

        //object? queryResult = client.Execute<Object>(request).Data;

        ////dynamic jsonResponse = JsonConvert.DeserializeObject(restResponse.Content);
        //RestResponse response = client.Execute(request);
        //Console.WriteLine("Rest response:");
        //Console.WriteLine(response.Content);

        //if (response.Content != null)
        //{
        //    //Dictionary<string, object>? values = JsonSerializer.Deserialize<Dictionary<string, object>>(json: response.Content);
        //    //Console.WriteLine(values.ToString());
        //    StockTickerProperties stockTickerProps = StockTickerProperties.CreateFromJson(jsonResponse: response.Content);
        //}



        //dynamic jsonResponse = JsonConvert.DeserializeObject(restResponse.Content);

        //string json = JsonConvert.SerializeObject(queryResult);
        //RestResponse response = client.Execute(request);


        //request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

        //var queryResult = client.Execute(request);

        //Console.WriteLine(queryResult.Content);

    }


    // curl --request GET --url https://yh-finance-complete.p.rapidapi.com/insights  --header 'x-rapidapi-host: yh-finance-complete.p.rapidapi.com' --header 'x-rapidapi-key: 9b405718ddmsh954d4191ebcf658p148c17jsn58521162b98'

    // curl --request GET  --url "https://yh-finance-complete.p.rapidapi.com/insights?symbol=AAPL&reportsCount=1" --header "x-rapidapi-host: yh-finance-complete.p.rapidapi.com" --header "x-rapidapi-key: 9b405718ddmsh954d4191ebcf658p148c17jsn58521162b938"

    // C:\Users\mtsizis>
    // curl --request GET  --url "https://yh-finance-complete.p.rapidapi.com/insights?symbol=AAPL&reportsCount=1" --header "x-rapidapi-host: yh-finance-complete.p.rapidapi.com" --header "x-rapidapi-key: 9b405718ddmsh954d4191ebcf658p148c17jsn58521162b938"
}
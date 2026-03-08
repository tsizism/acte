using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using UIPooc.Models;

// Poprtal API for Yahoo Finance data, including stock quotes, historical data, and market insights.
// https://rapidapi.com/belchiorarkad-FqvHs2EDOtP/api/yh-finance-complete

// https://rapidapi.com/belchiorarkad-FqvHs2EDOtP/api/yh-finance-complete/playground/apiendpoint_e40c1e4d-f29b-4041-b947-18bb42f3458b
// curl --request GET --url https://yh-finance-complete.p.rapidapi.com/insights  --header 'x-rapidapi-host: yh-finance-complete.p.rapidapi.com' --header 'x-rapidapi-key: 9b405718ddmsh954d4191ebcf658p148c17jsn58521162b98'
// curl --request GET  --url "https://yh-finance-complete.p.rapidapi.com/insights?symbol=AAPL&reportsCount=1" --header "x-rapidapi-host: yh-finance-complete.p.rapidapi.com" --header "x-rapidapi-key: 9b405718ddmsh954d4191ebcf658p148c17jsn58521162b938"
// curl --request GET  --url "https://yh-finance-complete.p.rapidapi.com/insights?symbol=AAPL&reportsCount=1" --header "x-rapidapi-host: yh-finance-complete.p.rapidapi.com" --header "x-rapidapi-key: 9b405718ddmsh954d4191ebcf658p148c17jsn58521162b938"
/*
 +	[0]	{[symbol, ValueKind = String : "AAPL"]}	System.Collections.Generic.KeyValuePair<string, object>
+		[1]	{[price, ValueKind = Number : "230.13"]}	System.Collections.Generic.KeyValuePair<string, object>
+		[2]	{[currency, ValueKind = String : "USD"]}	System.Collections.Generic.KeyValuePair<string, object>
+		[3]	{[symbolName, ValueKind = String : "Apple"]}	System.Collections.Generic.KeyValuePair<string, object>
+		[4]	{[marketCap, ValueKind = Number : "3498919591936"]}	System.Collections.Generic.KeyValuePair<string, object>
* 
 * 
*/


namespace UIPooc.Services;

public struct EntityStockPrice2
{
    public string symbol { get; set; }
    public string price { get; set; }
    public string currency { get; set; }
    public string marketCap { get; set; }

    //public EntityStockPrice()
    //{
    //    symbol = String.Empty;
    //    price = String.Empty;
    //    currency = String.Empty;
    //    marketCap = String.Empty;

    //}
}

public struct EntityStockPrice
{
    public string currency { get; set; }     //      "USD"
    public string currencySymbol { get; set; }     //      "$"
    public string exchange { get; set; }     //      "NYQ"
    public decimal exchangeDataDelayedBy { get; set; }     //      0
    public string exchangeName { get; set; }     //      "NYSE"
    public string fromCurrency { get; set; }     //      null
    public string lastMarket { get; set; }     //      null
    public string longName { get; set; }     //      "Agilent Technologies, Inc."
    public string marketCap { get; set; }     //      32546357248
    public string marketState { get; set; }     //      "CLOSED"
    public string maxAge { get; set; }     //      1
    public string postMarketChange { get; set; }     //      0.9299011
    public string postMarketChangePercent { get; set; }     //      0.008081177
    public string postMarketPrice { get; set; }     //      115.9999
    public string price { get; set; }     //      115.9999
    public string postMarketSource { get; set; }     //      "FREE_REALTIME"
    public string postMarketTime { get; set; }     //      "2026-03-07T00  { get; set; }     //      49  { get; set; }     //      29.000Z"
    public string preMarketSource { get; set; }     //      "DELAYED"
    public string priceHint { get; set; }     //      2
    public string quoteSourceName { get; set; }     //      "Nasdaq Real Time Price"
    public string quoteType { get; set; }     //      "EQUITY"
    public string regularMarketChange { get; set; }     //      -3.04
    public string regularMarketChangePercent { get; set; }     //      -0.0257387
    public string regularMarketDayHigh { get; set; }     //      116.74
    public string regularMarketDayLow { get; set; }     //      114.92
    public string regularMarketOpen { get; set; }     //      116.66
    public string regularMarketPreviousClose { get; set; }     //      118.11
    public string regularMarketPrice { get; set; }     //      115.07
    public string regularMarketSource { get; set; }     //      "FREE_REALTIME"
    public string regularMarketTime { get; set; }     //      "2026-03-06T21  { get; set; }     //      00  { get; set; }     //      03.000Z"
    public string regularMarketVolume { get; set; }     //      2507276
    public string shortName { get; set; }     //      "Agilent Technologies, Inc."
    public string symbol { get; set; }     //      "A"
    public string toCurrency { get; set; }     //      null
    public string underlyingSymbol { get; set; }     //      null
}






public class YahooHttpClient
{
    //public static async Task<Dictionary<string, object>?> Get(string url)

    /// <summary>
    /// Short stock price endpoint: https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL
    /// </summary>
    /// <param name="ticker"></param>
    /// <param name="stockTickerProps"></param>
    /// <returns></returns>
    public static async Task GetTickerPriceAsync(string ticker, EntityStockPrice stockTickerProps)
    {
        // Full stock price endpoint:  https://yh-finance-complete.p.rapidapi.com/price?ticker=AAPL
        // Short stock price endpoint: https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL
        //string urlYhComplete =       $"https://yh-finance-complete.p.rapidapi.com/price?symbol={ticker}";

        var url = $"https://yh-finance-complete.p.rapidapi.com/yhprice?ticker={ticker}";
        
        //Dictionary<string, object>? dict = await Get(url
        string jsonResponse = await Get(url);

        //string ticker = @"{""symbol"": ""AAPL"", 
        //                    ""price"": 230.4584, 
        //                    ""currency"": ""USD"",
        //                    ""symbolName"": ""Apple"",
        //                    ""marketCap"": 3503912648704
        //                    }";

        PopulateStockTicker(jsonResponse, stockTickerProps);

    }


    /// <summary>
    /// Full Price
    /// </summary>
    /// <param name="ticker"></param>
    /// <param name="stockTickerProps"></param>
    /// <returns></returns>
    public static async Task GetSymbolFullPriceAsync(string symbol, EntityStockPrice stockTickerProps)
    {
        // Full stock price endpoint:  https://yh-finance-complete.p.rapidapi.com/price?ticker=AAPL
        // Short stock price endpoint: https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL
        string urlYhComplete = $"https://yh-finance-complete.p.rapidapi.com/price?symbol={symbol}";

        //Dictionary<string, object>? dict = await Get(url
        string jsonResponse = await Get(urlYhComplete);

        //string ticker = @"{""symbol"": ""AAPL"", 
        //                    ""price"": 230.4584, 
        //                    ""currency"": ""USD"",
        //                    ""symbolName"": ""Apple"",
        //                    ""marketCap"": 3503912648704
        //                    }";

        PopulateEntityStockPrice(jsonResponse, stockTickerProps);

    }

    public static T CreateFromJson<T>(string jsonResponse) where T : new()
    {
        Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);
        T entity = new T();

        if (dict != null)
        {
            YahooHttpClient.PopulateEntityFromDict(entity, dict);
            Console.WriteLine(entity.ToString());
        }

        return entity;
    }


    static public void PopulateEntityFromDict<T>(T props, Dictionary<string, object> dict)
    {
        Type t = props!.GetType();

        try
        {
            foreach (var keyValuePair in dict)
            {
                PropertyInfo? property = t.GetProperty(keyValuePair.Key, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanWrite)
                {
                    //property.SetValue(this, Convert.ChangeType(keyValuePair.Value, property.PropertyType), null);
                    property.SetValue(props, keyValuePair.Value.ToString(), null);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error populating entity from dict: {ex.Message}");
        }
        }


    public static void PopulateStockTicker(string jsonResponse, EntityStockPrice stockTicker)
    {
        Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

        if (dict != null)
        {
            YahooHttpClient.PopulateEntityFromDict(stockTicker, dict);
            Console.WriteLine(stockTicker.ToString());
        }
    }

    public static void PopulateEntityStockPrice(string jsonResponse, EntityStockPrice stockTicker)
    {
        Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

        if (dict != null)
        {
            object tmp = dict["price"];
            //Dictionary<string, object>? priceDict = tmp as Dictionary<string, object>;
            Dictionary<string, object>? priceDict = JsonSerializer.Deserialize<Dictionary<string, object>>(tmp.ToString());
            YahooHttpClient.PopulateEntityFromDict(stockTicker, priceDict!);
            Console.WriteLine(stockTicker.ToString());
        }
    }


    //RequestUri = new Uri("https://yh-finance-complete.p.rapidapi.com/news?ticker=AAPL"),
    // RequestUri = new Uri("https://yh-finance-complete.p.rapidapi.com/insights?symbol=AAPL"),
    //RequestUri = new Uri("https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL"),


    static public async Task<string> Get(string url)
    {
        //  Secret Manager
        string token = File.ReadAllText("cfg.user");

        HttpClient client = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url),



            Headers =
                {
                    { "x-rapidapi-key", token },
                    { "x-rapidapi-host", "yh-finance-complete.p.rapidapi.com" },
                },
        };

        string bodyJson = string.Empty;
        using (HttpResponseMessage response = await client.SendAsync(request))
        {
            //HttpResponseMessage result = response.EnsureSuccessStatusCode();
            bodyJson = await response.Content.ReadAsStringAsync();
            //Console.WriteLine("Http response:");
            //values = JsonSerializer.Deserialize<Dictionary<string, object>>(body);


            //if (values != null)
            //{
            //    //StockTicker stockTicker = new StockTicker(values);
            //    //Console.WriteLine(stockTicker.ToString());
            //}

            //Console.WriteLine(body);

        }
        return bodyJson;
    }

    //public async Task TaskTestTickerHttpClientAsync()
    //{
    //    string ticker = "AAPL";
    //    StockTickerProperties stockTickerProps = new StockTickerProperties();
    //    await GetTickerAsync(ticker, stockTickerProps);
    //}
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

    //public static void PopulateStockTickerProps(string jsonResponse, StockTickerProperties stockTickerProps)
    //{
    //    Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

    //    if (dict != null)
    //    {
    //        ApiAdapterHttpClient.PopulateEntityFromDict(stockTickerProps, dict);
    //        Console.WriteLine(stockTickerProps.ToString());
    //    }
    //}

    public async Task<EquityMarket?> GetQuoteAsync(string ticker, string market = "US")
    {   // Short stock price endpoint: https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL
        //string urlYhComplete = $"https://yh-finance-complete.p.rapidapi.com/yhprice?ticker={ticker}";
        //string urlYhComplete = $"https://yh-finance-complete.p.rapidapi.com/price?ticker={ticker}";
        // Full stock price endpoint:  https://yh-finance-complete.p.rapidapi.com/price?ticker=AAPL
        string url = $"https://yh-finance-complete.p.rapidapi.com/price?ticker={ticker}";

        EntityStockPrice stockTickerProps = new EntityStockPrice();
        //await YahooHttpClient.GetTickerPriceAsync(ticker, stockTickerProps);

        await YahooHttpClient.GetSymbolFullPriceAsync(ticker, stockTickerProps);

        //StockTickerProperties stockTickerProps = new StockTickerProperties();
        //PopulateStockTickerProps(jsonResponse, stockTickerProps);

        return new EquityMarket
        {
            Symbol = stockTickerProps.symbol,
            Market = market,
            CompanyName = stockTickerProps.symbol, // Placeholder, as yhprice endpoint doesn't return company name
            Currency = stockTickerProps.currency,
            CurrentPrice = decimal.TryParse(stockTickerProps.price, out var price) ? price : 0,
            MarketCap = decimal.TryParse(stockTickerProps.marketCap, out var marketCap) ? marketCap : (decimal?)null,
            LastUpdated = DateTime.UtcNow

        public int EquityMarketId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Market { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal PreviousClose { get; set; }
    public decimal OpenPrice { get; set; }
    public decimal DayHigh { get; set; }
    public decimal DayLow { get; set; }
    public long Volume { get; set; }
    public decimal? MarketCap { get; set; }
    public decimal? Week52High { get; set; }
    public decimal? Week52Low { get; set; }
    public decimal? PERatio { get; set; }
    public decimal? DividendYield { get; set; }
    public decimal? EPS { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public DateTime? LastTradeTime { get; set; }
    public string? Exchange { get; set; }

};

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

    public async Task<EquityMarket?> GetQuoteAndCacheAsync(string symbol, string market = "US")
    {
        var quote = await GetQuoteAsync(symbol, market);
        if (quote != null)
        {
            await _modelService.UpsertEquityMarketAsync(quote);
        }
        return quote;
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

    public async Task<MarketSummary?> GetMarketSummaryAsync(string symbol, string market = "US")
    {
        try
        {
            var url = $"{YahooFinanceQuoteUrl}?symbols={symbol}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch market summary for {symbol}. Status: {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            var result = jsonDoc.RootElement
                .GetProperty("quoteResponse")
                .GetProperty("result");

            if (result.GetArrayLength() == 0)
                return null;

            var quote = result[0];

            return new MarketSummary
            {
                Symbol = GetStringValue(quote, "symbol"),
                ShortName = GetStringValue(quote, "shortName"),
                LongName = GetStringValue(quote, "longName"),
                RegularMarketPrice = GetDecimalValue(quote, "regularMarketPrice"),
                RegularMarketChange = GetDecimalValue(quote, "regularMarketChange"),
                RegularMarketChangePercent = GetDecimalValue(quote, "regularMarketChangePercent"),
                RegularMarketVolume = GetLongValue(quote, "regularMarketVolume"),
                MarketCap = GetNullableDecimalValue(quote, "marketCap"),
                FiftyTwoWeekHigh = GetNullableDecimalValue(quote, "fiftyTwoWeekHigh"),
                FiftyTwoWeekLow = GetNullableDecimalValue(quote, "fiftyTwoWeekLow"),
                TrailingPE = GetNullableDecimalValue(quote, "trailingPE"),
                DividendYield = GetNullableDecimalValue(quote, "trailingAnnualDividendYield"),
                Currency = GetStringValue(quote, "currency"),
                Exchange = GetStringValue(quote, "exchange")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching market summary for {symbol}");
            return null;
        }
    }

    #endregion

    #region Batch Operations

    public async Task UpdateEquityPricesAsync(int holdingId)
    {
        try
        {
            var equities = await _modelService.GetEquitiesByHoldingIdAsync(holdingId);
            var symbols = equities.Select(e => e.Symbol).Distinct().ToList();

            if (symbols.Count == 0)
                return;

            var quotes = await GetQuotesAndCacheAsync(symbols);

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
                await UpdateEquityPricesAsync(holding.HoldingId);
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
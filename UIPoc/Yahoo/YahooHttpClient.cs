using Microsoft.VisualBasic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using UIPooc.Attributes;
using UIPooc.Helpers;
using UIPooc.Models;
using UIPooc.Services;
using UIPooc.Utils;


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


namespace UIPooc.Yahoo;

// DTO for stock ticker price information retrieved from Yahoo Finance API
// Stock Price - "https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=bce"), 4 keys (symbol, price, currency, marketCap)
public class TickerPriceEntity
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal MarketCap { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Error { get; set; } = string.Empty;

    // TickerPriceEntity to Equity equity
    public void PopulateDatabaseEquity(Equity equity)
    {
        var symbol = EquityUtils.GetSymbolAdjustedToMarket(equity);

        if (symbol != this.Symbol)
        {
            throw new InvalidOperationException("FullStockPriceEntityPrice.ToEquity: Symbol mismatch.");
        }

        equity.Currency = this.Currency;
        equity.MarketPrice = this.Price;
        equity.CurrentPrice = this.Price;
        equity.HoldingHigh = equity.CurrentPrice;
        equity.HoldingHighAt = this.LastUpdated;
        equity.HoldingLow = equity.CurrentPrice;
        equity.HoldingLowAt = this.LastUpdated;

        equity.AverageCost = equity.AverageCost == 0 ? equity.CurrentPrice : equity.AverageCost;
        equity.Quantity = equity.Quantity == 0 ? 1 : equity.Quantity;
    }
}

// DTO for full stock price information retrieved from Yahoo Finance API
// Financials-Full Stock Price - "https://yh-finance-complete.p.rapidapi.com/price?symbol=bce" -38 keys
//                               "https://yh-finance-complete.p.rapidapi.com/price?symbol=bce.to"),  - 28 keys

// $"https://yh-finance-complete.p.rapidapi.com/price?symbol={symbol}";
public class FullStockPriceEntity
{
    public FullStockPriceEntityPrice? Price { get; set; }
    public DateTime LastUpdated { get; set; }

    public Equity ToDatabaseEquity(Equity equity)
    {
        if (Price == null)
        {
            throw new InvalidOperationException("FullStockPriceEntity.ToDatabaseEquity: Price is null.");
        }

        equity.AverageCost = equity.AverageCost == 0 ? Price.RegularMarketPrice : equity.AverageCost;
        equity.MarketPrice = Price.RegularMarketPrice;
        equity.CurrentPrice = Price.RegularMarketPrice;


        if (Price.RegularMarketPrice > equity.HoldingHigh)
        {
            equity.HoldingHigh = Price.RegularMarketPrice;
            equity.HoldingHighAt = this.LastUpdated;
        }

        if (Price.RegularMarketPrice < equity.HoldingLow)
        {
            equity.HoldingLow = Price.RegularMarketPrice;
            equity.HoldingLowAt = this.LastUpdated;
        }

        return equity;
    }

    internal void ToDatabaseEquityMarket(EquityMarket equityMarket)
    {
        equityMarket.Currency = Price!.Currency;
        equityMarket.CurrentPrice = Price.RegularMarketPrice;
        equityMarket.PreviousClose = Price.RegularMarketPreviousClose;
        equityMarket.OpenPrice = Price.RegularMarketOpen;
        equityMarket.DayHigh = Price.RegularMarketDayHigh;
        equityMarket.DayLow = Price.RegularMarketDayLow;
        equityMarket.Volume = Price.RegularMarketVolume;
        equityMarket.MarketCap = Price.MarketCap;
        equityMarket.Week52High = 0;
        equityMarket.Week52Low = 0;
        equityMarket.LastTradeTime = Price.RegularMarketTime;
        equityMarket.LastUpdated = DateTime.UtcNow;
    }
}
public class FullStockPriceEntityPrice
{
    public int MaxAge { get; set; }//maxAge:1
    public decimal RegularMarketChangePercent { get; set; } //regularMarketChangePercent:-0.009003931
    public decimal RegularMarketChange { get; set; } //regularMarketChange:-0.3199997
    public DateTime RegularMarketTime { get; set; } //regularMarketTime:"2026-03-25T20:00:00.000Z"
    public int PriceHint { get; set; } //priceHint:2
    public decimal RegularMarketPrice { get; set; } //regularMarketPrice:35.22
    public decimal RegularMarketDayHigh { get; set; } //regularMarketDayHigh:35.74
    public decimal RegularMarketDayLow { get; set; } //regularMarketDayLow:35.21
    public int RegularMarketVolume { get; set; } //regularMarketVolume:4902321
    public decimal RegularMarketPreviousClose { get; set; } //regularMarketPreviousClose:35.54
    public string RegularMarketSource { get; set; } = string.Empty;//regularMarketSource:"FREE_REALTIME"
    public decimal RegularMarketOpen { get; set; } //regularMarketOpen:35.67
    public string Exchange { get; set; } = string.Empty;//exchange:"TOR"
    public string ExchangeName { get; set; } = string.Empty;//exchangeName:"Toronto"
    public int ExchangeDataDelayedBy { get; set; } //exchangeDataDelayedBy:15
    public string MarketState { get; set; } = string.Empty;//marketState:"POSTPOST"
    public string QuoteType { get; set; } = string.Empty; //quoteType:"EQUITY"
    public string Symbol { get; set; } = string.Empty;  //symbol:"BCE.TO"
    public string ShortName { get; set; } = string.Empty; //shortName:"BCE INC."
    public string LongName { get; set; } = string.Empty; //longName:"BCE Inc."
    public string Currency { get; set; } = string.Empty;//currency:"CAD"
    public string QuoteSourceName { get; set; } = string.Empty;//quoteSourceName:"Delayed Quote"
    public string CurrencySymbol { get; set; } = string.Empty;//currencySymbol:"$"
    public string? FromCurrency { get; set; } //fromCurrency:null
    public string? ToCurrency { get; set; }//toCurrency:null
    public string? LastMarket { get; set; }//lastMarket:null
    public long MarketCap { get; set; } //marketCap:32843560960
}


public class YahooHttpClient
{
    //public static async Task<Dictionary<string, object>?> Get(string url)

    public static async Task<TickerPriceEntity> GetYhTickerPriceAsync(string ticker)
    {
        // Full stock price endpoint:  https://yh-finance-complete.p.rapidapi.com/price?ticker=AAPL
        // Short stock price endpoint: https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL
        //string urlYhComplete =       $"https://yh-finance-complete.p.rapidapi.com/price?symbol={ticker}";


        var url = $"https://yh-finance-complete.p.rapidapi.com/yhprice?ticker={ticker}";
        // full include 52weeks
        // https://yh-finance-complete.p.rapidapi.com/fullData?ticker=bce"),


        //Dictionary<string, object>? dict = await Get(url
        string jsonResponse = await Get(url);

        if (jsonResponse == null)
        {
            return new TickerPriceEntity() { Error = "GetYhTickerPriceAsync: Get returned empty string" };
        }

        if (jsonResponse.Contains("error"))
        {
            Dictionary<string, object>? dict1 = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse );

            if (dict1 == null)
            {
                return new TickerPriceEntity() { Error = "GetYhTickerPriceAsync: Failed to parse JSON response" };
            }

            string errorMessage = dict1.ContainsKey("error") ? dict1["error"].ToString() ?? "Unknown error" : "Undefined error";    

            return new TickerPriceEntity() { Error = errorMessage };
        }

        //string ticker = @"{""symbol"": ""AAPL"", 
        //                    ""price"": 230.4584, 
        //                    ""currency"": ""USD"",
        //                    ""symbolName"": ""Apple"",
        //                    ""marketCap"": 3503912648704
        //                    }";

        TickerPriceEntity? result = JsonSerializer.Deserialize<TickerPriceEntity>(jsonResponse, new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

        if (result == null)
        {
            result=  new TickerPriceEntity();
        }
        result.LastUpdated = DateTime.UtcNow;
        return result;
    }

    public static async Task<FullStockPriceEntity> GetYhFullStockPrice(string symbol)
    {
        // Full stock price endpoint:  https://yh-finance-complete.p.rapidapi.com/price?ticker=AAPL
        // Short stock price endpoint: https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL
        //string urlYhComplete =       $"https://yh-finance-complete.p.rapidapi.com/price?symbol={ticker}";


        var url = $"https://yh-finance-complete.p.rapidapi.com/price?symbol={symbol}";


        //Dictionary<string, object>? dict = await Get(url
        string jsonResponse = await Get(url);

        //string ticker = @"{""symbol"": ""AAPL"", 
        //                    ""price"": 230.4584, 
        //                    ""currency"": ""USD"",
        //                    ""symbolName"": ""Apple"",
        //                    ""marketCap"": 3503912648704
        //                    }";

        FullStockPriceEntity? result = JsonSerializer.Deserialize<FullStockPriceEntity>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result == null)
        {
            result = new FullStockPriceEntity();
        }
        result.LastUpdated = DateTime.UtcNow;
        return result;
    }
    // Stock Price - "https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=bce"), 4 keys (symbol, price, currency, marketCap)
    // Stock Full Information - "https://yh-finance-complete.p.rapidapi.com/fullData?ticker=bce"), 85 keys
    // Stock Full Information - "https://yh-finance-complete.p.rapidapi.com/fullData?ticker=bce.to"), 81 keys
    // Stock Summary Detail - https://yh-finance-complete.p.rapidapi.com/yhf?ticker=bce, 40 + 45 keys (price + summaryDetail)
    // Financials-Full Stock Price - "https://yh-finance-complete.p.rapidapi.com/price?symbol=bce" -38 keys
    //                               "https://yh-finance-complete.p.rapidapi.com/price?symbol=bce.to"),  - 28 keys

    /// <summary>
    /// Short stock price endpoint: https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL
    /// </summary>
    /// <param name="ticker"></param>
    /// <param name="stockTickerProps"></param>
    /// <returns></returns>
    public static async Task GetTickerPriceInfoAsync(string ticker, TickerPriceEntity entityYhPrice)
    {
        // Full stock price endpoint:  https://yh-finance-complete.p.rapidapi.com/price?ticker=AAPL
        // Short stock price endpoint: https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL
        //string urlYhComplete =       $"https://yh-finance-complete.p.rapidapi.com/price?symbol={ticker}";


        var url = $"https://yh-finance-complete.p.rapidapi.com/yhprice?ticker={ticker}";
        // full
        // https://yh-finance-complete.p.rapidapi.com/fullData?ticker=bce"),


        //Dictionary<string, object>? dict = await Get(url
        string jsonResponse = await Get(url);

        //string ticker = @"{""symbol"": ""AAPL"", 
        //                    ""price"": 230.4584, 
        //                    ""currency"": ""USD"",
        //                    ""symbolName"": ""Apple"",
        //                    ""marketCap"": 3503912648704
        //                    }";

        FromJson(jsonResponse, entityYhPrice);
    }

    public static bool FromJson(string json, TickerPriceEntity entityYhPrice)
    {
        Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        if (dict == null)
        {
            return false;
        }

        foreach (FieldInfo field in typeof(TickerPriceEntity).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (dict.TryGetValue(field.Name, out var value))
            {
                field.SetValue(entityYhPrice, value?.ToString() ?? string.Empty);
            }
        }
        return true;
    }


    public static async Task<Dictionary<string, object>> GetStockFullInformationAsync(string symbol)
    {
        // Full stock price endpoint:  https://yh-finance-complete.p.rapidapi.com/price?ticker=AAPL
        // Short stock price endpoint: https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL
        //string urlYhComplete = $"https://yh-finance-complete.p.rapidapi.com/price?symbol={symbol}";
        //string url = $"https://yh-finance-complete.p.rapidapi.com/yhf?ticker={symbol}";
        string url = $"https://yh-finance-complete.p.rapidapi.com/fullData?ticker={symbol}";

        //Dictionary<string, object>? dict = await Get(url
        string jsonResponse = await Get(url);

        //string ticker = @"{""symbol"": ""AAPL"", 
        //                    ""price"": 230.4584, 
        //                    ""currency"": ""USD"",
        //                    ""symbolName"": ""Apple"",
        //                    ""marketCap"": 3503912648704
        //                    }";

        //PopulateEntityStockPrice(jsonResponse, stockTickerProps);

        Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);


        if (dict == null)
        {
            return new Dictionary<string, object>() { { "Error", "Failed to deserialize JSON response" } };
        }

        //object pricesJson = dict["price"];
        ////Dictionary<string, object>? priceDict = tmp as Dictionary<string, object>;
        //Dictionary<string, object> priceDict = JsonSerializer.Deserialize<Dictionary<string, object>>(pricesJson.ToString());

        //Dictionary<string, PropertyMetadata> metadata = YahooFinanceMetadata.YahooFullPriceToEquityMarket;

        //EquityMarket dbEquityMarket = new EquityMarket();

        //EquityMarket equityMarket = DbEntityMapper.PopulateFromDictionary(dbEquityMarket, priceDict!, metadata);

        //EntityMapper.PopulateFromDictionary(stockTickerProps, priceDict!);

        //object summaryDetailJson = dict["summaryDetail"];
        //Dictionary<string, object> summaryDetailDict = JsonSerializer.Deserialize<Dictionary<string, object>>(summaryDetailJson.ToString());

        ///YahooHttpClient.PopulateEntityFromDict(stockTicker, priceDict!);
        //Console.WriteLine(stockTicker.ToString());

        return dict;
    }


    public static async Task<(Dictionary<string, object>, Dictionary<string, object>)> GetStockSummaryDetailAsync(string symbol)
    {
        // Full stock price endpoint:  https://yh-finance-complete.p.rapidapi.com/price?ticker=AAPL
        // Short stock price endpoint: https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL
        //string urlYhComplete = $"https://yh-finance-complete.p.rapidapi.com/price?symbol={symbol}";
        string url = $"https://yh-finance-complete.p.rapidapi.com/yhf?ticker={symbol}";

        //Dictionary<string, object>? dict = await Get(url
        string jsonResponse = await Get(url);

        //string ticker = @"{""symbol"": ""AAPL"", 
        //                    ""price"": 230.4584, 
        //                    ""currency"": ""USD"",
        //                    ""symbolName"": ""Apple"",
        //                    ""marketCap"": 3503912648704
        //                    }";

        Dictionary<string, object>? stockSummaryDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

        if (stockSummaryDict == null)
        {
            throw new InvalidOperationException("Failed to deserialize JSON response");
        }

        if (!stockSummaryDict.TryGetValue("price", out object? pricesJson) || pricesJson == null)
        {
            throw new InvalidOperationException("'Price' key not found in JSON response");
        }

        string nestedPriceJson = pricesJson.ToString() ?? throw new InvalidOperationException("Price value is null in JSON response");

        Dictionary<string, object>? priceDict = JsonSerializer.Deserialize<Dictionary<string, object>>(nestedPriceJson!);

        if (priceDict == null)
        {
            throw new InvalidOperationException("Failed to deserialize nested price JSON");
        }

        if ( !stockSummaryDict!.TryGetValue("summaryDetail", out var summaryDetailJson) || summaryDetailJson == null)
        {
            throw new InvalidOperationException("'SummaryDetail' key not found in price JSON response");
        }

        var summaryDetailJsonTxt = summaryDetailJson.ToString() ?? throw new InvalidOperationException("SummaryDetail value is null in JSON response");

        Dictionary<string, object>? summaryDetailDict = JsonSerializer.Deserialize<Dictionary<string, object>>(summaryDetailJsonTxt!);

        return (priceDict!, summaryDetailDict!);
    }


    public static async Task GetSymbolFullPriceAsync(string symbol, EntityYhFullStockPrice stockTickerProps)
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

        PopulateEntityStockPriceFromJson(jsonResponse, stockTickerProps);

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


    public static void PopulateStockTicker(string jsonResponse, EntityYhFullStockPrice stockTicker)
    {
        Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

        if (dict != null)
        {
            YahooHttpClient.PopulateEntityFromDict(stockTicker, dict);
            Console.WriteLine(stockTicker.ToString());
        }
    }

    public static void PopulateEntityStockPriceFromJson(string jsonResponse, EntityYhFullStockPrice stockTicker)
    {
        Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

        if (dict != null)
        {
            if( !dict.TryGetValue("price", out var tmp) || tmp == null)
            {
                throw new InvalidOperationException("'Price' key not found in JSON response");    
            }

            string priceJson = tmp.ToString() ?? throw new InvalidOperationException("Price value is null in JSON response");

            Dictionary<string, object>? priceDict = JsonSerializer.Deserialize<Dictionary<string, object>>(priceJson);
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



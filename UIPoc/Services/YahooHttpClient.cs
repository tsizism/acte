using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using UIPooc.Attributes;
using UIPooc.Helpers;
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


public class EntityYhPrice
{
    public string symbol { get; set; } = string.Empty;
    public decimal price { get; set; }
    public string currency { get; set; } = string.Empty;
    public decimal marketCap { get; set; }
}


public class YahooHttpClient
{
    //public static async Task<Dictionary<string, object>?> Get(string url)

    public static async Task<decimal> GetTickerPriceAsync(string ticker)
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

        EntityYhPrice? entityYhPrice = JsonSerializer.Deserialize<EntityYhPrice>(jsonResponse);

        if (entityYhPrice == null)
        {
            return 0;
        }

        return entityYhPrice!.price;
    }


    /// <summary>
    /// Short stock price endpoint: https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL
    /// </summary>
    /// <param name="ticker"></param>
    /// <param name="stockTickerProps"></param>
    /// <returns></returns>
    public static async Task GetTickerPriceInfoAsync(string ticker, EntityYhPrice entityYhPrice)
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

    public static bool FromJson(string json, EntityYhPrice entityYhPrice)
    {
        Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        if (dict == null)
        {
            return false;
        }

        foreach (FieldInfo field in typeof(EntityYhPrice).GetFields(BindingFlags.Public | BindingFlags.Instance))
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

        //PopulateEntityStockPrice(jsonResponse, stockTickerProps);

        Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);


        if (dict == null)
        {
            return (new Dictionary<string, object>() { { "Error", "Failed to deserialize JSON response" } }, new Dictionary<string, object>());
        }

        object pricesJson = dict["price"];
        //Dictionary<string, object>? priceDict = tmp as Dictionary<string, object>;
        Dictionary<string, object> priceDict = JsonSerializer.Deserialize<Dictionary<string, object>>(pricesJson.ToString());

        //Dictionary<string, PropertyMetadata> metadata = YahooFinanceMetadata.YahooFullPriceToEquityMarket;

        //EquityMarket dbEquityMarket = new EquityMarket();

        //EquityMarket equityMarket = DbEntityMapper.PopulateFromDictionary(dbEquityMarket, priceDict!, metadata);

        //EntityMapper.PopulateFromDictionary(stockTickerProps, priceDict!);

        object summaryDetailJson = dict["summaryDetail"];
        Dictionary<string, object> summaryDetailDict = JsonSerializer.Deserialize<Dictionary<string, object>>(summaryDetailJson.ToString());

        ///YahooHttpClient.PopulateEntityFromDict(stockTicker, priceDict!);
        //Console.WriteLine(stockTicker.ToString());

        return (priceDict, summaryDetailDict);
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


    public static void PopulateStockTicker(string jsonResponse, EntityYhFullStockPrice stockTicker)
    {
        Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

        if (dict != null)
        {
            YahooHttpClient.PopulateEntityFromDict(stockTicker, dict);
            Console.WriteLine(stockTicker.ToString());
        }
    }

    public static void PopulateEntityStockPrice(string jsonResponse, EntityYhFullStockPrice stockTicker)
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



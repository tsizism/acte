using System.Reflection;
using System.Text.Json;
using UIPooc.Services;
using System.Net.Http.Headers;


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

// Stock Price - "https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=bce"), 4 keys (symbol, price, currency, marketCap)
// Stock Full Information - "https://yh-finance-complete.p.rapidapi.com/fullData?ticker=bce"), 85 keys
// Stock Full Information - "https://yh-finance-complete.p.rapidapi.com/fullData?ticker=bce.to"), 81 keys
// Stock Summary Detail - https://yh-finance-complete.p.rapidapi.com/yhf?ticker=bce, 40 + 45 keys (price + summaryDetail)
// Financials-Full Stock Price - "https://yh-finance-complete.p.rapidapi.com/price?symbol=bce" -38 keys
//                               "https://yh-finance-complete.p.rapidapi.com/price?symbol=bce.to"),  - 28 keys


namespace UIPooc.Yahoo;

public class YhFinanceApiSettings
{
    public const string SectionName = "YhFinanceApi";
    public string ApiKey { get; set; } = string.Empty;
    public string ApiHost { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}


// DTO for stock ticker price information retrieved from Yahoo Finance API
// Stock Price - "https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=bce"), 4 keys (symbol, price, currency, marketCap)
public class YhHttpClient
{
    private readonly HttpClient _httpClient;
    private static string? _httpToken;
    private static readonly object _tokenLock = new object();

    /// <summary>
    /// Gets the HTTP token from file, cached after first read
    /// </summary>
    public static string HttpToken
    {
        get
        {
            if (_httpToken == null)
            {
                lock (_tokenLock)
                {
                    if (_httpToken == null)
                    {
                        _httpToken = File.ReadAllText("cfg.user");
                    }
                }
            }
            return _httpToken;
        }
    }


    public YhHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://yh-finance-complete.p.rapidapi.com");
        _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", HttpToken);
        _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "yh-finance-complete.p.rapidapi.com");


        //var yhFinanceApiSettings = builder.Configuration.GetSection(YhFinanceApiSettings.SectionName).Get<YhFinanceApiSettings>();

        //if (yhFinanceApiSettings is null || string.IsNullOrEmpty(yhFinanceApiSettings.ApiKey))
        //{
        //    throw new InvalidOperationException("YhFinanceApi settings are missing or incomplete in configuration.");
        //}

        //builder.Services.AddHttpClient<YhHttpClient>();
        //    (client =>
        //{
        //    client.BaseAddress = new Uri(yhFinanceApiSettings.BaseUrl);
        //    client.DefaultRequestHeaders.Add("x-rapidapi-key", yhFinanceApiSettings.ApiKey);
        //    client.DefaultRequestHeaders.Add("x-rapidapi-host", yhFinanceApiSettings.ApiHost);
        //});

    }

    private async Task<string> HttpGet(string url)
    {


        //HttpClient client = new HttpClient();
        //HttpRequestMessage request = new HttpRequestMessage
        //{
        //    Method = HttpMethod.Get,
        //    RequestUri = new Uri(url),

        //    Headers =
        //        {
        //            { "x-rapidapi-key", token },
        //            { "x-rapidapi-host", "yh-finance-complete.p.rapidapi.com" },
        //        },
        //};

        string bodyJson = string.Empty;
        using (HttpResponseMessage response = await _httpClient.GetAsync(url))
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



    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// https://rapidapi.com/belchiorarkad-FqvHs2EDOtP/api/yh-finance-complete
    /// GET Stock Price
    /// RequestUri = new Uri("https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=bce"),
    /// S(mall) Size Result
    /// </summary>
    /// <param name="ticker"></param>
    /// <returns></returns>
    public async Task<YhStockPriceResult> YhGetStockPriceAsync(string ticker)
    {
        //var url = $"https://yh-finance-complete.p.rapidapi.com/yhprice?ticker={ticker}";
        var url = $"yhprice?ticker={ticker}";

        string jsonResponse = await HttpGet(url);

        if (jsonResponse == null)
        {
            return new YhStockPriceResult() { Error = "YhGetStockPriceAsync: Get returned empty string" };
        }

        if (jsonResponse.Contains("error"))
        {
            Dictionary<string, object>? dict1 = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse );

            if (dict1 == null)
            {
                return new YhStockPriceResult() { Error = "YhGetStockPriceAsync: Failed to parse JSON response" };
            }

            string errorMessage = dict1.ContainsKey("error") ? dict1["error"].ToString() ?? "Unknown error" : "Undefined error";    

            return new YhStockPriceResult() { Error = errorMessage };
        }

        YhStockPriceResult? result = JsonSerializer.Deserialize<YhStockPriceResult>(jsonResponse, new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

        // { "symbol":"BCE","price":25.11,"currency":"USD","marketCap":23415724032}

        if (result == null)
        {
            result=  new YhStockPriceResult();
        }
        result.LastUpdated = DateTime.UtcNow;
        return result;
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////


    /// <summary>
    /// https://rapidapi.com/belchiorarkad-FqvHs2EDOtP/api/yh-finance-complete
    /// GET Full Stock Price
    /// RequestUri = new Uri("https://yh-finance-complete.p.rapidapi.com/price?symbol=bce"),
    /// M(edium) Size Result - Price  more than 34 keys, 
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>

    public async Task<YhGetFullStockPriceResult> YhGetFullStockPrice(string symbol)
    {
        var url = $"https://yh-finance-complete.p.rapidapi.com/price?symbol={symbol}";

        string jsonResponse = await HttpGet(url);

        YhGetFullStockPriceResult? result = JsonSerializer.Deserialize<YhGetFullStockPriceResult>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result == null)
        {
            result = new YhGetFullStockPriceResult();
        }
        result.LastUpdated = DateTime.UtcNow;
        return result;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //// Deprecated methods below - consider removing or refactoring to use the new YhGetStockPriceAsync and YhGetFullStockPrice methods instead      ///
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// https://rapidapi.com/belchiorarkad-FqvHs2EDOtP/api/yh-finance-complete
    /// GET Stock Price
    /// RequestUri = new Uri("https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=bce"),
    /// <summary>
    /// <param name="ticker"></param>
    /// <param name="stockTickerProps"></param>
    /// <returns></returns>
    /*
    public static async Task GetTickerPriceInfoAsyncDepr(string ticker, YhStockPriceResult entityYhPrice)
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
    */

    /*
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
    */


    //RequestUri = new Uri("https://yh-finance-complete.p.rapidapi.com/news?ticker=AAPL"),
    // RequestUri = new Uri("https://yh-finance-complete.p.rapidapi.com/insights?symbol=AAPL"),
    //RequestUri = new Uri("https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=AAPL"),


    /// <summary>
    /// Depr?
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="stockTickerProps"></param>
    /// <returns></returns>
    public async Task GetSymbolFullPriceAsync(string symbol, EntityYhFullStockPrice stockTickerProps)
    {
        string urlYhComplete = $"https://yh-finance-complete.p.rapidapi.com/price?symbol={symbol}";

        //Dictionary<string, object>? dict = await Get(url
        string jsonResponse = await HttpGet(urlYhComplete);

        //string ticker = @"{""symbol"": ""AAPL"", 
        //                    ""price"": 230.4584, 
        //                    ""currency"": ""USD"",
        //                    ""symbolName"": ""Apple"",
        //                    ""marketCap"": 3503912648704
        //                    }";

        PopulateEntityStockPriceFromJson(jsonResponse, stockTickerProps);

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


    public static void PopulateEntityStockPriceFromJson(string jsonResponse, EntityYhFullStockPrice stockTicker)
    {
        Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

        if (dict != null)
        {
            if (!dict.TryGetValue("price", out var tmp) || tmp == null)
            {
                throw new InvalidOperationException("'Price' key not found in JSON response");
            }

            string priceJson = tmp.ToString() ?? throw new InvalidOperationException("Price value is null in JSON response");

            Dictionary<string, object>? priceDict = JsonSerializer.Deserialize<Dictionary<string, object>>(priceJson);
            YhHttpClient.PopulateEntityFromDict(stockTicker, priceDict!);
            Console.WriteLine(stockTicker.ToString());
        }
    }



    /// <summary>
    /// Depr? - Use PopulateEntityStockPriceFromJson instead, which handles nested JSON for 'price' key
    /// </summary>
    /// <param name="jsonResponse"></param>
    /// <param name="stockTicker"></param>
    //public static void PopulateStockTicker(string jsonResponse, EntityYhFullStockPrice stockTicker)
    //{
    //    Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);

    //    if (dict != null)
    //    {
    //        YhHttpClient.PopulateEntityFromDict(stockTicker, dict);
    //        Console.WriteLine(stockTicker.ToString());
    //    }
    //}


    /// <summary>
    /// 
    /// </summary>
    /// <param name="json"></param>
    /// <param name="entityYhPrice"></param>
    /// <returns></returns>
    /*
        public static bool FromJson(string json, YhStockPriceResult entityYhPrice)
        {
            Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if (dict == null)
            {
                return false;
            }

            foreach (FieldInfo field in typeof(YhStockPriceResult).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (dict.TryGetValue(field.Name, out var value))
                {
                    field.SetValue(entityYhPrice, value?.ToString() ?? string.Empty);
                }
            }
            return true;
        }
    */


    //public static T CreateFromJson<T>(string jsonResponse) where T : new()
    //{
    //    Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);
    //    T entity = new T();

    //    if (dict != null)
    //    {
    //        YhHttpClient.PopulateEntityFromDict(entity, dict);
    //        Console.WriteLine(entity.ToString());
    //    }

    //    return entity;
    //}

    ///////////////////////////////////////////////////////
}

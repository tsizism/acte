using UIPooc.Components.Pages;
using UIPooc.Models;
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

// Stock Price - "https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=bce"), 4 keys (symbol, price, currency, marketCap)
// Stock Full Information - "https://yh-finance-complete.p.rapidapi.com/fullData?ticker=bce"), 85 keys
// Stock Full Information - "https://yh-finance-complete.p.rapidapi.com/fullData?ticker=bce.to"), 81 keys
// Stock Summary Detail - https://yh-finance-complete.p.rapidapi.com/yhf?ticker=bce, 40 + 45 keys (price + summaryDetail)
// Financials-Full Stock Price - "https://yh-finance-complete.p.rapidapi.com/price?symbol=bce" -38 keys
//                               "https://yh-finance-complete.p.rapidapi.com/price?symbol=bce.to"),  - 28 keys


namespace UIPooc.Yahoo;

public class YhStockPriceInfo
{
     public bool IsFullStockPriceResult { get; } 

    public YhStockPriceInfo(YhGetResultStockPrice price)
    {
        StockPrice = price;
        IsFullStockPriceResult = false;
    }

    public YhStockPriceInfo(YhGetResultStockDetails priceDetails)
    {
        StockPriceDetails = priceDetails;
        IsFullStockPriceResult = true;
    }

    public YhStockPriceInfo(YhGetResultStockSummaryDetails summaryDetails)
    {
        StockSummaryDetails = summaryDetails;
        IsFullStockPriceResult = true;
    }

    public YhGetResultStockPrice? StockPrice { get; set; }

    public YhGetResultStockDetails? StockPriceDetails { get; set; }

    public YhGetResultStockSummaryDetails? StockSummaryDetails { get; set; }

    public DateTime LastUpdated => IsFullStockPriceResult ? StockPriceDetails?.LastUpdated ?? DateTime.UtcNow : StockPrice?.LastUpdated ?? DateTime.UtcNow;

    public string Error => IsFullStockPriceResult ? StockPriceDetails?.Error ?? string.Empty : StockPrice?.Error ?? string.Empty;


    // DTO for stock ticker price information retrieved from Yahoo Finance API
    // Stock Price - "https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=bce"), 4 keys (symbol, price, currency, marketCap)
    public class YhGetResultStockPrice
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal MarketCap { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Error { get; set; } = string.Empty;

        // TickerPriceEntity to Equity equity
        public void PopulateDatabaseEntity(Equity equity)
        {
            var symbol = EquityUtils.GetSymbolAdjustedToMarket(equity);

            if (symbol != this.Symbol)
            {
                throw new InvalidOperationException("TickerPriceEntity.PopulateDatabaseEquity: Symbol mismatch.");
            }

            equity.Currency = this.Currency;
            equity.MarketPrice = this.Price;
            equity.CurrentPrice = this.Price; // Will be adjusted to currency in FetchEquitiesForHoldingAsync
        }
    }

    // DTO for full stock price information retrieved from Yahoo Finance API
    // Financials-Full Stock Price - "https://yh-finance-complete.p.rapidapi.com/price?symbol=bce" -38 keys
    //                               "https://yh-finance-complete.p.rapidapi.com/price?symbol=bce.to"),  - 28 keys

    // $"https://yh-finance-complete.p.rapidapi.com/price?symbol={symbol}";
    public class YhGetResultStockDetails
    {
        public YhGetResultStockPriceDetails? Price { get; set; }
        public DateTime LastUpdated { get; set; }

        public string Error { get; set; } = string.Empty;

        public YhGetResultStockPrice ExtractResultStockPrice()
        {
            if (Price == null)
            {
                throw new InvalidOperationException("FullStockPriceEntity.GetResultStockPrice: Price is null.");
            }

            return new YhGetResultStockPrice
            {
                Symbol = Price.Symbol,
                Price = Price.RegularMarketPrice,
                Currency = Price.Currency,
                MarketCap = Price.MarketCap,
                LastUpdated = this.LastUpdated,
                Error = this.Error
            };
        }

        public Equity PopulateDatabaseEntity(Equity equity)
        {
            if (Price == null)
            {
                throw new InvalidOperationException("FullStockPriceEntity.PopulateDatabaseEntity: Price is null.");
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

        internal void PopulateEquityMarket(EquityMarket equityMarket)
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

    public class YhGetResultStockSummaryDetails
    {
        public YhGetResultStockPriceDetails? Price { get; set; }

        public YhGetResultStockPriceSummaryDetails? SummaryDetail { get; set; } 
        public DateTime LastUpdated { get; set; }

        public string Error { get; set; } = string.Empty;

        public YhGetResultStockPrice ExtractResultStockPrice()
        {
            if (Price == null)
            {
                throw new InvalidOperationException("FullStockPriceEntity.GetResultStockPrice: Price is null.");
            }

            return new YhGetResultStockPrice
            {
                Symbol = Price.Symbol,
                Price = Price.RegularMarketPrice,
                Currency = Price.Currency,
                MarketCap = Price.MarketCap,
                LastUpdated = this.LastUpdated,
                Error = this.Error
            };
        }

        public Equity PopulateDatabaseEntity(Equity equity)
        {
            if (Price == null)
            {
                throw new InvalidOperationException("FullStockPriceEntity.PopulateDatabaseEntity: Price is null.");
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

        internal void PopulateEquityMarket(EquityMarket equityMarket)
        {
            equityMarket.Currency = Price!.Currency;
            equityMarket.CurrentPrice = Price.RegularMarketPrice;
            equityMarket.PreviousClose = Price.RegularMarketPreviousClose;
            equityMarket.OpenPrice = Price.RegularMarketOpen;
            equityMarket.DayHigh = Price.RegularMarketDayHigh;
            equityMarket.DayLow = Price.RegularMarketDayLow;
            equityMarket.Volume = Price.RegularMarketVolume;
            equityMarket.MarketCap = Price.MarketCap;
            equityMarket.Week52High = SummaryDetail!.FiftyTwoWeekHigh;
            equityMarket.Week52Low = SummaryDetail!.FiftyTwoWeekLow;
            equityMarket.LastTradeTime = Price.RegularMarketTime;
            equityMarket.LastUpdated = DateTime.UtcNow;
        }
    }

    public class YhGetResultStockPriceDetails   // 186-160+1=27 keys , full 41 key
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
        public int AverageDailyVolume10Day { get; set; } //averageDailyVolume10Day:5399520
        public int AverageDailyVolume3Month { get; set; } //averageDailyVolume3Month:4006129
    }

    public class YhGetResultStockPriceSummaryDetails
    {
        public int MaxAge { get; set; } //maxAge: 1
        public int PriceHint { get; set; } //priceHint: 2
        public decimal PreviousClose { get; set; } //previousClose: 18.78
        public decimal Open { get; set; } //open: 18.83
        public decimal DayLow { get; set; } //dayLow: 18.8
        public decimal DayHigh { get; set; } //dayHigh: 19.02
        public decimal RegularMarketPreviousClose { get; set; } //regularMarketPreviousClose: 18.78
        public decimal RegularMarketOpen { get; set; } //regularMarketOpen: 18.83
        public decimal RegularMarketDayLow { get; set; } //regularMarketDayLow: 18.8
        public decimal RegularMarketDayHigh { get; set; } //regularMarketDayHigh: 19.02
        public decimal DividendRate { get; set; } //dividendRate: 1.23
        public decimal DividendYield { get; set; } //dividendYield: 0.0655
        public DateTime ExDividendDate { get; set; } //exDividendDate: 2026-09-15T00:00:00.000Z
        public decimal PayoutRatio { get; set; } //payoutRatio: 0.25959998
        public decimal FiveYearAvgDividendYield { get; set; } //fiveYearAvgDividendYield: 6.93
        public decimal Beta { get; set; } //beta: 0.652
        public decimal TrailingPE { get; set; } //trailingPE: 3.8647542
        public decimal ForwardPE { get; set; } //forwardPE: 10.189675
        public long Volume { get; set; } //volume: 3048472
        public long RegularMarketVolume { get; set; } //regularMarketVolume: 3048472
        public long AverageVolume { get; set; } //averageVolume: 4006129
        public long AverageVolume10Days { get; set; } //averageVolume10days: 5399520
        public long AverageDailyVolume10Day { get; set; } //averageDailyVolume10Day: 5399520
        public decimal Bid { get; set; } //bid: 18.88
        public decimal Ask { get; set; } //ask: 18.88
        public long BidSize { get; set; } //bidSize: 200000
        public long AskSize { get; set; } //askSize: 550000
        public long MarketCap { get; set; } //marketCap: 17587437568
        public long NonDilutedMarketCap { get; set; } //nonDilutedMarketCap: 20238765565
        public decimal FiftyTwoWeekLow { get; set; } //fiftyTwoWeekLow: 18.68
        public decimal FiftyTwoWeekHigh { get; set; } //fiftyTwoWeekHigh: 26.52
        public decimal AllTimeHigh { get; set; } //allTimeHigh: 136.27354
        public decimal AllTimeLow { get; set; } //allTimeLow: 8.733895
        public decimal FullDayPrice { get; set; } //fullDayPrice: 18.87
        public decimal FullDayChange { get; set; } //fullDayChange: 0.09000015
        public decimal FullDayChangePercent { get; set; } //fullDayChangePercent: 0.00479234
        public decimal PriceToSalesTrailing12Months { get; set; } //priceToSalesTrailing12Months: 0.70925665
        public decimal FiftyDayAverage { get; set; } //fiftyDayAverage: 22.3106
        public decimal TwoHundredDayAverage { get; set; } //twoHundredDayAverage: 23.70135
        public decimal TrailingAnnualDividendRate { get; set; } //trailingAnnualDividendRate: 1.75
        public decimal TrailingAnnualDividendYield { get; set; } //trailingAnnualDividendYield: 0.09318423
        public string? Currency { get; set; } //currency: "USD"
        public string? FromCurrency { get; set; } //fromCurrency: null
        public string? ToCurrency { get; set; } //toCurrency: null
        public string? LastMarket { get; set; } //lastMarket: null
        public string? CoinMarketCapLink { get; set; } //coinMarketCapLink: null
        public string? Algorithm { get; set; } //algorithm: null
        public bool Tradeable { get; set; } //tradeable: false
  }
}



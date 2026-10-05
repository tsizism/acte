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

public abstract class YhStockPriceResultAbstract
{
    public abstract bool IsFullStockPriceResult { get; }

    public void PopulateDatabaseEntity(Equity equity)
    {
        var symbol = EquityUtils.GetSymbolAdjustedToMarket(equity);

        if (symbol != this.Symbol)
        {
            throw new InvalidOperationException("TickerPriceEntity.PopulateDatabaseEquity: Symbol mismatch.");
        }

        equity.Currency = this.Currency;
        equity.MarketPrice = this.Price;
        equity.CurrentPrice = this.Price;
    }


    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal MarketCap { get; set; }
    public string Error { get; set; } = string.Empty;

    public DateTime LastUpdated { get; set; }
}


// DTO for stock ticker price information retrieved from Yahoo Finance API
// Stock Price - "https://yh-finance-complete.p.rapidapi.com/yhprice?ticker=bce"), 4 keys (symbol, price, currency, marketCap)
public class YhStockPriceResult : YhStockPriceResultAbstract
{
    public override bool IsFullStockPriceResult => false;


    // TickerPriceEntity to Equity equity
}

// DTO for full stock price information retrieved from Yahoo Finance API
// Financials-Full Stock Price - "https://yh-finance-complete.p.rapidapi.com/price?symbol=bce" -38 keys
//                               "https://yh-finance-complete.p.rapidapi.com/price?symbol=bce.to"),  - 28 keys

// $"https://yh-finance-complete.p.rapidapi.com/price?symbol={symbol}";
public class YhGetFullStockPriceResult : YhStockPriceResultAbstract
{
    public override bool IsFullStockPriceResult => true;

    public YhGetFullStockPricePriceResult? PriceInfo { get; set; }

    //public override void PopulateDatabaseEntity(Equity equity)
    //{
    //    if (PriceInfo == null)
    //    {
    //        throw new InvalidOperationException("FullStockPriceEntity.PopulateDatabaseEntity: PriceInfo is null.");
    //    }

    //    equity.AverageCost = equity.AverageCost == 0 ? PriceInfo.RegularMarketPrice : equity.AverageCost;
    //    equity.MarketPrice = PriceInfo.RegularMarketPrice;
    //    equity.CurrentPrice = PriceInfo.RegularMarketPrice;


    //    if (PriceInfo.RegularMarketPrice > equity.HoldingHigh)
    //    {
    //        equity.HoldingHigh = PriceInfo.RegularMarketPrice;
    //        equity.HoldingHighAt = this.LastUpdated;
    //    }

    //    if (PriceInfo.RegularMarketPrice < equity.HoldingLow)
    //    {
    //        equity.HoldingLow = PriceInfo.RegularMarketPrice;
    //        equity.HoldingLowAt = this.LastUpdated;
    //    }
    //}

    public void PopulateDatabaseEquityMarket(EquityMarket equityMarket)
    {
        equityMarket.Currency = PriceInfo!.Currency;
        equityMarket.CurrentPrice = PriceInfo.RegularMarketPrice;
        equityMarket.PreviousClose = PriceInfo.RegularMarketPreviousClose;
        equityMarket.OpenPrice = PriceInfo.RegularMarketOpen;
        equityMarket.DayHigh = PriceInfo.RegularMarketDayHigh;
        equityMarket.DayLow = PriceInfo.RegularMarketDayLow;
        equityMarket.Volume = PriceInfo.RegularMarketVolume;
        equityMarket.MarketCap = PriceInfo.MarketCap;
        equityMarket.Week52High = 0;
        equityMarket.Week52Low = 0;
        equityMarket.LastTradeTime = PriceInfo.RegularMarketTime;
        equityMarket.LastUpdated = DateTime.UtcNow;
    }
}
public class YhGetFullStockPricePriceResult
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



using UIPooc.Models;

namespace UIPooc.Services
{
    public interface IFinanceService
    {
        // Quote Operations
        Task<EquityMarket?> GetQuoteAsync(string symbol, string market = "US");
        Task<List<EquityMarket>> GetQuotesAsync(List<string> symbols, string market = "US");
        Task<EquityMarket?> GetQuoteAndCacheAsync(string symbol, string market = "US");
        Task<List<EquityMarket>> GetQuotesAndCacheAsync(List<string> symbols, string market = "US");

        // Historical Data
        Task<List<StockHistoricalData>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate, string market = "US");
        Task<List<StockHistoricalData>> GetHistoricalDataAsync(string symbol, string period = "1mo", string interval = "1d", string market = "US");

        // Market Summary
        Task<EquityMarket?> GetMarketSummaryAsync(string symbol, string market = "US");

        // Batch Operations
        Task EtlEquityPricesAsync(int holdingId);

        Task<bool> RefreshMarketCacheAsync(string symbol, string market);

        // Search
        Task<List<EquitySearchResult>> SearchSymbolsAsync(string query);
        //Task<TickerPriceEntity> GetTickerPriceAsync(string ticker);
        Task<decimal> GetCADUSDExchangeRateAsync();
        Task<decimal> GetCADExchangeRateAsync();
        Task<List<Equity>> GetEquitiesForHoldingAsync(Holding holding);
        Task<List<Holding>> GetHoldingsAsync();
        Task<Holding?> GetHoldingAsync(int holdingId);
    }

    public class StockHistoricalData
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal AdjustedClose { get; set; }
        public long Volume { get; set; }
    }

    public class MarketSummary
    {
        public string Symbol { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string LongName { get; set; } = string.Empty;
        public decimal RegularMarketPrice { get; set; }
        public decimal RegularMarketChange { get; set; }
        public decimal RegularMarketChangePercent { get; set; }
        public long RegularMarketVolume { get; set; }
        public decimal? MarketCap { get; set; }
        public decimal? FiftyTwoWeekHigh { get; set; }
        public decimal? FiftyTwoWeekLow { get; set; }
        public decimal? TrailingPE { get; set; }
        public decimal? DividendYield { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Exchange { get; set; } = string.Empty;
    }

    public class EquitySearchResult
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Exchange { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}

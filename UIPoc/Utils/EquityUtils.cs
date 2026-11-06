using UIPooc.Models;
using UIPooc.Services;

namespace UIPooc.Utils;

static public class EquityUtils
{
    static public string GetSymbolAdjustedToMarket(Equity equity)
    {
        //return equity.Market == "CDN" ? equity.Symbol + ".TO" : equity.Symbol;
        return GetSymbolAdjustedToMarket(equity.Symbol, equity.Market);
    }

    static public string GetSymbolAdjustedToMarket(string symbol, string marker)
    {
        return marker == "CDN" ? symbol + ".TO" : symbol;
    }


    static public string GetMarketFromSymbol(string symbol, out string marker)
    {
        marker = "US";
        if (symbol.EndsWith(".TO"))
        {
            marker = "CDN";
            symbol = symbol.Replace(".TO", "");
        }
        return symbol;
    }


    static public decimal CalculateGainLossPercent(Equity equity)
    {
        var costBasis = equity.Quantity * equity.AverageCost;
        if (costBasis == 0)
        {
            return 0;
        }

        return equity.GainLoss!.Value / costBasis;
    }

    static public void CalculateGainLoss(List<Equity> equities, out decimal totalGainLoss, out decimal totalGainLossPercent)
    {
        totalGainLoss = equities.Sum(e => e.GainLoss!.Value);
        var totalCost = equities.Sum(e => e.Quantity * e.AverageCost);
        totalGainLossPercent = totalCost != 0 ? totalGainLoss / totalCost : 0;
    }

    static public bool ValidateSymbol(Equity equity, out string symbolValidationMessage)
    {
        var symbol = equity.Symbol?.Trim() ?? string.Empty;

        if (symbol.Length is < 1 or > 5)
        {
            symbolValidationMessage  = "Symbol must be between 1 and 5 characters";
            return false;
        }

        if (!symbol.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-'))
        {
            symbolValidationMessage = "Symbol can only contain letters, digits, '.', or '-'";
            return false;
        }

        //var tickerPrice = await FinanceService.GetTickerPriceAsync(symbol);
        //if (tickerPrice == null)
        //{
        //    _symbolValidationMessage = $"Symbol '{symbol}' is not valid or not found.";
        //    return false;
        //}

        symbolValidationMessage = string.Empty;
        return true;
    }

    public static string GetFinancePortalUrlForStock(string marketSymbol)
    {
        if (string.IsNullOrWhiteSpace(marketSymbol))
        {
            return "#"; // Return a non-functional link if symbol is missing
        }

        // Normalize market to uppercase for reliable comparison
        var normalizedMarket = marketSymbol.Trim().ToUpperInvariant();

        var baseUrl = normalizedMarket.EndsWith(".TO") ? "https://ca.finance.yahoo.com/quote/" : "https://finance.yahoo.com/quote/";

        return $"{baseUrl}{marketSymbol}";
    }

    public static Equity AdjustEquityHoldingHighLow(Equity equity)
    {
        if (equity.CurrentPrice > equity.HoldingHigh)
        {
            equity.HoldingHigh = equity.CurrentPrice;
            equity.HoldingHighAt = DateTime.UtcNow;
        }

        if (equity.HoldingLow == 0 || equity.CurrentPrice < equity.HoldingLow)
        {
            equity.HoldingLow = equity.CurrentPrice;
            equity.HoldingLowAt = DateTime.UtcNow;
        }

        return equity;
    }


    //Date: Nov. 02, 2026
    //Settlement Date S: Nov. 03, 2026
    //CAD Buy
    //ORACLE CORPORATION
    //ORCL:US	100	
    //USD118.80 US 
    //Canadian dollars -$16,894.92
    //Settlement -
    //Commission USD 6.95 US dollars
    //Exchange rate: 1.42130

    public static Equity CreateEquityFromTransaction(Transaction transaction)
    {
        Equity equity = new Equity
        {
            HoldingId = transaction.HoldingId, 
            Currency = transaction.Currency,
            Market = transaction.Market,
            Symbol = transaction.Symbol,
            //Keywords = other.Keywords;
            Quantity = (int)transaction.Quantity,
            AverageCost = transaction.Price,
            AverageCostMarket = transaction.MarketPrice,
            CurrentPrice = transaction.Price,
            MarketPrice = transaction.MarketPrice,
            GainLoss = 0,
            ///////////
            LastTxnType = transaction.Type,
            LastTxnQuantity = transaction.Quantity,
            LastTxnMarketPrice = transaction.MarketPrice,
            LastTxnAt = transaction.TransactionDate,
            //////////
            HoldingHigh = transaction.Price,
            HoldingHighAt = transaction.TransactionDate,
            IndexWeight = 0,
            HoldingLow = transaction.Price,
            HoldingLowAt = transaction.TransactionDate,
            FlagMax = 0,
            FlagMin = 0,
            LastUpdated = transaction.TransactionDate.Date,
            //Flag = transaction.Flag,
            //FlagMessage = transaction.FlagMessage,
            //FlagDate = transaction.FlagDate,
            Holding = transaction.Holding,
        };

        return equity;
    }

}


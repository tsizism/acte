 
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Radzen.Blazor.Rendering;
using UIPooc.Data;
using UIPooc.Models;
using UIPooc.Yahoo;

namespace UIPooc.Utils;

static public class EquityUtils
{
    static public decimal CalculateGainLossPercent(Equity equity)
    {
        var costBasis = equity.Quantity * equity.AverageCost;
        if (costBasis == 0)
        {
            return 0;
        }

        return equity.GainLoss!.Value / costBasis;
    }

    static public void CalculateGainLoss(List<Equity> _equities, out decimal totalGainLoss, out decimal totalGainLossPercent)
    {
        totalGainLoss = _equities.Sum(e => e.GainLoss!.Value);
        var totalCost = _equities.Sum(e => e.Quantity * e.AverageCost);
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



}


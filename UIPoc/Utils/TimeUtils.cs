 
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Radzen.Blazor.Rendering;
using UIPooc.Data;
using UIPooc.Models;
using UIPooc.Yahoo;

namespace UIPooc.Utils;

static public class TimeUtils
{
    static readonly TimeOnly TRADING_START_UTC = TimeOnly.Parse("14:30");
    static readonly TimeOnly TRADING_FINISH_UTC = TimeOnly.Parse("21:00");
    static readonly int TICKER_CACHE_DURATION_MINUTES = 120; // 2 hours
    static readonly int SYMBOL_FULL_PRICE_CACHE_DURATION_MINUTES = 480; // 4 hours


    /// <summary>
    /// Pre-market session: 4:00 a.m. – 9:30 a.m. ET.  (09:00 to 14:30 UTC)
    // After-hours session: 4:00 p.m. – 8:00 p.m.ET.   (21:00 to 01:00 UTC)
    // Overnight trading: Some platforms allow trading between 8:00 p.m.and 4:00 a.m.ET.
    // Regular trading hours: 9:30 a.m. – 4:00 p.m ET.   (14:30 to 21:00 UTC)
    /// </summary>
    /// <param name="ticker"></param>
    /// <param name="live"></param>
    /// <returns></returns>
    /// 


    static public bool IsTicketPriceCacheExpired(DateTime dt) => (DateTime.UtcNow - dt).TotalMinutes  > TICKER_CACHE_DURATION_MINUTES;
    //if ((DateTime.UtcNow - cached.LastUpdated).TotalMinutes<SYMBOL_FULL_PRICE_CACHE_DURATION_MINUTES)
    static public bool IsFullStockPriceCacheExpired(DateTime dt) => (DateTime.UtcNow - dt).TotalMinutes > SYMBOL_FULL_PRICE_CACHE_DURATION_MINUTES;

    static public bool IsTradingTime()
    {
        DateTime currentTime = DateTime.UtcNow;
        DayOfWeek day = currentTime.DayOfWeek;

        if ((day == DayOfWeek.Saturday) || (day == DayOfWeek.Sunday))
        {
            return false;
        }

        TimeOnly nowTimeOnly = TimeOnly.FromDateTime(currentTime);
        if (nowTimeOnly < TRADING_START_UTC || nowTimeOnly > TRADING_FINISH_UTC)
        {
            return false;
        }

        return true;
    }

    static public bool IsEquityUpToDate(DateTime equityDateTime)
    {
        if (equityDateTime.Date != DateTime.UtcNow.Date)
        {
            return false;
        }

        DayOfWeek day = equityDateTime.DayOfWeek;

        if ((day == DayOfWeek.Saturday) || (day == DayOfWeek.Sunday))
        {
            return true;
        }

        // Weekday
        TimeOnly equityTimeOnly = TimeOnly.FromDateTime(equityDateTime);

        if (equityTimeOnly < TRADING_START_UTC || equityTimeOnly > TRADING_FINISH_UTC)
        {
            return true;
        }

        if (equityTimeOnly - TRADING_START_UTC < TimeSpan.FromHours(4))
        {
            return true;
        }

        return false;
    }

}


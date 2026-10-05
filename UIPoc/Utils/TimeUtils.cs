 
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
    static readonly TimeOnly TRADING_30_MIN_IN_UTC = TimeOnly.Parse("15:00");
    static readonly TimeOnly TRADING_FINISH_UTC = TimeOnly.Parse("21:00");
    static readonly int TICKER_CACHE_DURATION_MINUTES = 120; // 2 hours
    static readonly int SYMBOL_FULL_PRICE_CACHE_DURATION_MINUTES = 480; // 4 hours


    /// <summary>
    /// Pre-market session: 4:00 a.m. – 9:30 a.m. ET.  (09:00 to 14:30 UTC)
    // After-hours session: 4:00 p.m. – 8:00 p.m.ET - 00:00 am EST.   (21:00 to 01:00 UTC to 05:00 UTC)
    // Overnight trading: Some platforms allow trading between 8:00 p.m.and 4:00 a.m.ET.
    // Regular trading hours: 9:30 a.m. – 4:00 p.m ET.   (14:30 to 21:00 UTC)
    /// </summary>
    /// <param name="ticker"></param>
    /// <param name="live"></param>
    /// <returns></returns>
    /// 


    static public bool IsTickerPriceCacheExpired(DateTime dt) => (DateTime.UtcNow - dt).TotalMinutes  > TICKER_CACHE_DURATION_MINUTES;
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

    public static bool IsEquityMarketCacheExpired(DateTime lastUpdated)
    {
        throw new NotImplementedException();
    }


    // TODO: Consider using TimeZoneInfo.ConvertTimeFromUtc to convert to Eastern Time instead of hardcoding UTC offsets.
    static public bool IsHoldingUpToDate(DateTime equityDateTime)
    {
        //if (!IsTradingTime())
        //{
        //    return true;
        //}

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

    static public DateTime AdjustEquityTime(DateTime equityDateTime)
    {
        DateTime nowUtc = DateTime.UtcNow;
        if (IsTradingTime())
        {
            DateTime trading30MinInDateTime = nowUtc.Date.Add(TRADING_30_MIN_IN_UTC.ToTimeSpan());
            if (equityDateTime < trading30MinInDateTime)
            {
                return nowUtc.Date.Add(TRADING_START_UTC.ToTimeSpan()); // Adjust to the start of trading hours (14:30 UTC)
            }

            return equityDateTime;
        }

        if (equityDateTime.TimeOfDay > TRADING_FINISH_UTC.ToTimeSpan())
        {
            return nowUtc.Date.Add(TRADING_FINISH_UTC.ToTimeSpan());
        }

        return equityDateTime;
    }

    static public string EquityTimeToAction(DateTime equityDateTime, string ticker)
    {
        bool isTradingNow = IsTradingTime();
        DateTime nowUtc = DateTime.UtcNow;
        DateTime trading30MinInDateTime = nowUtc.Date.Add(TRADING_30_MIN_IN_UTC.ToTimeSpan());

        TimeSpan timeSinceEquity = nowUtc - equityDateTime;

        if (isTradingNow)
        {
            if (equityDateTime < trading30MinInDateTime)  // EDT before 30 minutes into trading, we need a full update (before 10am EST)
            {
                return "FullUpdate";
            }

            if (equityDateTime.AddHours(SYMBOL_FULL_PRICE_CACHE_DURATION_MINUTES / 60) < nowUtc) // equityDateTime is more than eg. 4 hour old
            {
                return "FullUpdate";
            }

            if (equityDateTime.AddHours(TICKER_CACHE_DURATION_MINUTES / 60) < nowUtc) // equityDateTime is more than eg 2 hour old
            {
                return "QuickUpdate";
            }

            if (equityDateTime > nowUtc)
            {
                throw new ArgumentException("equityDateTime cannot be in the future.");
            }
        }
        else // not trading now
        {
            DayOfWeek today = nowUtc.DayOfWeek;

            //if ((day == DayOfWeek.Saturday) || (day == DayOfWeek.Sunday) || (day == DayOfWeek.Friday))
            //{
            //    if (equityDateTime.Date < nowUtc.Date) // equityDateTime is from a previous day, we need a full update.
            //    {
            //        return ("FullUpdate", nowUtc);
            //    }
            //    else
            //    {
            //        return ("NoUpdate", null);
            //    }

            //}

            // We check in after hours on Tuesday, Wednesday, Thursday, Friday
            if ((today == DayOfWeek.Monday) || (today == DayOfWeek.Tuesday) || (today == DayOfWeek.Wednesday) || (today == DayOfWeek.Thursday) || (today == DayOfWeek.Friday))
            {
                if (equityDateTime.Day < nowUtc.Day)  
                {
                    return "FullUpdate";        // we are in after hours, equityDateTime is from a previous day, we need a full update.
                }

                if (equityDateTime.TimeOfDay < TRADING_FINISH_UTC.ToTimeSpan()) 
                {
                    return "FullUpdate"; // Last update before end of trade ie 16:30 PM EST (21:30 UTC)
                }
                else // equityDateTime is at the end of trading hours (21:00 UTC)
                {
                    if (equityDateTime.TimeOfDay != TRADING_FINISH_UTC.ToTimeSpan())
                    {
                        throw new ArgumentException("equityDateTime should be at the end of trading hours (21:00 UTC) for after hours check.");
                    }
                    return "NoUpdate"; // After trading hours, equityDateTime is during trading hours.
                }
            }
            else //if (today == DayOfWeek.Saturday || today == DayOfWeek.Sunday)
            {
                int prevDays = (today == DayOfWeek.Saturday) ? 1 : 2; // Saturday -> Friday, Sunday -> Friday

                // Weekend check, equityDateTime should be from Friday's trading session
                DateTime fridayEndOfSessionEquityTime = nowUtc.Date.AddDays(-1 * prevDays).Add(TRADING_FINISH_UTC.ToTimeSpan());
                
                if (equityDateTime.Date < fridayEndOfSessionEquityTime)
                {
                    return "FullUpdate"; // Last update before end of trade ie 16:30 PM EST (21:30 UTC)
                }
                else
                {
                    return "NoUpdate"; 
                }
            }

            //return "NoUpdate";
            //{
            //    if (timeSinceEquity < TimeSpan.FromHours(18)) // last time after trade hours , ignore
            //    {
            //        return ("NoUpdate", nowUtc); 
            //    }
            //    else 
            //    {
            //        return ("FullUpdate", nowUtc);  
            //    }
            //}


            // After trading hours, equityDateTime is during trading hours, we need a full update.
            //if (equityDateTime.TimeOfDay > TRADING_START_UTC.ToTimeSpan() && equityDateTime.TimeOfDay < TRADING_FINISH_UTC.ToTimeSpan() && nowUtc.TimeOfDay > TRADING_FINISH_UTC.ToTimeSpan()) 
            //{
            //    return ("FullUpdate", nowUtc);
            //}

            //if (nowUtc.TimeOfDay > TRADING_FINISH_UTC.ToTimeSpan())
            //{

            //}


            //Aftermarket Mon, Tue, Wed, Thu 



            //if ((day == DayOfWeek.Monday) || (day == DayOfWeek.Tuesday) || (day == DayOfWeek.Wednesday) || (day == DayOfWeek.Thursday))
            //{
            //    // Aftermarket hours to next trading session (21:00 to 05:00 UTC) we need a quick update if equityDateTime is before 21:00 UTC, otherwise no update needed.
            //    if (nowUtc.TimeOfDay >= TRADING_FINISH_UTC.ToTimeSpan() || nowUtc.TimeOfDay <= TRADING_START_UTC.ToTimeSpan()) // 21:00 to 05:00 UTC (midnight etc)
            //    {
            //        if( equityDateTime.TimeOfDay < TRADING_FINISH_UTC.ToTimeSpan())
            //        {
            //            return ("QuickUpdate", nowUtc);
            //        }
            //        else
            //        {
            //            return ("NoUpdate", null);
            //        }

            //    }
            //}
            //if ((nowUtc - equityDateTime).TotalHours >= 17) // 17 hour difference between now and equityDateTime, we need a full update. 
            //{
            //    return ("FullUpdate", nowUtc); // may be "quick" 17 hours is the time between 21:00 and 14:00 UTC, which is the time between the end of trading and the start of trading the next day
            //}



        }


        //DateTime yesterdayUtc = nowUtc.Date.AddDays(-1);
        //DateTime dayBeforeYesterdayUtc = nowUtc.Date.AddDays(-2);

        //DayOfWeek equityDay = equityDateTime.Date.DayOfWeek;

        //// equityDateTime is from the previous calendar day
        //bool isPreviousDay = equityDateTime.Date == yesterdayUtc || equityDateTime.Date == dayBeforeYesterdayUtc;

        ////equityDateTime is yesterday & yesterday was Fri FullUpdate

        //// Previous day was a Friday (covers weekend gap: Fri data, Mon now)
        //if (isPreviousDay && equityDay == DayOfWeek.Friday)
        //    return "FullUpdate";

        //// Previous day was Sun–Thu and we are currently in trading hours
        //if (isPreviousDay
        //    && equityDay != DayOfWeek.Saturday
        //    && equityDay != DayOfWeek.Sunday
        //    && isTradingNow)
        //    return "FullUpdate";

        //// Currently trading and equityDateTime is more than 1 hour old
        //if (isTradingNow && (nowUtc - equityDateTime).TotalHours > 1)
        //    return "QuickUpdate";

        //// Past trading hours but equityDateTime was captured during trading hours
        //if (!isTradingNow)
        //{
        //    TimeOnly equityTime = TimeOnly.FromDateTime(equityDateTime);
        //    bool equityWasDuringTrading =
        //        equityTime >= TRADING_START_UTC &&
        //        equityTime <= TRADING_FINISH_UTC;

        //    if (equityWasDuringTrading)
        //        return "QuickUpdate";
        //}

        return "NoUpdate";
    }


    /// <summary>
    /// Determines the action to take based on the equityDateTime and current time.
    /// \ChatLogs\sep22.md  --> EquityTimeToAction
    /// </summary>
    /// <param name="equityDateTime"></param>
    /// <returns></returns>
    static public string EquityTimeToAction2(DateTime equityDateTime)
    {
        DateTime nowUtc         = DateTime.UtcNow;
        DateTime yesterdayUtc   = nowUtc.Date.AddDays(-1);
        DateTime dayBeforeYesterdayUtc = nowUtc.Date.AddDays(-2);
        bool     isTradingNow   = IsTradingTime();
        DayOfWeek equityDay     = equityDateTime.Date.DayOfWeek;

        // equityDateTime is from the previous calendar day
        bool isPreviousDay = equityDateTime.Date == yesterdayUtc || equityDateTime.Date == dayBeforeYesterdayUtc;

        //equityDateTime is yesterday & yesterday was Fri FullUpdate

        // Previous day was a Friday (covers weekend gap: Fri data, Mon now)
        if (isPreviousDay && equityDay == DayOfWeek.Friday)
            return "FullUpdate";

        // Previous day was Sun–Thu and we are currently in trading hours
        if (isPreviousDay
            && equityDay != DayOfWeek.Saturday
            && equityDay != DayOfWeek.Sunday
            && isTradingNow)
            return "FullUpdate";

        // Currently trading and equityDateTime is more than 1 hour old
        if (isTradingNow && (nowUtc - equityDateTime).TotalHours > 1)
            return "QuickUpdate";

        // Past trading hours but equityDateTime was captured during trading hours
        if (!isTradingNow)
        {
            TimeOnly equityTime = TimeOnly.FromDateTime(equityDateTime);
            bool equityWasDuringTrading =
                equityTime >= TRADING_START_UTC &&
                equityTime <= TRADING_FINISH_UTC;

            if (equityWasDuringTrading)
                return "QuickUpdate";
        }

        return "NoUpdate";
    }

}




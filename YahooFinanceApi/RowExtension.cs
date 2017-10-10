using System;
using System.Globalization;
using NodaTime;

namespace YahooFinanceApi
{
    static class RowExtension
    {
        public static Candle ToCandle(this string[] row, string timeZone, Period period)
            => new Candle(row[0].ToSpecDateTime(timeZone, period),
                          Convert.ToDecimal(row[1], CultureInfo.InvariantCulture),
                          Convert.ToDecimal(row[2], CultureInfo.InvariantCulture),
                          Convert.ToDecimal(row[3], CultureInfo.InvariantCulture),
                          Convert.ToDecimal(row[4], CultureInfo.InvariantCulture),
                          Convert.ToInt64(row[6], CultureInfo.InvariantCulture),
                          Convert.ToDecimal(row[5], CultureInfo.InvariantCulture));

        public static Candle ToFallbackCandle(this string[] row, string timeZone, Period period)
            => new Candle(row[0].ToSpecDateTime(timeZone, period), 0, 0, 0, 0, 0, 0);

        public static DividendTick ToDividendTick(this string[] row, string timeZone, Period period)
            => new DividendTick(row[0].ToSpecDateTime(timeZone, period), 
                                Convert.ToDecimal(row[1], CultureInfo.InvariantCulture));

        public static DividendTick ToFallbackDividendTick(this string[] row, string timeZone, Period period)
            => new DividendTick(row[0].ToSpecDateTime(timeZone, period), 0);

        public static SplitTick ToSplitTick(this string[] row, string timeZone, Period period)
            => new SplitTick(row[0].ToSpecDateTime(timeZone, period),
                             Convert.ToInt32(row[1].Split('/')[0], CultureInfo.InvariantCulture),
                             Convert.ToInt32(row[1].Split('/')[1], CultureInfo.InvariantCulture));

        public static SplitTick ToFallbackSplitTick(this string[] row, string timeZone, Period period)
            => new SplitTick(row[0].ToSpecDateTime(timeZone, period), 0, 0);

        internal static DateTime ToSpecDateTime(this string dateTimeStr, string timeZone, Period period)
        {
            var dateTime = Convert.ToDateTime(dateTimeStr, CultureInfo.InvariantCulture);

            var provider = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            if (!string.IsNullOrWhiteSpace(timeZone))
                provider = DateTimeZoneProviders.Tzdb[timeZone];

            // The returned data from csv is timed by UTC, if the market opening time is less than the utc value, the date is reduced by 1
            // The below flag checks it, and make sure that the data is corrected
            bool isAboveUtcPlus9 = provider.MinOffset > Offset.FromHours(9);

			bool roundToDay = period == Period.Daily || period == Period.Weekly || period == Period.Monthly;
            if (roundToDay && isAboveUtcPlus9)
                dateTime = dateTime.Date.AddDays(1);
            
            return dateTime;
        }
    }
}

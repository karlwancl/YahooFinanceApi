using System;
using System.Globalization;
namespace YahooFinanceApi
{
    static class RowExtension
    {
        public static Candle ToCandle(this string[] row)
            => new Candle(Convert.ToDateTime(row[0], CultureInfo.InvariantCulture),
                          Convert.ToDecimal(row[1], CultureInfo.InvariantCulture),
                          Convert.ToDecimal(row[2], CultureInfo.InvariantCulture),
                          Convert.ToDecimal(row[3], CultureInfo.InvariantCulture),
                          Convert.ToDecimal(row[4], CultureInfo.InvariantCulture),
                          Convert.ToInt64(row[6], CultureInfo.InvariantCulture),
                          Convert.ToDecimal(row[5], CultureInfo.InvariantCulture));

        public static Candle ToFallbackCandle(this string[] row)
            => new Candle(Convert.ToDateTime(row[0], CultureInfo.InvariantCulture), 0, 0, 0, 0, 0, 0);

        public static DividendTick ToDividendTick(this string[] row)
            => new DividendTick(Convert.ToDateTime(row[0], CultureInfo.InvariantCulture), 
                                Convert.ToDecimal(row[1], CultureInfo.InvariantCulture));

        public static DividendTick ToFallbackDividendTick(this string[] row)
            => new DividendTick(Convert.ToDateTime(row[0], CultureInfo.InvariantCulture), 0);

        public static SplitTick ToSplitTick(this string[] row)
            => new SplitTick(Convert.ToDateTime(row[0], CultureInfo.InvariantCulture),
                             Convert.ToInt32(row[1].Split('/')[0], CultureInfo.InvariantCulture),
                             Convert.ToInt32(row[1].Split('/')[1], CultureInfo.InvariantCulture));

        public static SplitTick ToFallbackSplitTick(this string[] row)
            => new SplitTick(Convert.ToDateTime(row[0], CultureInfo.InvariantCulture), 0, 0);
    }
}

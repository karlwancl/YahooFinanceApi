using System;
namespace YahooFinanceApi
{
    static class RowExtension
    {
        public static Candle ToCandle(this string[] row)
            => new Candle(Convert.ToDateTime(row[0]),
                          Convert.ToDecimal(row[1]),
                          Convert.ToDecimal(row[2]),
                          Convert.ToDecimal(row[3]),
                          Convert.ToDecimal(row[4]),
                          Convert.ToInt64(row[6]),
                          Convert.ToDecimal(row[5]));

        public static DividendTick ToDividendTick(this string[] row)
            => new DividendTick(Convert.ToDateTime(row[0]), Convert.ToDecimal(row[1]));

        public static SplitTick ToSplitTick(this string[] row)
            => new SplitTick(Convert.ToDateTime(row[0]), Convert.ToInt32(row[1].Split('/')[0]), Convert.ToInt32(row[1].Split('/')[1]));
    }
}

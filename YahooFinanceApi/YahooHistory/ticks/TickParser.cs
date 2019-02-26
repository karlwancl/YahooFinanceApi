using System;
using System.Globalization;

namespace YahooFinanceApi
{
    public static class TickParser
    {
        public static bool IgnoreEmptyRows { get; set; }

        internal static string GetParamFromType<ITick>()
        {
            var type = typeof(ITick);

            if (type == typeof(HistoryTick))
                return "history";
            else if (type == typeof(DividendTick))
                return "div";
            else if (type == typeof(SplitTick))
                return "split";

            throw new Exception("GetParamFromTickType<T>: Invalid type.");
        }

        internal static ITick Parse<ITick>(string[] row)
        {
            var type = typeof(ITick);
            object instance;

            if (type == typeof(HistoryTick))
                instance = ToHistoryTick(row);
            else if (type == typeof(DividendTick))
                instance = ToDividendTick(row);
            else if (type == typeof(SplitTick))
                instance = ToSplitTick(row);
            else
                throw new Exception("Parse<ITick>: Invalid type.");

            return (ITick)instance;
        }

        private static HistoryTick ToHistoryTick(string[] row)
        {
            var tick = new HistoryTick
            {
                DateTime      = row[0].ToDateTime(),
                Open          = row[1].ToDecimal(),
                High          = row[2].ToDecimal(),
                Low           = row[3].ToDecimal(),
                Close         = row[4].ToDecimal(),
                AdjustedClose = row[5].ToDecimal(),
                Volume        = row[6].ToInt64()
            };

            if (IgnoreEmptyRows &&
                tick.Open == 0 && tick.High == 0 && tick.Low == 0 && tick.Close == 0 &&
                tick.AdjustedClose == 0 &&  tick.Volume == 0)
                return null;

            return tick;
        }

        private static DividendTick ToDividendTick(string[] row)
        {
            var tick = new DividendTick
            {
                DateTime = row[0].ToDateTime(),
                Dividend = row[1].ToDecimal()
            };

            if (IgnoreEmptyRows && tick.Dividend == 0)
                return null;

            return tick;
        }

        private static SplitTick ToSplitTick(string[] row)
        {
            var tick = new SplitTick { DateTime = row[0].ToDateTime() };

            var split = row[1].Split('/');
            if (split.Length == 2)
            {
                tick.AfterSplit  = split[0].ToDecimal();
                tick.BeforeSplit = split[1].ToDecimal();
            }

            if (IgnoreEmptyRows && tick.AfterSplit == 0 && tick.BeforeSplit == 0)
                return null;

            return tick;
        }

        private static DateTime ToDateTime(this string str)
        {
            if (!DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                throw new Exception($"Could not convert '{str}' to DateTime.");
            return dt;
        }

        private static decimal ToDecimal(this string str)
        {
            decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result);
            return result;
        }

        private static long ToInt64(this string str)
        {
            long.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out long result);
            return result;
        }
    }
}

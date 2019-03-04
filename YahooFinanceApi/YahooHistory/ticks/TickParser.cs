using System;
using System.Diagnostics;
using System.Globalization;
using NodaTime;
using NodaTime.Text;

#nullable enable

namespace YahooFinanceApi
{
    internal static class TickParser
    {
        private static readonly LocalDatePattern pattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");
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

        internal static ITick? Parse<ITick>(string[] row) where ITick: class
        {
            var type = typeof(ITick);
            object? instance;

            if (type == typeof(HistoryTick))
                instance = ToHistoryTick(row);
            else if (type == typeof(DividendTick))
                instance = ToDividendTick(row);
            else if (type == typeof(SplitTick))
                instance = ToSplitTick(row);
            else
                throw new Exception("Parse<ITick>: Invalid type.");

            return (ITick?)instance;
        }

        private static HistoryTick? ToHistoryTick(string[] row)
        {
            var tick = new HistoryTick
            {
                Date          = row[0].ToLocalDate(),
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

        private static DividendTick? ToDividendTick(string[] row)
        {
            var tick = new DividendTick
            {
                Date     = row[0].ToLocalDate(),
                Dividend = row[1].ToDecimal()
            };

            if (IgnoreEmptyRows && tick.Dividend == 0)
                return null;

            return tick;
        }

        private static SplitTick? ToSplitTick(string[] row)
        {
            var tick = new SplitTick { Date = row[0].ToLocalDate() };

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

        private static LocalDate ToLocalDate(this string str)
        {
            var result = pattern.Parse(str);
            return result.Success ? result.Value : throw new Exception($"Could not convert '{str}' to LocalDate.", result.Exception);
        }

        private static decimal ToDecimal(this string str)
        {
            if (str == "null")
                return 0M;

            if (!decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                throw new Exception($"Could not convert '{str}' to Decimal.");

            return result;
        }

        private static long ToInt64(this string str)
        {
            if (str == "null")
                return 0L;

            if (!long.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out long result))
                throw new Exception($"Could not convert '{str}' to Int64.");

            return result;
        }
    }
}

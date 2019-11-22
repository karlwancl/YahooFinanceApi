using NodaTime;

#nullable enable

namespace YahooFinanceApi
{
    public sealed class SplitTick : ITick
    {
        public LocalDate Date { get; }
        public decimal BeforeSplit { get;  }
        public decimal AfterSplit { get; }

        private SplitTick(string[]row)
        {
            Date = row[0].ToLocalDate();

            var split = row[1].Split('/');
            if (split.Length == 2)
            {
                BeforeSplit = split[0].ToDecimal();
                AfterSplit = split[1].ToDecimal();
            }
        }

        internal static SplitTick? Create(string[] row, bool ignoreEmptyRows)
        {
            var tick = new SplitTick(row);

            if (ignoreEmptyRows && tick.AfterSplit == 0 && tick.BeforeSplit == 0)
                return null;

            return tick;
        }
    }
}

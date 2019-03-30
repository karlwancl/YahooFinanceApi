using NodaTime;

#nullable enable

namespace YahooFinanceApi
{
    public sealed class HistoryTick: ITick
    {
        public LocalDate Date { get; }
        public decimal Open { get; }
        public decimal High { get; }
        public decimal Low { get; }
        public decimal Close { get; }
        public decimal AdjustedClose { get; }
        public long Volume { get; }

        private HistoryTick(string[] row)
        {
            Date = row[0].ToLocalDate();
            Open = row[1].ToDecimal();
            High = row[2].ToDecimal();
            Low = row[3].ToDecimal();
            Close = row[4].ToDecimal();
            AdjustedClose = row[5].ToDecimal();
            Volume = row[6].ToInt64();
        }

        internal static HistoryTick? Create(string[] row, bool ignoreEmptyRows)
        {
            var tick = new HistoryTick(row);

            if (ignoreEmptyRows &&
                tick.Open == 0 && tick.High == 0 && tick.Low == 0 && tick.Close == 0 &&
                tick.AdjustedClose == 0 && tick.Volume == 0)
                return null;

            return tick;
        }
    }
}

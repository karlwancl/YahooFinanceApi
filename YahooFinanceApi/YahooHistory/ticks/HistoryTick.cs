using NodaTime;

namespace YahooFinanceApi
{
    public sealed class HistoryTick: ITick
    {
        public LocalDate Date { get; internal set; }
        public decimal Open { get; internal set; }
        public decimal High { get; internal set; }
        public decimal Low { get; internal set; }
        public decimal Close { get; internal set; }
        public decimal AdjustedClose { get; internal set; }
        public long Volume { get; internal set; }
    }
}

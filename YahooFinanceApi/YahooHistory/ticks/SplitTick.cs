using NodaTime;

namespace YahooFinanceApi
{
    public sealed class SplitTick : ITick
    {
        public LocalDate Date { get; internal set;  }
        public decimal BeforeSplit { get; internal set; }
        public decimal AfterSplit { get; internal set; }
    }
}

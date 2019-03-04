using NodaTime;

namespace YahooFinanceApi
{
    public sealed class DividendTick : ITick
    {
        public LocalDate Date { get; internal set; }
        public decimal Dividend { get; internal set; }
    }
}

using NodaTime;

namespace YahooFinanceApi
{
    public interface ITick
    {
        LocalDate Date { get; }
    }
}

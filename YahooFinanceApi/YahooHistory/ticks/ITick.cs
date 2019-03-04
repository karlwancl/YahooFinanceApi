using NodaTime;

#nullable enable

namespace YahooFinanceApi
{
    public interface ITick
    {
        LocalDate Date { get; }
    }
}

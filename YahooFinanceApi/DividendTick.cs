using System;

namespace YahooFinanceApi
{
    public class DividendTick: ITick
    {
        public DividendTick(DateTime dateTime, decimal dividend)
        {
            DateTime = dateTime;
            Dividend = dividend;
        }

        public DateTime DateTime { get; }

        public decimal Dividend { get; }
    }
}

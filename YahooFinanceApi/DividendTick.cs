using System;
using System.Collections.Generic;
using System.Text;

namespace YahooFinanceApi
{
    public class DividendTick
    {
        public DividendTick(DateTime dateTime, decimal dividend)
        {
            DateTime = dateTime;
            Dividend = dividend;
        }

        public DateTime DateTime { get; private set; }

        public decimal Dividend { get; private set; }
    }
}

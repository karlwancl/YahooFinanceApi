using System;
using System.Collections.Generic;
using System.Text;

namespace YahooFinanceApi
{
    public class Candle
    {
        public Candle(DateTime dateTime, decimal open, decimal high, decimal low, decimal close, long volume, decimal adjustedClose)
        {
            DateTime = dateTime;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            AdjustedClose = adjustedClose;
        }

        public DateTime DateTime { get; private set; }

        public decimal Open { get; private set; }

        public decimal High { get; private set; }

        public decimal Low { get; private set; }

        public decimal Close { get; private set; }

        public long Volume { get; private set; }

        public decimal AdjustedClose { get; private set; }
    }
}

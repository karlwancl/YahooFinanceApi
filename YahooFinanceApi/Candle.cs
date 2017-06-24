using System;
using System.Collections.Generic;
using System.Text;

namespace YahooFinanceApi
{
    public class Candle: ITick
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

        public DateTime DateTime { get; }

        public decimal Open { get; }

        public decimal High { get; }

        public decimal Low { get; }

        public decimal Close { get; }

        public long Volume { get; }

        public decimal AdjustedClose { get; }
    }
}

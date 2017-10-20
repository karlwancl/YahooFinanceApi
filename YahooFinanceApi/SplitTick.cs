using System;
namespace YahooFinanceApi
{
    public class SplitTick: ITick
    {
        public DateTime DateTime { get; internal set;  }

        public decimal BeforeSplit { get; internal set; }

        public decimal AfterSplit { get; internal set; }
    }
}

using System;
namespace YahooFinanceApi
{
    public class SplitTick: ITick
    {
        public DateTime DateTime { get; }
        public decimal BeforeSplit { get; }
        public decimal AfterSplit { get; }

        public SplitTick(DateTime dateTime, decimal afterSplit, decimal beforeSplit)
        {
            AfterSplit = afterSplit;
            BeforeSplit = beforeSplit;
            DateTime = dateTime;
        }
    }
}

using System;
using System.Linq;
using YahooFinanceApi;

class Program
{
    static void Main(string[] args)
    {
        //var list = Yahoo.GetHistoricalAsync("AAPL", new DateTime(2016, 1, 1), period: Period.Daily).Result;
        //var divList = Yahoo.GetHistoricalDividendsAsync("AAPL").Result;
        var list = Yahoo
            .Symbol("AAPL", "GOOG")
            .Tag(Tag.LastTradePriceOnly, Tag.ChangeAndPercentChange, Tag.DaysLow, Tag.DaysHigh)
            .GetAsync()
            .Result;
        var aapl = list["AAPL"];
        Console.WriteLine(aapl[Tag.LastTradePriceOnly].GetValue<decimal>());
        Console.ReadLine();
    }
}
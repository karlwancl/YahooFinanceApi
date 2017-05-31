using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YahooFinanceApi;

class Program
{
    static void Main(string[] args)
    {
        IList<Candle> candles;

        string[] symbols = new string[] { "AAPL", "GOOG", "MSFT", "NVDA", "AMAT", "ATVI" };
        //Parallel.For(0, 5, i =>
        //{
        //    Console.WriteLine($"Start {symbols[i]}");
        //    candles = Yahoo.GetHistoricalAsync(symbols[i], new DateTime(2016, 1, 1), period: Period.Daily).Result;
        //    Console.WriteLine($"{symbols[i]} - O: {candles.Last().Open}, H: {candles.Last().High}, L: {candles.Last().Low}, C: {candles.Last().Close}");
        //    var divList = Yahoo.GetHistoricalDividendsAsync(symbols[i]).Result;
        //    Console.WriteLine("{0}: {1}", symbols[i],  divList.Any() ? divList.Last().DateTime.ToString() : "None");
        //});

        //candles = Yahoo.GetHistoricalAsync("^GSPC", new DateTime(2016, 1, 1), period: Period.Daily).Result;

        //var list = Yahoo
        //   .Symbol("VAW")
        //   .Tag(Tag.LastTradePriceOnly, Tag.ChangeAndPercentChange, Tag.DaysLow, Tag.DaysHigh)
        //   .GetAsync()
        //   .Result;
        //var aapl = list["VAW"];
        //Console.WriteLine(aapl[Tag.LastTradePriceOnly]);
        //Enumerable.Range(0, 100)
        //.AsParallel()
        //.WithDegreeOfParallelism(16)
        //.ForAll(i =>
        //{
        // try
        // {
        //  Yahoo.GetHistoricalAsync(symbols[0], new DateTime(2017, 1, 1), new DateTime(2017, 2, 1), Period.Daily).Wait();
        // }
        // catch (Exception ex)
        // {
        //  Console.WriteLine($"Failed: " + ex);
        // }
        // Console.WriteLine($"Done: " + i);
        //});
        var result = Yahoo.Symbol("AAPL").Tag(Tag.Open, Tag.DaysHigh, Tag.LowLimit).GetAsync().Result;
        Console.WriteLine(result["AAPL"][Tag.DaysHigh]);

        Console.ReadLine();
    }
}
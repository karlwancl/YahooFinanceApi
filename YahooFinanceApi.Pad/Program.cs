using System;
using YahooFinanceApi;

class Program
{
    static void Main(string[] args)
    {
        var list = Yahoo.GetAsync("AAPL", new DateTime(2016, 1, 1), period: Period.Daily).Result;
        var divList = Yahoo.GetDividendsAsync("AAPL").Result;
        Console.ReadLine();
    }
}
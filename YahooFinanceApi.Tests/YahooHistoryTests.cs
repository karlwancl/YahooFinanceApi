using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace YahooFinanceApi.Tests
{
    public class YahooHistoryTests
    {
        private readonly Action<string> Write;
        public YahooHistoryTests(ITestOutputHelper output) => Write = output.WriteLine;

        [Fact]
        public void NoSymbolsArgumentTest()
        {
            Assert.Throws<ArgumentException>(() => YahooHistory.Symbols());
            Assert.Throws<ArgumentException>(() => YahooHistory.Symbols(new string[] { }));
            Assert.Throws<ArgumentNullException>(() => YahooHistory.Symbols(null));
            Assert.Throws<ArgumentNullException>(() => YahooHistory.Symbols("C", null));
            Assert.Throws<ArgumentException>(() => YahooHistory.Symbols(""));
            Assert.Throws<ArgumentException>(() => YahooHistory.Symbols("C", " "));
        }

        [Fact]
        public void DuplicateSymbolsTest()
        {
            var exception = Assert.Throws<ArgumentException>(() => YahooHistory.Symbols("C", "X", "C"));
            Assert.StartsWith("Duplicate symbol(s): \"C\".", exception.Message);
        }

        [Fact]
        public async Task SuccessTest()
        {
            IReadOnlyList<(string Symbol, Task<IReadOnlyList<HistoryTick>> Task)> results = await YahooHistory.Symbols("C").GetHistoryAsync();
            Task<IReadOnlyList<HistoryTick>> task = results[0].Task;
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            IReadOnlyList <HistoryTick> ticks1 = results[0].Task.Result;
            Assert.Equal(ticks1, await results[0].Task);

            // SingleAsync() may be used to simplify the result from a single symbol.
            IReadOnlyList<HistoryTick> ticks2 = await YahooHistory.Symbols("C").GetHistoryAsync().SingleAsync();
            Assert.Equal(ticks1.Count, ticks2.Count);
        }

        [Fact]
        public async Task FailureTest()
        {
            IReadOnlyList<(string Symbol, Task<IReadOnlyList<HistoryTick>> Task)> results = await YahooHistory.Symbols("badSymbol").GetHistoryAsync();
            Task<IReadOnlyList<HistoryTick>> task1 = results[0].Task;
            Assert.Equal(TaskStatus.Faulted, task1.Status);
            var exception1 = task1.Exception.InnerException;
            Assert.Equal(exception1, await Assert.ThrowsAsync<Exception>(async () => await task1));

            // SingleAsync() may be used to simplify the result from a single symbol.
            var exception2 = await Assert.ThrowsAsync<Exception>(async () => await YahooHistory.Symbols("badSymbol").GetHistoryAsync().SingleAsync());
            Assert.Equal(exception2.Message, exception1.Message);
            Write(exception2.Message.ToString());
            Assert.Equal($"Invalid ticker or endpoint for symbol 'badSymbol'.", exception2.Message);
        }

        [Fact]
        public async Task MixedSymbolsTest()
        {
            string[] symbols = { "C", "badSymbol" };
            var results = await YahooHistory.Symbols(symbols).GetHistoryAsync();
            Assert.Equal(symbols.Length, results.Count);

            var successfulResult = results[0];
            successfulResult.Symbol = symbols[0];
            Assert.Equal(TaskStatus.RanToCompletion, successfulResult.Task.Status);
            var ticks = successfulResult.Task.Result;
            Assert.True(ticks.Count > 1);
            Assert.True(ticks[0].Close > 0);

            var faultedResult = results[1];
            faultedResult.Symbol = symbols[1];
            Assert.Equal(TaskStatus.Faulted, faultedResult.Task.Status);
            var exception1 = faultedResult.Task.Exception.InnerException; // AggregateException
            Assert.Equal(exception1, await Assert.ThrowsAsync<Exception>(async () => await faultedResult.Task));
            Write(exception1.ToString());
            Assert.Equal($"Invalid ticker or endpoint for symbol 'badSymbol'.", exception1.Message);
        }

        [Fact]
        public async Task PeriodTest()
        {
            var ticks = await YahooHistory.Symbols("AAPL").Period(new DateTime(2017, 1, 3), DateTime.Now)
                .GetHistoryAsync(Frequency.Daily).SingleAsync();
            Assert.Equal(115.800003m, ticks[0].Open);

            ticks = await YahooHistory.Symbols("AAPL").Period(new DateTime(2017, 1, 3), DateTime.Now)
                .GetHistoryAsync(Frequency.Weekly).SingleAsync();
            Assert.Equal(115.800003m, ticks[0].Open);

            ticks = await YahooHistory.Symbols("AAPL").Period(new DateTime(2017, 1, 3), DateTime.Now)
                .GetHistoryAsync(Frequency.Monthly).SingleAsync();
            Assert.Equal(115.800003m, ticks[0].Open);
        }

        [Fact]
        public async Task PeriodLatestTest()
        {
            var ticks = await YahooHistory.Symbols("C").Period(DateTime.Now.AddDays(-7))
                .GetHistoryAsync().SingleAsync();
            foreach (var tick in ticks)
                Write($"{tick.DateTime} {tick.Close}");
        }

        [Fact]
        public async Task HistoryTickTest()
        {
            var ticks = await YahooHistory.Symbols("AAPL").Period(new DateTime(2017, 1, 3), new DateTime(2017, 1, 4))
                .GetHistoryAsync(Frequency.Daily).SingleAsync();
            Assert.Equal(2, ticks.Count());

            var tick = ticks[0];
            Assert.Equal(115.800003m, tick.Open);
            Assert.Equal(116.330002m, tick.High);
            Assert.Equal(114.760002m, tick.Low);
            Assert.Equal(116.150002m, tick.Close);
            Assert.Equal(28_781_900, tick.Volume);
        }

        [Fact]
        public async Task DividendTest()
        {
            var dividends = await YahooHistory.Symbols("AAPL").Period(new DateTime(2016, 2, 4), new DateTime(2016, 2, 5))
                .GetDividendsAsync().SingleAsync();
            Assert.Equal(0.52m, dividends[0].Dividend);
        }

        [Fact]
        public async Task SplitTest()
        {
            var splits = await YahooHistory.Symbols("AAPL").Period(new DateTime(2014, 6, 8), new DateTime(2014, 6, 10))
                .GetSplitsAsync().SingleAsync();
            Assert.Equal(7, splits[0].BeforeSplit);
            Assert.Equal(1, splits[0].AfterSplit);
        }

        [Fact]
        public async Task DatesTest_US()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var ticks = await YahooHistory.Symbols("C").Period(from, to)
                .GetHistoryAsync(Frequency.Daily).SingleAsync();

            Assert.Equal(from, ticks.First().DateTime);
            Assert.Equal(to,   ticks.Last() .DateTime);

            Assert.Equal(3, ticks.Count());
            Assert.Equal(75.18m, ticks[0].Close);
            Assert.Equal(74.940002m, ticks[1].Close);
            Assert.Equal(72.370003m, ticks[2].Close);
        }

        [Fact]
        public async Task DatesTest_UK()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var ticks = await YahooHistory.Symbols("BA.L").Period(from, to)
                .GetHistoryAsync(Frequency.Daily).SingleAsync();

            Assert.Equal(from, ticks.First().DateTime);
            Assert.Equal(to,   ticks.Last() .DateTime);

            Assert.Equal(3, ticks.Count());
            Assert.Equal(616.50m, ticks[0].Close);
            Assert.Equal(615.00m, ticks[1].Close);
            Assert.Equal(616.00m, ticks[2].Close);
        }

        [Fact]
        public async Task DatesTest_TW()
        {
            var from = new DateTime(2017, 10, 11);
            var to = new DateTime(2017, 10, 13);

            var ticks = await YahooHistory.Symbols("2498.TW").Period(from, to)
                .GetHistoryAsync(Frequency.Daily).SingleAsync();

            Assert.Equal(from, ticks.First().DateTime);
            Assert.Equal(to,   ticks.Last() .DateTime);

            Assert.Equal(3, ticks.Count());
            Assert.Equal(71.599998m, ticks[0].Close);
            Assert.Equal(71.599998m, ticks[1].Close);
            Assert.Equal(73.099998m, ticks[2].Close);
        }

        [Theory]
        [InlineData("SPY")] // USA
        [InlineData("TD.TO")] // Canada
        [InlineData("BP.L")] // London
        [InlineData("AIR.PA")] // Euronext
        [InlineData("AIR.DE")] // Xetra
        [InlineData("UNITECH.BO")] // Bombay
        [InlineData("2800.HK")] // Hong Kong
        [InlineData("000001.SS")] // Shanghai
        [InlineData("2448.TW")] // Taiwan
        [InlineData("005930.KS")] // Korea
        [InlineData("BHP.AX")] // Sydney
        public async Task DatesTest(params string[] symbols)
        {
            var from = new DateTime(2017, 9, 12);
            var to = from.AddDays(2);

            var ticks = await YahooHistory.Symbols(symbols).Period(from, to)
                .GetHistoryAsync().SingleAsync();

            Assert.Equal(from, ticks.First().DateTime);
            Assert.Equal(to,   ticks.Last() .DateTime);
            Assert.Equal(3, ticks.Count());
        }

        [Fact]
        public async Task CurrencyTest()
        {
            // Note: Forex seems to return date = (requested date - 1 day)
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var ticks = await YahooHistory.Symbols("EURUSD=X").Period(from, to)
                .GetHistoryAsync().SingleAsync();

            foreach (var tick in ticks)
                Write($"{tick.DateTime} {tick.Close}");

            Assert.Equal(from, ticks.First().DateTime.AddDays(1));
            Assert.Equal(to, ticks.Last().DateTime.AddDays(1));

            Assert.Equal(3, ticks.Count());
            Assert.Equal(1.174164m, ticks[0].Close);
            Assert.Equal(1.181488m, ticks[1].Close);
            Assert.Equal(1.186549m, ticks[2].Close);
        }

        [Fact]
        public async Task TestManySymbols()
        {
            var symbols = File.ReadAllLines(@"..\..\..\symbols.txt")
                .Where(line => !line.StartsWith("#"))
                .Take(100)
                .ToArray();

            var results = (await YahooHistory.Symbols(symbols).Period(DateTime.Now.AddDays(-10)).GetHistoryAsync()).ToList();
            int successes = 0, badSymbols = 0, collectionNodified = 0, other = 0;

            for (var i = 0 ; i < results.Count; i++)
            {
                var result = results[i];
                if (result.Task.Status == TaskStatus.RanToCompletion)
                {
                    successes++;
                    continue;
                }
                var message = result.Task.Exception.InnerException.Message;
                if (message.StartsWith("Invalid ticker or endpoint for symbol"))
                {
                    badSymbols++;
                    continue;
                }
                if (message.StartsWith("Call failed. Collection was modified"))
                {
                    // This is a bug in Flurl: https://github.com/tmenier/Flurl/issues/398
                    collectionNodified++;
                    continue;
                }
                other++;

                Write($"Symbol: {result.Symbol}({i}), Status: {result.Task.Status}, Exception.Message: {result.Task.Exception.InnerException.Message}");
            }

            Write("");
            Write($"Total Symbols: {results.Count}");
            Write($"Successes: {successes}");
            Write($"Bad Symbols: {badSymbols}");
            Write($"CollectionModified: {collectionNodified}");
            Write($"Other: {other}");
        }

        [Fact]
        public void TestDates()
        {
            var dt0 = new DateTime(2000, 1, 1, 14, 24, 0, DateTimeKind.Utc);
            var seconds0 = new DateTimeOffset(dt0).ToUnixTimeSeconds();

            var dt1 = new DateTime(2000, 1, 1, 22, 24, 0, DateTimeKind.Local); // DateTimeKind.Unspecified => DateTimeKind.Local
            var seconds = new DateTimeOffset(dt1).ToUnixTimeSeconds(); // first converted to UTC

            //new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc)).ToUnixTimeSeconds().ToString("F0");
            var dt2 = DateTimeOffset.FromUnixTimeSeconds(seconds0).UtcDateTime; // assumes UTC
            var dt3 = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime; // assumes UTC
            ;
        }
    }
}

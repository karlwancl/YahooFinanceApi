using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace YahooFinanceApi.Tests
{
    public class Historical
    {
        private readonly Action<string> Write;
        public Historical(ITestOutputHelper output) => Write = output.WriteLine;

        [Fact]
        public async Task InvalidSymbolTest()
        {
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await Yahoo.GetHistoricalAsync("invalidSymbol", new DateTime(2017, 1, 3), new DateTime(2017, 1, 4)));

            Write(exception.ToString());

            Assert.Contains("Not Found", exception.InnerException.Message);
        }

        [Fact]
        public void PeriodTest()
        {
            var periods = Enum.GetValues(typeof(Period)).Cast<Period>();

            var dict = new Dictionary<Period, decimal>
            {
                {Period.Daily, 115.800003m},
                {Period.Weekly, 115.800003m},
                {Period.Monthly, 115.800003m}
            };

            periods.ToList().ForEach(async p =>
            {
                var results = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2017, 1, 3), DateTime.Now, p);
                Assert.Equal(dict[p], results.First().Open);
            });
        }

        [Fact]
        public async Task HistoricalTest()
        {
            var results = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2017, 1, 3), new DateTime(2017, 1, 4), Period.Daily);
            var first = results.First();

            Assert.Equal(115.800003m, first.Open);
            Assert.Equal(116.330002m, first.High);
            Assert.Equal(114.760002m, first.Low);
            Assert.Equal(116.150002m, first.Close);
            Assert.Equal(28_781_900, first.Volume);
        }

        [Fact]
        public async Task DividendTest()
        {
            var results = await Yahoo.GetDividendsAsync("AAPL", new DateTime(2016, 2, 4), new DateTime(2016, 2, 5));

            Assert.Equal(0.52m, results.First().Dividend);
        }

        [Fact]
        public async Task SplitTest()
        {
            var results = await Yahoo.GetSplitsAsync("AAPL", new DateTime(2014, 6, 8), new DateTime(2014, 6, 10));
            var first = results.First();

            Assert.Equal(1, first.BeforeSplit);
            Assert.Equal(7, first.AfterSplit);
        }

        [Fact]
        public async Task DatesTest_US()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var results = await Yahoo.GetHistoricalAsync("C", from, to, Period.Daily);

            Assert.Equal(3, results.Count());

            Assert.Equal(from, results.First().DateTime);
            Assert.Equal(to, results.Last().DateTime);

            Assert.Equal(75.18m, results[0].Close);
            Assert.Equal(74.940002m, results[1].Close);
            Assert.Equal(72.370003m, results[2].Close);
        }

        [Fact]
        public async Task Test_UK()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var results = await Yahoo.GetHistoricalAsync("BA.L", from, to, Period.Daily);

            Assert.Equal(3, results.Count());

            Assert.Equal(from, results.First().DateTime);
            Assert.Equal(to, results.Last().DateTime);

            // Don't know why Yahoo changes its historical data, the test is not passed before these changes
            Assert.Equal(616.50m, results[0].Close);
            Assert.Equal(615.00m, results[1].Close);
            Assert.Equal(616.00m, results[2].Close);
        }

        [Fact]
        public async Task DatesTest_TW()
        {
            var from = new DateTime(2017, 10, 11);
            var to = new DateTime(2017, 10, 13);

            var results = await Yahoo.GetHistoricalAsync("2498.TW", from, to, Period.Daily);

            Assert.Equal(3, results.Count());

            Assert.Equal(from, results.First().DateTime);
            Assert.Equal(to, results.Last().DateTime);

            Assert.Equal(71.599998m, results[0].Close);
            Assert.Equal(71.599998m, results[1].Close);
            Assert.Equal(73.099998m, results[2].Close);
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

            // start tasks
            var tasks = symbols.Select(symbol => Yahoo.GetHistoricalAsync(symbol, from, to));

            // wait for all tasks to complete
            var results = await Task.WhenAll(tasks.ToArray());

            foreach (var result in results)
            {
                Assert.Equal(3, result.Count());

                Assert.Equal(from, result.First().DateTime);
                Assert.Equal(to, result.Last().DateTime);
            }
        }

        [Fact]
        public async Task TestLatest()
        {
            var results = await Yahoo.GetHistoricalAsync("C", DateTime.Now.AddDays(-7));
            foreach (var result in results)
                Write($"{result.DateTime} {result.Close}");
        }

        [Fact]
        public async Task CurrencyTest()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var results = await Yahoo.GetHistoricalAsync("EURUSD=X", from, to);

            foreach (var result in results)
                Write($"{result.DateTime} {result.Close}");

            Assert.Equal(3, results.Count());

            Assert.Equal(1.174164m, results[0].Close);
            Assert.Equal(1.181488m, results[1].Close);
            Assert.Equal(1.186549m, results[2].Close);

            // Note: Forex seems to return date = (requested date - 1 day)
            Assert.Equal(from, results.First().DateTime.AddDays(1));
            Assert.Equal(to, results.Last().DateTime.AddDays(1));
        }
    }
}

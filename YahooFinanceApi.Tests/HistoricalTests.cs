using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace YahooFinanceApi.Tests
{
    public class HistoricalTests
    {
        private readonly Action<string> Write;
        public HistoricalTests(ITestOutputHelper output)
        {
            Write = output.WriteLine;
        } 

        [Fact]
        public async Task InvalidSymbolTest()
        {
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await Yahoo.GetHistoricalAsync("invalidSymbol", new DateTime(2017, 1, 3), new DateTime(2017, 1, 4)));

            Write(exception.ToString());

            Assert.Contains("Not Found", exception.InnerException.Message);
        }

        [Fact]
        public async Task PeriodTest()
        {
            var date = new DateTime(2023, 1, 9);

            var candles = await Yahoo.GetHistoricalAsync("AAPL", date, date.AddDays(1), Period.Daily);
            Assert.Equal(130.470001m, candles.First().Open);

            candles = await Yahoo.GetHistoricalAsync("AAPL", date, date.AddDays(7), Period.Weekly);
            Assert.Equal(130.470001m, candles.First().Open);

            candles = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2023, 1, 1), new DateTime(2023, 2, 1), Period.Monthly);
            Assert.Equal(130.279999m, candles.First().Open);
        }

        [Fact]
        public async Task HistoricalTest()
        {
            var candles = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2023, 1, 3), new DateTime(2023, 1, 4), Period.Daily);

            var candle = candles.First();
            Assert.Equal(130.279999m, candle.Open);
            Assert.Equal(130.899994m, candle.High);
            Assert.Equal(124.169998m, candle.Low);
            Assert.Equal(125.070000m, candle.Close);
            Assert.Equal(112_117_500, candle.Volume);
        }

        [Fact]
        public async Task DividendTest()
        {
            var dividends = await Yahoo.GetDividendsAsync("AAPL", new DateTime(2016, 2, 4), new DateTime(2016, 2, 5));
            Assert.Equal(0.130000m, dividends.First().Dividend);
        }

        [Fact]
        public async Task SplitTest()
        {
            var splits = await Yahoo.GetSplitsAsync("AAPL", new DateTime(2014, 6, 1), new DateTime(2014, 6, 15));

            Assert.Equal(7, splits.First().BeforeSplit);
            Assert.Equal(1, splits.First().AfterSplit);
        }

        [Fact]
        public async Task DatesTest_US()
        {
            var from = new DateTime(2017, 10, 10);
            var to   = new DateTime(2017, 10, 12).AddHours(12);

            var candles = await Yahoo.GetHistoricalAsync("C", from, to, Period.Daily);

            Assert.Equal(3, candles.Count());

            Assert.Equal(from, candles.First().DateTime);
            Assert.Equal(to.Date,   candles.Last().DateTime);

            Assert.Equal(75.18m,     candles[0].Close);
            Assert.Equal(74.940002m, candles[1].Close);
            Assert.Equal(72.370003m, candles[2].Close);
        }

        [Fact]
        public async Task Test_UK()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 13);

            var candles = await Yahoo.GetHistoricalAsync("BA.L", from, to, Period.Daily);

            Assert.Equal(3, candles.Count());

            Assert.Equal(from, candles.First().DateTime);
            Assert.Equal(to,   candles.Last().DateTime.AddDays(1));

            Assert.Equal(616.50m, candles[0].Close);
            Assert.Equal(615.00m, candles[1].Close);
            Assert.Equal(616.00m, candles[2].Close);
        }

        [Fact]
        public async Task DatesTest_TW()
        {
            var from = new DateTime(2017, 10, 11);
            var to = new DateTime(2017, 10, 13);

            var candles = await Yahoo.GetHistoricalAsync("2498.TW", from, to, Period.Daily);

            Assert.Equal(3, candles.Count());

            Assert.Equal(from, candles.First().DateTime);
            Assert.Equal(to,   candles.Last().DateTime);

            Assert.Equal(71.599998m, candles[0].Close);
            Assert.Equal(71.599998m, candles[1].Close);
            Assert.Equal(73.099998m, candles[2].Close);
        }

        [Theory]
        [InlineData("SPY")] // USA
        [InlineData("TD.TO")] // Canada
        [InlineData("BP.L")] // London
        [InlineData("AIR.PA")] // Euronext
        [InlineData("AIR.DE")] // Xetra
        [InlineData("UNITECH.BO")] // Bombay
        [InlineData("0388.HK")] // Hong Kong
        [InlineData("000001.SS")] // Shanghai
        [InlineData("2330.TW")] // Taiwan
        [InlineData("005930.KS")] // Korea
        [InlineData("BHP.AX")] // Sydney
        public async Task DatesTest(params string[] symbols)
        {
            var from = new DateTime(2023, 1, 3);
            var to = from.AddDays(2).AddHours(12);

            // start tasks
            var tasks = symbols.Select(symbol => Yahoo.GetHistoricalAsync(symbol, from, to));

            // wait for all tasks to complete
            var results = await Task.WhenAll(tasks.ToArray());

            foreach (var candles in results)
            {
                Assert.Equal(3, candles.Count);

                Assert.Equal(from.Date, candles.First().DateTime);
                Assert.Equal(to.Date,   candles.Last().DateTime);
            }
        }

        [Fact]
        public async Task TestLatest()
        {
            var candles = await Yahoo.GetHistoricalAsync("C", DateTime.Now.AddDays(-7));
            foreach (var candle in candles)
                Write($"{candle.DateTime} {candle.Close}");
        }

        [Fact]
        public async Task CurrencyTest()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var candles = await Yahoo.GetHistoricalAsync("EURUSD=X", from, to);

            foreach (var candle in candles)
                Write($"{candle.DateTime} {candle.Close}");

            Assert.Equal(3, candles.Count());

            Assert.Equal(1.174164m, candles[0].Close);
            Assert.Equal(1.181488m, candles[1].Close);
            Assert.Equal(1.186549m, candles[2].Close);

            // Note: Forex seems to return date = (requested date - 1 day)
            Assert.Equal(from, candles.First().DateTime);
            Assert.Equal(to, candles.Last().DateTime);
        }
    }
}

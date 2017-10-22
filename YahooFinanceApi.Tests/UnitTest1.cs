using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace YahooFinanceApi.Tests
{
    public class UnitTest1
    {
        public UnitTest1()
        {
            // Test culture invariant
			CultureInfo.CurrentCulture = new CultureInfo("nl-nl");
        }

        [Fact]
        public async Task HistoricalExceptionTest()
        {
            var exception = await Assert.ThrowsAsync<Exception>(async() =>
                await Yahoo.GetHistoricalAsync("aapl1234", new DateTime(2017, 1, 3), new DateTime(2017, 1, 4)));
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
                var hist = await Yahoo.GetHistoricalAsync("aapl", new DateTime(2017, 1, 3), DateTime.Now, p);
                Assert.Equal(dict[p], hist.First().Open);
            });
        }

		[Fact]
		public async Task HistoricalTest()
		{
            var hist0 = await Yahoo.GetHistoricalAsync("aapl", new DateTime(2017, 1, 3), new DateTime(2017, 1, 4), Period.Daily);
            var hist = hist0.First();
			Assert.Equal(115.800003m, hist.Open);
			Assert.Equal(116.330002m, hist.High);
			Assert.Equal(114.760002m, hist.Low);
			Assert.Equal(116.150002m, hist.Close);
			Assert.Equal(114.722694m, hist.AdjustedClose);
			Assert.Equal(28_781_900, hist.Volume);
		}

        [Fact]
        public async Task DividendTest()
        {
            var hist = await Yahoo.GetDividendsAsync("aapl", new DateTime(2016, 2, 4), new DateTime(2016, 2, 5));
            Assert.Equal(0.52m, hist.First().Dividend);
        }

        [Fact]
        public async Task SplitTest()
        {
            var hist = await Yahoo.GetSplitsAsync("aapl", new DateTime(2014, 6, 8), new DateTime(2014, 6, 10));

            Assert.Equal(1, hist.First().BeforeSplit);
            Assert.Equal(7, hist.First().AfterSplit);
        }

        [Fact]
        public async Task QuoteTest()
        {
			const string aaplTag = "aapl";

            var tags = Enum.GetValues(typeof(Tag)).Cast<Tag>();
            var quote = await Yahoo.Symbol(aaplTag).Tag(tags.ToArray()).GetAsync();
            var aapl = quote[aaplTag];

            Assert.Equal("Apple Inc.", aapl[Tag.Name]);
            tags.ToList().ForEach(t => Assert.True(aapl.ContainsKey(t)));
		}

        [Fact]
        public async Task ParallelTest()
        {
            // start 100 tasks
            var tasks = Enumerable
                .Range(1, 100)
                .Select(x => Yahoo.GetHistoricalAsync("AAPL", new DateTime(2017, 1, 3), new DateTime(2017, 1, 10), Period.Daily))
                .ToArray();

            // wait for tasks to finish
            await Task.WhenAll(tasks);

            Assert.True(tasks
                .Select(task => task.Result)
                .All(hist => hist.First().Volume == 28_781_900));
        }

        [Fact]
        public async Task HistoricalDatesTest_US()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var hist = await Yahoo.GetHistoricalAsync("C", from, to, Period.Daily);

            Assert.Equal(3, hist.Count());

            Assert.Equal(from, hist.First().DateTime);
            Assert.Equal(to, hist.Last().DateTime);

            Assert.Equal(75.18m,     hist[0].AdjustedClose);
            Assert.Equal(74.940002m, hist[1].AdjustedClose);
            Assert.Equal(72.370003m, hist[2].AdjustedClose);
        }

        [Fact]
        public async Task HistoricalDatesTest_UK()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var hist = await Yahoo.GetHistoricalAsync("BA.L", from, to, Period.Daily);

            Assert.Equal(3, hist.Count());

            Assert.Equal(from, hist.First().DateTime);
            Assert.Equal(to, hist.Last().DateTime);

            // Don't know why Yahoo changes its historical data, the test is not passed before these changes
            Assert.Equal(607.68573m, hist[0].AdjustedClose);
            Assert.Equal(606.207153m, hist[1].AdjustedClose);
            Assert.Equal(607.192871m, hist[2].AdjustedClose);
        }

        [Fact]
        public async Task HistoricalDatesTest_TW()
        {
            var from = new DateTime(2017, 10, 11);
            var to = new DateTime(2017, 10, 13);

            var hist = await Yahoo.GetHistoricalAsync("2498.TW", from, to, Period.Daily);

            Assert.Equal(3, hist.Count());

            Assert.Equal(from, hist.First().DateTime);
            Assert.Equal(to, hist.Last().DateTime);

            Assert.Equal(71.599998m, hist[0].AdjustedClose);
            Assert.Equal(71.599998m, hist[1].AdjustedClose);
            Assert.Equal(73.099998m, hist[2].AdjustedClose);
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
        public async Task HistoricalDatesTest(params string[] symbols)
        {
            var from = new DateTime(2017, 9, 12);
            var to = from.AddDays(2);

            foreach (var symbol in symbols)
            {
                var hist = await Yahoo.GetHistoricalAsync(symbol, from, to, Period.Daily);

                Assert.Equal(3, hist.Count());

                Assert.Equal(from, hist.First().DateTime);
                Assert.Equal(to, hist.Last().DateTime);
            }
        }

        /*
        [Fact]
        public void HistoricalCurrencyTest()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var hist = Yahoo.GetHistoricalAsync("EURUSD=X", from, to, Period.Daily, ascending: true).Result;

            Assert.Equal(3, hist.Count());

            Assert.Equal(1.174164m, hist[0].AdjustedClose);
            Assert.Equal(1.181488m, hist[1].AdjustedClose);
            Assert.Equal(1.186549m, hist[2].AdjustedClose);

            Assert.Equal(from, hist.First().DateTime);
            Assert.Equal(to, hist.Last().DateTime);
        }
        */
    }
}

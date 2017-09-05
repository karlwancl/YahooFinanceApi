using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YahooFinanceApi.Lookup;

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
        public void SymbolListTest()
        {
            const string Symbol = "aap";
            var symbols = YahooLookup.GetLookupSymbolsAsync(Symbol, LookupType.Stocks, MarketType.US_Canada).Result;
            Assert.True(symbols.Any(s => s.CompanyName == "Advance Auto Parts, Inc."));
        }

        [Fact]
        public void HistoricalExceptionTest()
        {
            Action @delegate = () => Yahoo.GetHistoricalAsync("aapl1234", new DateTime(2017, 1, 3), new DateTime(2017, 1, 4)).Wait();
            var aggregateEx = Assert.Throws<AggregateException>(@delegate);
            var innerEx = Assert.IsType<Exception>(aggregateEx.InnerException);
            Assert.Contains("invalid ticker", innerEx.Message);
        }

        [Fact]
        public void PeriodTest()
        {
			const string aaplTag = "aapl";

            var periods = Enum.GetValues(typeof(Period)).Cast<Period>();
            var dict = new Dictionary<Period, decimal>
            {
                {Period.Daily, 115.800003m},
                {Period.Weekly, 115.800003m},
                {Period.Monthly, 115.800003m}
            };
            periods.ToList().ForEach(p =>
            {
                var hist = Yahoo.GetHistoricalAsync(aaplTag, new DateTime(2017, 1, 3), DateTime.Now, p).Result.First();
                Assert.Equal(dict[p], hist.Open);
            });
        }

		[Fact]
		public void HistoricalTest()
		{
			const string aaplTag = "aapl";

            var hist0 = Yahoo.GetHistoricalAsync(aaplTag, new DateTime(2017, 1, 3), new DateTime(2017, 1, 4), Period.Daily).Result;
            var hist = hist0.First();
			Assert.Equal(115.800003m, hist.Open);
			Assert.Equal(116.330002m, hist.High);
			Assert.Equal(114.760002m, hist.Low);
			Assert.Equal(116.150002m, hist.Close);
			Assert.Equal(114.722694m, hist.AdjustedClose);
			Assert.Equal(28_781_900, hist.Volume);
		}

        [Fact]
        public void DividendTest()
        {
            const string aaplTag = "aapl";

            var hist = Yahoo.GetDividendsAsync(aaplTag, new DateTime(2016, 2, 4), new DateTime(2016, 2, 5)).Result.First();
            Assert.Equal(0.52m, hist.Dividend);
        }

        [Fact]
        public void SplitTest()
        {
			const string aaplTag = "aapl";

            var hist = Yahoo.GetSplitsAsync(aaplTag, new DateTime(2014, 6, 8), new DateTime(2014, 6, 10)).Result.First();
            Assert.Equal(1, hist.BeforeSplit);
            Assert.Equal(7, hist.AfterSplit);
        }

        [Fact]
        public void QuoteTest()
        {
			const string aaplTag = "aapl";

            var tags = Enum.GetValues(typeof(Tag)).Cast<Tag>();
            var quote = Yahoo.Symbol(aaplTag).Tag(tags.ToArray()).GetAsync().Result;
            var aapl = quote[aaplTag];

            Assert.Equal("Apple Inc.", aapl[Tag.Name]);
            tags.ToList().ForEach(t => Assert.True(aapl.ContainsKey(t)));
		}

        [Fact]
        public void ParallelTest()
        {
			const string aaplTag = "aapl";

            Parallel.For(0, 10, n =>
            {
                var hist = Yahoo.GetHistoricalAsync(aaplTag, new DateTime(2017, 1, 3), new DateTime(2017, 1, 4), Period.Daily).Result.First();
				Assert.Equal(28_781_900, hist.Volume);
			});
        }

        [Fact]
        public void HistoricalDatesTest_US()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var hist = Yahoo.GetHistoricalAsync("C", from, to, Period.Daily).Result;

            Assert.Equal(3, hist.Count());

            Assert.Equal(from, hist.First().DateTime);
            Assert.Equal(to, hist.Last().DateTime);

            Assert.Equal(75.18m,     hist[0].AdjustedClose);
            Assert.Equal(74.940002m, hist[1].AdjustedClose);
            Assert.Equal(72.370003m, hist[2].AdjustedClose);
        }

        [Fact]
        public void HistoricalDatesTest_UK()
        {
            var from = new DateTime(2017, 10, 10);
            var to = new DateTime(2017, 10, 12);

            var hist = Yahoo.GetHistoricalAsync("BA.L", from, to, Period.Daily).Result;

            Assert.Equal(3, hist.Count());

            Assert.Equal(from, hist.First().DateTime);
            Assert.Equal(to, hist.Last().DateTime);

            // Don't know why Yahoo changes its historical data, the test is not passed before these changes
            Assert.Equal(607.68573m, hist[0].AdjustedClose);
            Assert.Equal(606.207153m, hist[1].AdjustedClose);
            Assert.Equal(607.192871m, hist[2].AdjustedClose);
        }

        [Fact]
        public void HistoricalDatesTest_TW()
        {
            var from = new DateTime(2017, 10, 11);
            var to = new DateTime(2017, 10, 13);

            var hist = Yahoo.GetHistoricalAsync("2498.TW", from, to, Period.Daily).Result;

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
        public void HistoricalDatesTest(params string[] symbols)
        {
            var from = new DateTime(2017, 9, 12);
            var to = from.AddDays(2);

            foreach (var symbol in symbols)
            {
                var hist = Yahoo.GetHistoricalAsync(symbol, from, to, Period.Daily).Result;

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

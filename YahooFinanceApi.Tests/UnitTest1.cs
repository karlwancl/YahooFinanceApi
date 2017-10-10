using System;
using System.Collections.Generic;
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
                var hist = Yahoo.GetHistoricalAsync(aaplTag, new DateTime(2017, 1, 3), DateTime.Now, p, true).Result.First();
                Assert.Equal(dict[p], hist.Open);
            });
        }

		[Fact]
		public void HistoricalTest()
		{
			const string aaplTag = "aapl";

            var hist0 = Yahoo.GetHistoricalAsync(aaplTag, new DateTime(2017, 1, 3), new DateTime(2017, 1, 4), Period.Daily, true, timeZone: "America/New_York").Result;
            var hist = hist0.First();
			Assert.Equal(115.800003m, hist.Open);
			Assert.Equal(116.330002m, hist.High);
			Assert.Equal(114.760002m, hist.Low);
			Assert.Equal(116.150002m, hist.Close);
			Assert.Equal(114.722694m, hist.AdjustedClose);
			Assert.Equal(28_781_900, hist.Volume);
		}

        [Fact]
        public void TimeZoneTest()
        {
            const string rxpaxTag = "rxp.ax";

            var hist0 = Yahoo.GetHistoricalAsync(rxpaxTag, new DateTime(2017, 1, 4), new DateTime(2017, 1, 5), Period.Daily, true, timeZone: "Australia/Sydney").Result;
            var hist = hist0.First();
			Assert.Equal(0.965m, hist.Open);
			Assert.Equal(0.965m, hist.High);
			Assert.Equal(0.93m, hist.Low);
			Assert.Equal(0.935m, hist.Close);
			Assert.Equal(0.863688m, hist.AdjustedClose);
			Assert.Equal(146_514, hist.Volume);
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
                var hist = Yahoo.GetHistoricalAsync(aaplTag, new DateTime(2017, 1, 3), new DateTime(2017, 1, 4), Period.Daily, true).Result.First();
				Assert.Equal(28_781_900, hist.Volume);
			});
        }
    }
}

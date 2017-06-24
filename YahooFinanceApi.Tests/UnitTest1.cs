using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace YahooFinanceApi.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void PeriodTest()
        {
            const string aaplTag = "aapl";

            var periods = Enum.GetValues(typeof(Period)).Cast<Period>();
            var dict = new Dictionary<Period, decimal>
            {
                {Period.Daily, 115.800003m},
                {Period.Weekly, 117.949997m},
                {Period.Monthly, 127.029999m}
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

			var hist = Yahoo.GetHistoricalAsync(aaplTag, new DateTime(2017, 1, 3), new DateTime(2017, 1, 4), Period.Daily, true).Result.First();
			Assert.Equal(115.800003m, hist.Open);
			Assert.Equal(116.330002m, hist.High);
			Assert.Equal(114.760002m, hist.Low);
			Assert.Equal(116.150002m, hist.Close);
			Assert.Equal(115.173210m, hist.AdjustedClose);
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
                var hist = Yahoo.GetHistoricalAsync(aaplTag, new DateTime(2017, 1, 3), new DateTime(2017, 1, 4), Period.Daily, true).Result.First();
				Assert.Equal(28_781_900, hist.Volume);
			});
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace YahooFinanceApi.Tests
{
    public class Quotes
    {
        protected readonly Action<string> Write;
        public Quotes(ITestOutputHelper output) => Write = output.WriteLine;

        static Quotes() // static ctor
        {
            // Test culture
            CultureInfo.CurrentCulture = new CultureInfo("nl-nl");

            // may speed up http: implemented by application
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.DefaultConnectionLimit = 1000;
        }

        [Fact]
        public async Task Test1()
        {
            const string symbol = "AAPL";

            var tags = Enum.GetValues(typeof(Tag)).Cast<Tag>();
            var results = await Yahoo.Symbol(symbol).Tag(tags.ToArray()).GetAsync();
            var result = results[symbol];

            Assert.Equal("Apple Inc.", result[Tag.Name]);

            foreach (var tag in tags)
            {
                Assert.True(result.ContainsKey(tag));
                Write($"{tag} {result[tag]}");
            }
        }
    }
}

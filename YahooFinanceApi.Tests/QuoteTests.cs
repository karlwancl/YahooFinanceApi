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
        public async Task TestYahooQuery()
        {
            IDictionary<string, IDictionary<string, dynamic>> securities =
                await Yahoo
                    .Symbols("C", "AAPL")
                    .Fields("regularMarketPrice", "bid", "ask", "longName", "tradeable")
                    .QueryAsync();

            Assert.Equal(2, securities.Count());

            dynamic bid = securities["C"]["bid"];

            dynamic x = 2 * bid; // bid must be a numeric type

            double ask = securities["C"]["ask"]; // ask must be double

            Assert.True(securities["C"]["tradeable"]); // inferred type

            Assert.Equal("Apple Inc.", securities["AAPL"]["longName"]); // inferred type
        }

        [Fact]
        public async Task TestYahooQueryArguments()
        {
            // no symbols
            await Assert.ThrowsAsync<ArgumentException>(async () => await Yahoo.Symbols().QueryAsync());
            
            // symbol not found
            await Assert.ThrowsAsync<Flurl.Http.FlurlHttpException>(async () => await Yahoo.Symbols("invalidsymbol").QueryAsync());

            // invalid field has no effect!
            await Yahoo.Symbols("C").Fields("invalidfield").QueryAsync();

            // when no fields are specified, some default fields are returned
            await Yahoo.Symbols("C").QueryAsync();

        }

    }
}

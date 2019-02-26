using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace YahooFinanceApi.Tests
{
    public class YahooQuotesTest
    {
        [Fact]
        public async Task TestSymbolsArgument()
        {
            // no symbols
            await Assert.ThrowsAsync<ArgumentException>(async () => await YahooQuotes.Symbols().GetAsync());

            // some symbols
            var securities = await YahooQuotes.Symbols("C", "AAPL").GetAsync();
            Assert.Equal(2, securities.Count);

            // duplicate symbol
            await Assert.ThrowsAsync<ArgumentException>(async () => await YahooQuotes.Symbols("C", "A", "C").GetAsync());

            // invalid symbols are ignored by Yahoo!
            securities = await YahooQuotes.Symbols("invalidsymbol").GetAsync();
            Assert.Empty(securities);

            // invalid symbols are ignored by Yahoo!
            securities = await YahooQuotes.Symbols("C", "invalidsymbol", "X").GetAsync();
            Assert.Equal(2, securities.Count);
        }

        [Fact]
        public async Task TestFieldsArgument()
        {
            // when no fields are specified, many(all?) fields are returned!
            var securities = await YahooQuotes.Symbols("C").GetAsync();
            Assert.True(securities["C"].Fields.Count > 10);

            // duplicate field
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await YahooQuotes.Symbols("C").Fields("currency", "bid").Fields(Field.Ask, Field.Bid).GetAsync());

            // invalid fields are ignored
            securities = await YahooQuotes.Symbols("C").Fields("invalidfield").GetAsync();
            var security = securities["C"];
            Assert.Throws<KeyNotFoundException>(() => security["invalidfield"]);
            Assert.False(security.Fields.TryGetValue("invalidfield", out dynamic bid));
        }

        [Fact]
        public async Task TestQuery()
        {
            var securities = await YahooQuotes
                .Symbols("C", "AAPL")
                // Can use string field names:
                .Fields("Bid", "Ask", "Tradeable", "LongName")
                // and/or field enums:
                .Fields(Field.RegularMarketPrice, Field.Currency)
                .GetAsync();

            Assert.Equal(2, securities.Count());
            var security = securities["C"];

            // Bid string or enum indexer returns dynamic type.
            security.Fields.TryGetValue("Bid", out dynamic bid);
            bid = security.Fields["Bid"];
            bid = security["Bid"];
            bid = security[Field.Bid];

            // Bid property returns static type.
            var bid2 = security.Bid;

            Assert.True(securities["C"][Field.Tradeable]);
            Assert.Equal("Apple Inc.", securities["AAPL"]["LongName"]);
        }

        [Fact]
        public async Task TestManySymbols()
        {
            var symbols = File.ReadAllLines(@"..\..\..\symbols.txt")
                .Where(line => !line.StartsWith("#"))
                .Take(100)
                .ToArray();

            // The length limit for a URI depends on the browser and is around 5,000 characters.
            // Exception.Message.StartsWith("Invalid URI: The Uri string is too long.");
            // Flurl could support longer URLs but it does not.

            var securities = await YahooQuotes.Symbols(symbols).GetAsync();
        }

        [Fact]
        public async Task TestRegularMarketTime()
        {
            //string symbol = "BBRY"; // changed symbol is sometimes ignored!
            string symbol = "ORCL";

            IReadOnlyDictionary<string, Security> securities = await YahooQuotes.Symbols(symbol).GetAsync();
            Security security = securities[symbol];

            // RegularMarketTime is probably the time of the last price. 
            // When not during regular market hours, the time is the time og the last trade (when RTH closed)
            DateTime utc = security.RegularMarketTime.FromUnixTimeSeconds();
            var est = utc.ToLocalTimeIn("Eastern Standard Time");

            var exchange = security.ExchangeTimezoneName;

            ;
        }

    }
}

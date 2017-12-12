using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace YahooFinanceApi.Tests
{
    public class QuotesTest
    {
        [Fact]
        public async Task TestSymbolsArgument()
        {
            // one symbols
            var result = await Yahoo.Symbols("C").QueryAsync();
            Assert.Single(result);

            // no symbols
            await Assert.ThrowsAsync<ArgumentException>(async () => await Yahoo.Symbols().QueryAsync());

            // duplicate symbol
            await Assert.ThrowsAsync<ArgumentException>(async () => await Yahoo.Symbols("C", "A", "C").QueryAsync());

            // invalid symbols are ignored by Yahoo!
            result = await Yahoo.Symbols("invalidsymbol").QueryAsync();
            Assert.Empty(result);

            result = await Yahoo.Symbols("C", "invalidsymbol", "X").QueryAsync();
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task TestFieldsArgument()
        {
            // when no fields are specified, many(all?) fields are returned!
            var result = await Yahoo.Symbols("C").QueryAsync();
            Assert.True(result["C"].Fields.Count > 10);

            // duplicate field
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await Yahoo.Symbols("C").Fields("currency", "bid").Fields(Field.Ask, Field.Bid).QueryAsync());

            // invalid fields are ignored
            result = await Yahoo.Symbols("C").Fields("invalidfield").QueryAsync();
            var security = result["C"];
            Assert.Throws<KeyNotFoundException>(() => security["invalidfield"]);
            Assert.False(security.Fields.TryGetValue("invalidfield", out dynamic bid));
        }

        [Fact]
        public async Task TestQuery()
        {
            var securities = await Yahoo
                .Symbols("C", "AAPL")
                // Can use string field names:
                .Fields("Bid", "Ask", "Tradeable", "LongName")
                // and/or field enums:
                .Fields(Field.RegularMarketPrice, Field.Currency)
                .QueryAsync();

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

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace YahooFinanceApi.Tests
{
    public class QuotesTest
    {
        [Fact]
        public async Task TestQuoteAsync()
        {
            const string AAPL = "AAPL";

            var quote = await Yahoo.Symbols(AAPL).Fields(
                Field.RegularMarketOpen,
                Field.RegularMarketDayHigh,
                Field.RegularMarketDayLow,
                Field.RegularMarketPrice,
                Field.RegularMarketVolume,
                Field.RegularMarketTime)
                .QueryAsync();

            var aaplQuote = quote[AAPL];
            var aaplOpen = aaplQuote[Field.RegularMarketOpen.ToString()];
            var aaplHigh = aaplQuote[Field.RegularMarketDayHigh.ToString()];
            var aaplLow = aaplQuote[Field.RegularMarketDayLow.ToString()];
            var aaplCurrentPrice = aaplQuote[Field.RegularMarketPrice.ToString()];
            var aaplVolume = aaplQuote[Field.RegularMarketVolume.ToString()];
            var aaplTime = aaplQuote[Field.RegularMarketTime.ToString()];

            // Get New York Timezone for conversion from UTC to New York Time for Yahoo Quotes
            TimeZoneInfo TzEst = TimeZoneInfo
                .GetSystemTimeZones()
                .Single(tz => tz.Id == "Eastern Standard Time" || tz.Id == "America/New_York");

            long unixDate = 1568232001; // Any unix timestamp
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddSeconds(unixDate);
            DateTime estDate = TimeZoneInfo.ConvertTimeFromUtc(date, TzEst);
        }

        [Fact]
        public async Task TestSymbolsArgument()
        {
            // one symbols
            var securities = await Yahoo.Symbols("C").QueryAsync();
            Assert.Single(securities);

            // no symbols
            await Assert.ThrowsAsync<ArgumentException>(async () => await Yahoo.Symbols().QueryAsync());

            // duplicate symbol
            await Assert.ThrowsAsync<ArgumentException>(async () => await Yahoo.Symbols("C", "A", "C").QueryAsync());

            // invalid symbols are ignored by Yahoo!
            securities = await Yahoo.Symbols("invalidsymbol").QueryAsync();
            Assert.Empty(securities);

            securities = await Yahoo.Symbols("C", "invalidsymbol", "X").QueryAsync();
            Assert.Equal(2, securities.Count);
        }

        [Fact]
        public async Task TestFieldsArgument()
        {
            // when no fields are specified, many(all?) fields are returned!
            var securities = await Yahoo.Symbols("C").QueryAsync();
            Assert.True(securities["C"].Fields.Count > 10);

            // duplicate field
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await Yahoo.Symbols("C").Fields("currency", "bid").Fields(Field.Ask, Field.Bid).QueryAsync());

            // invalid fields are ignored
            securities = await Yahoo.Symbols("C").Fields("invalidfield").QueryAsync();
            var security = securities["C"];
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

using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace YahooFinanceApi.Tests
{
    public class YahooQuotesTest
    {
        private readonly Action<string> Write;
        public YahooQuotesTest(ITestOutputHelper output) => Write = output.WriteLine;

        [Fact]
        public async Task SimpleQuery()
        {
            // one symbol
            Security security = await new YahooQuotes().GetAsync("C");
            Assert.NotNull(security.LongName);

            // invalid symbol
            Assert.Null(await new YahooQuotes().GetAsync("invalidSymbol"));

            // list of symbols
            Dictionary<string, Security> securities = await new YahooQuotes().GetAsync(new List<string>() { "C", "invalidSymbol" });
            security = securities["C"];
            Assert.True(security.RegularMarketPrice > 0);
            Assert.Null(securities["invalidSymbol"]);
        }

        [Fact]
        public async Task TestSymbolArgument()
        {
            // null symbol
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await new YahooQuotes().GetAsync((string)null));

            // no symbol
            await Assert.ThrowsAsync<ArgumentException>(async () => await new YahooQuotes().GetAsync(""));

            // null symbols
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await new YahooQuotes().GetAsync(new string[] { null }));

            // empty symbols
            await Assert.ThrowsAsync<ArgumentException>(async () => await new YahooQuotes().GetAsync(new[] { "" }));

            // duplicate symbol
            await Assert.ThrowsAsync<ArgumentException>(async () => await new YahooQuotes().GetAsync(new[] { "C", "A", "C" }));
        }

        [Fact]
        public async Task TestFieldsArgument()
        {
            Security security = await new YahooQuotes()
                // can use string field names:
                .Fields("Bid", "Ask", "Tradeable", "LongName")
                // and/or field enums:
                .Fields(Field.RegularMarketPrice, Field.Currency)
                .GetAsync("C");

            // when no fields are specified, many(all?) fields are returned!
            security = await new YahooQuotes().GetAsync("C");
            Assert.True(security.Fields.Count > 10);

            // duplicate field
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await new YahooQuotes().Fields("bid", "currency", "bid").GetAsync("C"));

            // duplicate field
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await new YahooQuotes().Fields("currency", "bid").Fields(Field.Ask, Field.Bid).GetAsync("C"));

            // invalid fields are ignored
            security = await new YahooQuotes().Fields("invalidfield").GetAsync("C");
            Assert.Throws<KeyNotFoundException>(() => security["invalidfield"]);
            Assert.False(security.Fields.TryGetValue("invalidfield", out dynamic bid));
        }

        [Fact]
        public async Task TestQuery()
        {
            Security security = await new YahooQuotes().GetAsync("AAPL");

            // Properties returns static type.
            double bid1 = security.Bid;

            // String or enum indexers return dynamic type.
            security.Fields.TryGetValue("Bid", out dynamic bid2);
            bid2 = security.Fields["Bid"];
            bid2 = security["Bid"];
            bid2 = security[Field.Bid];

            Assert.Equal("Apple Inc.", security["LongName"]);
        }

        [Fact]
        public async Task TestManySymbols()
        {
            List<string> symbols = File.ReadAllLines(@"..\..\..\symbols.txt")
                .Where(line => !line.StartsWith("#"))
                .Take(10)
                .ToList();

            // The length limit for a URI depends on the browser.
            // Exception.Message.StartsWith("Invalid URI: The Uri string is too long.");

            Dictionary<string, Security> securities = await new YahooQuotes().GetAsync(symbols);

            var counted = symbols.Where(s => securities.ContainsKey(s)).Count();
            Write($"requested symbols: {symbols.Count}");
            Write($"returned securities: {securities.Count}");
            Write($"missing: {symbols.Count - counted}");
            //Write($"changed?: {securities.Count - counted}");
        }

        [Fact]
        public async Task TestDateTime()
        {
            Security security = await new YahooQuotes().GetAsync("C");

            // RegularMarketTime is the time of the last price during regular market hours.
            long seconds = security.RegularMarketTime;
            Write("UnixTimeSeconds: " + seconds);

            Instant instant = Instant.FromUnixTimeSeconds(seconds);
            Write("Instant: " + instant); // ISO 8601 format string: yyyy-MM-ddTHH:mm:ssZ

            string exchangeTimezoneName = security.ExchangeTimezoneName;
            Write("ExchangeTimezoneName: " + exchangeTimezoneName);

            DateTimeZone timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(exchangeTimezoneName);
            Assert.NotNull(timeZone);

            ZonedDateTime zdt = instant.InZone(timeZone);
            Write("ZonedDateTime: " + zdt);

            // Using the provided extension methods:
            timeZone = security.ExchangeTimezoneName.ToDateTimeZone();
            ZonedDateTime zdt2 = security.RegularMarketTime.ToZonedDateTime(timeZone);
            Assert.Equal(zdt, zdt2);
        }

        [Theory]
        [InlineData("GMEXICOB.MX")] // Mexico City - 6:00
        [InlineData("TD.TO")]  // Canada -5
        [InlineData("SPY")]    // USA -5
        [InlineData("PETR4.SA")] //Sao_Paulo -3
        [InlineData("BP.L")]   // London 0:
        [InlineData("AIR.PA")] // Paris +1
        [InlineData("AIR.DE")] // Xetra +1
        [InlineData("AGL.JO")] //Johannesburg +2
        [InlineData("AFLT.ME")] // Moscow +3:00
        [InlineData("UNITECH.BO")] // IST +5:30
        [InlineData("2800.HK")] // Hong Kong +8
        [InlineData("000001.SS")] // Shanghai +8
        [InlineData("2448.TW")] // Taiwan +8
        [InlineData("005930.KS")] // Seoul +9
        [InlineData("7203.T")] // Tokyo +9 (Toyota)
        [InlineData("NAB.AX")] // Sydney +10
        [InlineData("FBU.NZ")] // Auckland + 12
        public async Task PeriodWithUnixTimeSeconds2(string symbol)
        {
            Security security = await new YahooQuotes().GetAsync(symbol);
            string timeZoneName = security.ExchangeTimezoneName;
            DateTimeZone timeZone = timeZoneName.ToDateTimeZone();

            ZonedDateTime zdt = security.RegularMarketTime.ToZonedDateTime(timeZone);

            Write($"Symbol:        {symbol}");
            Write($"TimeZone:      {timeZone.Id}");
            Write($"ZonedDateTime: {zdt}");
        }
    }
}

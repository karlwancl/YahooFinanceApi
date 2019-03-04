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
            string symbol = "C";
            Security? security = await new YahooQuotes().GetAsync(symbol);
            if (security == null)
                throw new Exception("Invalid symbol: " + symbol);
            Assert.NotNull(security.LongName);

            // invalid symbol
            Assert.Null(await new YahooQuotes().GetAsync("invalidSymbol"));

            // multiple symbols
            IList<string> symbols = new List<string>() { "invalidSymbol", "C" };
            Dictionary<string, Security?> securities = await new YahooQuotes().GetAsync(symbols);
            Assert.Null(securities["invalidSymbol"]);

            security = securities["C"];
            if (security == null)
                throw new Exception("Invalid symbol: " + symbols[0]);
            Assert.True(security.RegularMarketPrice > 0);
        }

        [Fact]
        public async Task TestSymbolArgument()
        {
            // empty string
            await Assert.ThrowsAsync<ArgumentException>(async () => await new YahooQuotes().GetAsync(""));

            // empty list
            await Assert.ThrowsAsync<ArgumentException>(async () => await new YahooQuotes().GetAsync(new string[] { }));

            // empty string in list
            await Assert.ThrowsAsync<ArgumentException>(async () => await new YahooQuotes().GetAsync(new[] { "" }));

            // duplicate symbol
            await Assert.ThrowsAsync<ArgumentException>(async () => await new YahooQuotes().GetAsync(new[] { "C", "A", "C" }));
        }

        [Fact]
        public async Task TestFieldsArgument()
        {
            Security? security = await new YahooQuotes()
                // can use string field names:
                .Fields("Bid", "Ask", "Tradeable", "LongName")
                // and/or field enums:
                .Fields(Field.RegularMarketPrice, Field.Currency)
                .GetAsync("C");

            // when no fields are specified, many(all?) fields are returned!
            security = await new YahooQuotes().GetAsync("C");
            if (security == null)
                throw new Exception("Invalid symbol: C");
            Assert.True(security.Fields.Count > 10);

            // duplicate field
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await new YahooQuotes().Fields("bid", "currency", "bid").GetAsync("C"));

            // duplicate field
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await new YahooQuotes().Fields("currency", "bid").Fields(Field.Ask, Field.Bid).GetAsync("C"));

            // invalid fields return null
            security = await new YahooQuotes().Fields("invalidfield").GetAsync("C");
            if (security == null)
                throw new Exception("Invalid symbol: C");
            Assert.Null(security["invalidfield"]);
        }

        [Fact]
        public async Task TestQuery()
        {
            Security? security = await new YahooQuotes().GetAsync("AAPL");
            if (security == null)
                throw new Exception("bad symbol");

            // Properties returns static type.
            double? bid1 = security.Bid;

            // String or enum indexers return dynamic type.
            security.Fields.TryGetValue("Bid", out dynamic? bid2);
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

            Dictionary<string, Security?> securities = await new YahooQuotes().GetAsync(symbols);

            var counted = symbols.Where(s => securities.ContainsKey(s)).Count();
            Write($"requested symbols: {symbols.Count}");
            Write($"returned securities: {securities.Count}");
            Write($"missing: {symbols.Count - counted}");
        }

        [Fact]
        public async Task TestDateTime()
        {
            Security? security = await new YahooQuotes().GetAsync("C");
            if (security == null)
                throw new Exception("Invalid symbol");

            // RegularMarketTime is the time of the last price during regular market hours.
            long? seconds = security.RegularMarketTime;
            if (seconds == null)
                throw new Exception("Invalid seconds");

            Write("UnixTimeSeconds: " + seconds);

            Instant instant = Instant.FromUnixTimeSeconds(seconds.Value);
            Write("Instant: " + instant); // ISO 8601 format string: yyyy-MM-ddTHH:mm:ssZ

            string? exchangeTimezoneName = security.ExchangeTimezoneName;
            Write("ExchangeTimezoneName: " + exchangeTimezoneName);

            DateTimeZone? timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(exchangeTimezoneName);
            Assert.NotNull(timeZone);

            ZonedDateTime zdt = instant.InZone(timeZone);
            Write("ZonedDateTime: " + zdt);

            // Using the provided extension methods:
            timeZone = security.ExchangeTimezoneName?.ToDateTimeZone();
            if (timeZone == null)
                throw new Exception("Invalid timeZone");

            ZonedDateTime? zdt2 = security.RegularMarketTime?.ToZonedDateTime(timeZone);
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
            Security? security = await new YahooQuotes().GetAsync(symbol);
            if (security == null)
                throw new Exception("invalid symbol: " + symbol);
            DateTimeZone ? timeZone = security.ExchangeTimezoneName?.ToDateTimeZone();
            if (timeZone == null)
                throw new Exception("Invalid time zone");

            ZonedDateTime? zdt = security.RegularMarketTime?.ToZonedDateTime(timeZone);

            Write($"Symbol:        {symbol}");
            Write($"TimeZone:      {timeZone.Id}");
            Write($"ZonedDateTime: {zdt}");
        }
    }
}

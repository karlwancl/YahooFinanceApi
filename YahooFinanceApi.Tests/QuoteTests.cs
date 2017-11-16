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
        public async Task TestYahooQueryArguments()
        {
            // no symbols
            await Assert.ThrowsAsync<ArgumentException>(async () => await Yahoo.Symbols().QueryAsync());

            // symbol not found
            await Assert.ThrowsAsync<Flurl.Http.FlurlHttpException>(async () => await Yahoo.Symbols("invalidsymbol").QueryAsync());

            // duplicate symbol
            await Assert.ThrowsAsync<ArgumentException>(async () => await Yahoo.Symbols("C", "A", "C").QueryAsync());

            // note that invalid fields have no effect!
            await Yahoo.Symbols("C").Fields("invalidfield").QueryAsync();

            // duplicate field
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await Yahoo.Symbols("C").Fields("currency", "bid").Fields(Field.Ask, Field.Bid).QueryAsync());

            // when no fields are specified, all fields are returned, probably
            await Yahoo.Symbols("C").QueryAsync();
        }

        [Fact]
        public async Task TestYahooQuery()
        {
            var securities = await Yahoo
                    .Symbols("C", "AAPL")
                    // Can use string field names ...
                    .Fields("Bid", "Ask", "Tradeable", "LongName")
                    // and/or Field enums.
                    .Fields(Field.RegularMarketPrice, Field.Currency)

                    .QueryAsync();

            Assert.Equal(2, securities.Count());

            double bid1 = securities["C"]["Bid"]; // strings => dynamic

            double bid2 = securities["C"][Field.Bid]; // Field enum => dynamic

            double bid3 = securities["C"].Bid; // property => static type


            Assert.True(securities["C"]["tradeable"]);

            Assert.Equal("Apple Inc.", securities["AAPL"]["longName"]);
        }

        [Fact]
        public async Task TestYahooQueryNotRequested()
        {
            var securities = await Yahoo.Symbols("AAPL").Fields(Field.Symbol).QueryAsync();

            var security = securities.First().Value;

            // This field was requested and therefore will be available.
            Assert.Equal("AAPL", security.Symbol);

            // This field was not requested and is not available.
            Assert.Throws<KeyNotFoundException>(() => security.TwoHundredDayAverageChange);

            // Many fields are available even though only one was requested!
            Assert.True(security.Fields.Count > 1);
        }

        [Fact]
        public async Task MakeEnumList()
        {
            var securities = await Yahoo.Symbols("C").QueryAsync();
            var fields = securities.First().Value.Fields;

            Write("Paste into Field enum:" + Environment.NewLine);

            Write("// This list was generated automatically. These names have been defined by Yahoo.");

            Write(String.Join("," + Environment.NewLine, fields.Select(f => f.Key)));

            Write(Environment.NewLine + ".");
        }

        [Fact]
        public async Task MakePropertyList()
        {
            var securities = await Yahoo.Symbols("C").QueryAsync();
            var fields = securities.First().Value.Fields;

            Write(String.Format("{0}{0}Paste into class Security:{0}", Environment.NewLine));

            Write("// This list was generated automatically. These names and types have been defined by Yahoo.");

            foreach (var field in fields)
                Write($"public {field.Value.GetType().Name} {field.Key} => Fields[\"{field.Key}\"];");

            Write(Environment.NewLine + ".");
        }

    }
}

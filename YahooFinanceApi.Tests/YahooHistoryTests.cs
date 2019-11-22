using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using NodaTime;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#nullable enable

namespace YahooFinanceApi.Tests
{
    public class YahooHistoryTests
    {
        private readonly Action<string> Write;
        public YahooHistoryTests(ITestOutputHelper output) => Write = output.WriteLine;

        [Fact]
        public async Task SimpleTest()
        {
            List<HistoryTick>? ticks = await new YahooHistory()
                .Period(Duration.FromDays(10))
                .GetHistoryAsync("C");
            if (ticks == null)
                throw new Exception("Invalid symbol");

            Assert.NotEmpty(ticks);
            Assert.True(ticks[0].Close > 0);

            Assert.Null(await new YahooHistory().GetHistoryAsync("badSymbol"));
        }

        [Fact]
        public async Task TestSymbols()
        {
            string[] symbols = new [] { "C", "badSymbol" };
            Dictionary<string, List<HistoryTick>?> dictionary = await new YahooHistory().GetHistoryAsync(symbols);
            Assert.Equal(symbols.Length, dictionary.Count);

            List<HistoryTick>? ticks = dictionary["C"];
            if (ticks == null)
                throw new Exception("Invalid symbol");
            Assert.True(ticks[0].Close > 0);

            Assert.Null(dictionary["badSymbol"]);
        }

        [Fact]
        public void TestSymbolsArgument()
        {
            var y = new YahooHistory();
            Assert.ThrowsAsync<ArgumentException>(async () => await y.GetHistoryAsync(""));
            Assert.ThrowsAsync<ArgumentException>(async () => await y.GetHistoryAsync(new string[] { }));
            Assert.ThrowsAsync<ArgumentException>(async () => await y.GetHistoryAsync(new string[] { "" }));
            Assert.ThrowsAsync<ArgumentException>(async () => await y.GetHistoryAsync(new string[] { "C", "" }));
        }

        [Fact]
        public async Task TestDuplicateSymbols()
        {
            var y = new YahooHistory();
            var exception = await Assert.ThrowsAsync<ArgumentException>
                (async () => await y.GetHistoryAsync(new[] { "C", "X", "C" }));
            Assert.StartsWith("Duplicate symbol(s): \"C\".", exception.Message);
        }

        [Fact]
        public async Task TestPeriodWithDuration() // Duration does not take into account calendar or timezone.
        {
            // default frequency is daily
            var ticks = await new YahooHistory().Period(Duration.FromDays(10)).GetHistoryAsync("C");
            if (ticks == null)
                throw new Exception("Invalid symbol");
            foreach (var tick in ticks)
                Write($"{tick.Date} {tick.Close}");
            Assert.True(ticks.Count > 3);
        }

        [Fact]
        public async Task TestPeriodWithUnixTimeSeconds()
        {
            LocalDateTime dt = new LocalDateTime(2019, 1, 7, 16, 0);
            ZonedDateTime zdt = dt.InZoneLeniently("America/New_York".ToDateTimeZone());
            long seconds = zdt.ToInstant().ToUnixTimeSeconds();

            var ticks = await new YahooHistory().Period(seconds).GetHistoryAsync("C");
            if (ticks == null)
                throw new Exception("Invalid symbol");
            foreach (var tick in ticks)
                Write($"{tick.Date} {tick.Close}");
            Assert.Equal(ticks[0].Date, dt.Date);
        }

        [Fact]
        public async Task TestPeriodWithDate()
        {
            DateTimeZone dateTimeZone = "Asia/Taipei".ToDateTimeZone() ?? throw new Exception("Invalid timezone");
            LocalDate localDate = new LocalDate(2019, 1, 7);

            var ticks = await new YahooHistory().Period(dateTimeZone, localDate).GetHistoryAsync("2448.TW");
            if (ticks == null)
                throw new Exception("Invalid symbol");
            foreach (var tick in ticks)
                Write($"{tick.Date} {tick.Close}");
            Assert.Equal(ticks[0].Date, localDate);
        }

        [Fact]
        public async Task TestHistoryTickTest()
        {
            DateTimeZone dateTimeZone = "America/New_York".ToDateTimeZone() ?? throw new Exception("Invalid timezone");

            LocalDate localDate1 = new LocalDate(2017, 1, 3);
            LocalDate localDate2 = new LocalDate(2017, 1, 4);

            var ticks = await new YahooHistory()
                .Period(dateTimeZone, localDate1, localDate2)
                .GetHistoryAsync("AAPL", Frequency.Daily);

            if (ticks == null)
                throw new Exception("Invalid symbol");

            Assert.Equal(2, ticks.Count());

            var tick = ticks[0];
            Assert.Equal(115.800003m, tick.Open);
            Assert.Equal(116.330002m, tick.High);
            Assert.Equal(114.760002m, tick.Low);
            Assert.Equal(116.150002m, tick.Close);
            Assert.Equal(28_781_900, tick.Volume);

            foreach (var t in ticks)
                Write($"{t.Date} {t.Close}");
        }

        [Fact]
        public async Task TestDividend()
        {
            DateTimeZone dateTimeZone = "America/New_York".ToDateTimeZone() ?? throw new Exception("Invalid timezone");
            var dividends = await new YahooHistory()
                .Period(dateTimeZone, new LocalDate(2016, 2, 4), new LocalDate(2016, 2, 5))
                .GetDividendsAsync("AAPL");

            if (dividends == null)
                throw new Exception("Invalid symbol");

            Assert.Equal(0.52m, dividends[0].Dividend);
        }

        [Fact]
        public async Task TestSplit()
        {
            DateTimeZone dateTimeZone = "America/New_York".ToDateTimeZone() ?? throw new Exception("Invalid timezone");
            var splits = await new YahooHistory()
                .Period(dateTimeZone, new LocalDate(2014, 6, 8), new LocalDate(2014, 6, 10))
                .GetSplitsAsync("AAPL");
            if (splits == null)
                throw new Exception("Invalid symbol");
            Assert.Equal(1, splits[0].BeforeSplit);
            Assert.Equal(7, splits[0].AfterSplit);
        }

        [Fact]
        public async Task TestDates_US()
        {
            DateTimeZone dateTimeZone = "America/New_York".ToDateTimeZone() ?? throw new Exception("Invalid timezone");

            var from = new LocalDate(2017, 10, 10);
            var to = new LocalDate(2017, 10, 12);

            var ticks = await new YahooHistory().Period(dateTimeZone, from, to)
                .GetHistoryAsync("C", Frequency.Daily);
            if (ticks == null)
                throw new Exception("Invalid symbol");

            Assert.Equal(from, ticks.First().Date);
            Assert.Equal(to, ticks.Last().Date);

            Assert.Equal(3, ticks.Count());
            Assert.Equal(75.18m, ticks[0].Close);
            Assert.Equal(74.940002m, ticks[1].Close);
            Assert.Equal(72.370003m, ticks[2].Close);
        }

        [Fact]
        public async Task TestDates_UK()
        {
            DateTimeZone dateTimeZone = "Europe/London".ToDateTimeZone() ?? throw new Exception("Invalid timezone");

            var from = new LocalDate(2017, 10, 10);
            var to = new LocalDate(2017, 10, 12);

            var ticks = await new YahooHistory().Period(dateTimeZone, from, to)
                .GetHistoryAsync("BA.L", Frequency.Daily);
            if (ticks == null)
                throw new Exception("Invalid symbol");

            Assert.Equal(from, ticks.First().Date);
            Assert.Equal(to, ticks.Last().Date);

            Assert.Equal(3, ticks.Count());
            Assert.Equal(616.50m, ticks[0].Close);
            Assert.Equal(615.00m, ticks[1].Close);
            Assert.Equal(616.00m, ticks[2].Close);
        }

        [Fact]
        public async Task TestDates_TW()
        {
            DateTimeZone dateTimeZone = "Asia/Taipei".ToDateTimeZone() ?? throw new Exception("Invalid timezone");

            var from = new LocalDate(2019, 3, 19);
            var to = new LocalDate(2019, 3, 21);

            var ticks = await new YahooHistory().Period(dateTimeZone, from, to)
                .GetHistoryAsync("2618.TW", Frequency.Daily);
            if (ticks == null)
                throw new Exception("Invalid symbol");

            Assert.Equal(from, ticks.First().Date);
            Assert.Equal(to, ticks.Last().Date);

            Assert.Equal(3, ticks.Count());
            Assert.Equal(14.8567m, ticks[0].Close);
            Assert.Equal(14.8082m, ticks[1].Close);
            Assert.Equal(14.8567m, ticks[2].Close);
        }

        [Theory]
        [InlineData("SPY")] // USA
        [InlineData("TD.TO")] // Canada
        [InlineData("BP.L")] // London
        [InlineData("AIR.PA")] // Euronext
        [InlineData("AIR.DE")] // Xetra
        [InlineData("UNITECH.BO")] // Bombay
        [InlineData("2800.HK")] // Hong Kong
        [InlineData("000001.SS")] // Shanghai
        [InlineData("2448.TW")] // Taiwan
        [InlineData("005930.KS")] // Korea
        [InlineData("BHP.AX")] // Sydney
        public async Task TestDates(string symbol)
        {
            var security = await new YahooQuotes().GetAsync(symbol) ?? throw new Exception($"Invalid symbol: {symbol}.");
            var timeZoneName = security.ExchangeTimezoneName ?? throw new Exception($"Timezone name not found.");
            var timeZone = timeZoneName.ToDateTimeZone() ?? throw new Exception($"Invalid timezone: {timeZoneName}.");

            var from = new LocalDate(2019, 9, 4);
            var to = from.PlusDays(2);

            var ticks = await new YahooHistory().Period(timeZone, from, to)
                .GetHistoryAsync(symbol);

            Assert.Equal(from, ticks.First().Date);
            Assert.Equal(to, ticks.Last().Date);
            Assert.Equal(3, ticks.Count());
        }

        [Fact]
        public async Task TestCurrency()
        {
            var symbol = "EURUSD=X";
            var security = await new YahooQuotes().GetAsync(symbol) ?? throw new Exception($"Invalid symbol: {symbol}.");

            var timezoneName = security.ExchangeTimezoneName ?? throw new Exception($"Timezone name not found.");
            var timeZone = timezoneName.ToDateTimeZone() ?? throw new Exception($"Invalid timezone: {timezoneName}.");

            var from = new LocalDate(2017, 10, 10);
            var to = from.PlusDays(2);

            var ticks = await new YahooHistory().Period(timeZone, from, to)
                .GetHistoryAsync(symbol);

            if (ticks == null)
                throw new Exception($"Invalid symbol: {symbol}");

            foreach (var tick in ticks)
                Write($"{tick.Date} {tick.Close}");

            Assert.Equal(from, ticks.First().Date);
            Assert.Equal(to, ticks.Last().Date);

            Assert.Equal(3, ticks.Count());
            Assert.Equal(1.174164m, ticks[0].Close);
            Assert.Equal(1.181488m, ticks[1].Close);
            Assert.Equal(1.186549m, ticks[2].Close);
        }

        [Fact]
        public async Task TestFrequency()
        {
            var symbol = "AAPL";
            var timeZone = "America/New_York".ToDateTimeZone();
            var startDate = new LocalDate(2019, 1, 10);

            if (timeZone == null)
                throw new Exception("Invalid timezone");


            var ticks1 = await new YahooHistory().Period(timeZone, startDate).GetHistoryAsync(symbol, Frequency.Daily);
            if (ticks1 == null)
                throw new Exception($"Invalid symbol: {symbol}");

            Assert.Equal(new LocalDate(2019, 1, 10), ticks1[0].Date);
            Assert.Equal(new LocalDate(2019, 1, 11), ticks1[1].Date);
            Assert.Equal(152.880005m, ticks1[1].Open);


            var ticks2 = await new YahooHistory().Period(timeZone, startDate).GetHistoryAsync(symbol, Frequency.Weekly);
            if (ticks2 == null)
                throw new Exception($"Invalid symbol: {symbol}");

            Assert.Equal(new LocalDate(2019, 1, 7), ticks2[0].Date); // previous Monday
            Assert.Equal(new LocalDate(2019, 1, 14), ticks2[1].Date);
            Assert.Equal(150.850006m, ticks2[1].Open);


            var ticks3 = await new YahooHistory().Period(timeZone, startDate).GetHistoryAsync(symbol, Frequency.Monthly);
            if (ticks3 == null)
                throw new Exception($"Invalid symbol: {symbol}");

            foreach (var tick in ticks3)
                Write($"{tick.Date} {tick.Close}");

            Assert.Equal(new LocalDate(2019, 2, 1), ticks3[0].Date); // next start of month !!!?
            Assert.Equal(new LocalDate(2019, 3, 1), ticks3[1].Date);
            Assert.Equal(174.279999m, ticks3[1].Open);
        }

        private List<string> GetSymbols(int number)
        {
            return File.ReadAllLines(@"..\..\..\symbols.txt")
                .Where(line => !line.StartsWith("#"))
                .Take(number)
                .ToList();
        }

        [Fact]
        public async Task TestManySymbols()
        {
            var symbols = GetSymbols(10);

            var results = await new YahooHistory().Period(Duration.FromDays(10)).GetHistoryAsync(symbols);
            var invalidSymbols = results.Where(r => r.Value == null).Count();

            // If (message.StartsWith("Call failed. Collection was modified"))
            // this is a bug in Flurl: https://github.com/tmenier/Flurl/issues/398

            Write("");
            Write($"Total Symbols:   {symbols.Count}");
            Write($"Invalid Symbols: {invalidSymbols}");
        }

        [Fact]
        public async Task TestCancellationTimeout()
        {
            var cts = new CancellationTokenSource();
            //cts.CancelAfter(20);

            var task = new YahooHistory(false, null, cts.Token).Period(Duration.FromDays(10)).GetHistoryAsync(GetSymbols(5));

            cts.Cancel();

            await Assert.ThrowsAnyAsync<Exception>(async () => await task);
        }

        [Fact]
        public async Task TestLoggerInjection()
        {
            YahooHistory yahooHistory = new ServiceCollection()
                .AddSingleton<YahooHistory>()
                .BuildServiceProvider()
                .GetRequiredService<YahooHistory>();

            await yahooHistory.GetHistoryAsync("C"); // log message should appear in the debug output (when debugging)
        }

    }
}

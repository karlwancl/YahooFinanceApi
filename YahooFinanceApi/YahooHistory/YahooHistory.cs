using CsvHelper;
using Flurl;
using Flurl.Http;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Linq;

#nullable enable

namespace YahooFinanceApi
{
    public sealed class YahooHistory
    {
        private readonly CancellationToken ct;
        private readonly bool ignoreEmptyRows;
        private long start = 0, end = long.MaxValue;
        private Frequency frequency = Frequency.Daily;

        public YahooHistory(bool ignoreEmptyRows = false, CancellationToken ct = default)
        {
            this.ignoreEmptyRows = ignoreEmptyRows;
            this.ct = ct;
        }

        // UnixTimeSeconds, UTC, native
        public YahooHistory Period(long start, long end = long.MaxValue)
        {
            if (start > Utility.Clock.GetCurrentInstant().ToUnixTimeSeconds())
                throw new ArgumentException("start > now");
            if (start > end)
                throw new ArgumentException("start > end");
            this.start = start;
            this.end = end;
            return this;
        }

        public YahooHistory Period(Duration duration) =>
            Period(Utility.Clock.GetCurrentInstant().Minus(duration).ToUnixTimeSeconds(), long.MaxValue);

        public YahooHistory Period(DateTimeZone timeZone, LocalDate start, LocalDate? end = null)
        {
            var startSeconds = start.At(new LocalTime(16, 0)).InZoneLeniently(timeZone).ToInstant().ToUnixTimeSeconds();
            var endSeconds = (end == null) ? long.MaxValue : end.Value.At(new LocalTime(16, 0)).InZoneLeniently(timeZone).ToInstant().ToUnixTimeSeconds();
            return Period(startSeconds, endSeconds);
        }

        public Task<List<HistoryTick>?>
            GetHistoryAsync(string symbol, Frequency frequency = Frequency.Daily) => GetTicksAsync<HistoryTick>(symbol, frequency);

        public Task<Dictionary<string, List<HistoryTick>?>>
            GetHistoryAsync(IList<string> symbols, Frequency frequency = Frequency.Daily) => GetTicksAsync<HistoryTick>(symbols, frequency);

        public Task<List<DividendTick>?>
            GetDividendsAsync(string symbol) => GetTicksAsync<DividendTick>(symbol);

        public Task<Dictionary<string, List<DividendTick>?>>
            GetDividendsAsync(IList<string> symbols) => GetTicksAsync<DividendTick>(symbols);

        public Task<List<SplitTick>?> 
            GetSplitsAsync(string symbol) => GetTicksAsync<SplitTick>(symbol);

        public Task<Dictionary<string, List<SplitTick>?>>
            GetSplitsAsync(IList<string> symbols) => GetTicksAsync<SplitTick>(symbols);

        private async Task<Dictionary<string, List<ITick>?>> GetTicksAsync<ITick>(IList<string> symbols, Frequency frequency = Frequency.Daily) where ITick : class
        {
            if (!symbols.Any())
                throw new ArgumentException("Empty list.", nameof(symbols));

            var duplicates = symbols.CaseInsensitiveDuplicates();
            if (duplicates.Any())
            {
                var msg = "Duplicate symbol(s): " + duplicates.Select(s => "\"" + s + "\"").ToCommaDelimitedList() + ".";
                throw new ArgumentException(msg, nameof(symbols));
            }

            // create a list of started tasks
            var tasks = symbols.Select(symbol => GetTicksAsync<ITick>(symbol, frequency)).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var dictionary = tasks.Select((task, i) => (task, i))
                .ToDictionary(x => symbols[x.i], x => x.task.Result, StringComparer.OrdinalIgnoreCase);

            return dictionary;
        }

        private async Task<List<ITick>?> GetTicksAsync<ITick>(string symbol, Frequency frequency = Frequency.Daily) where ITick : class
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Empty string.", nameof(symbol));

            this.frequency = frequency;
            string tickParam = TickParser.GetParamFromType<ITick>();

            try
            {
                return await GetTickResponseAsync<ITick>(symbol, tickParam).ConfigureAwait(false);
            }
            catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // symbol not found
            }
        }

        private async Task<List<ITick>> GetTickResponseAsync<ITick>(string symbol, string tickParam) where ITick:class
        {

            using (var stream = await GetResponseStreamAsync(symbol, tickParam).ConfigureAwait(false))
            using (var sr = new StreamReader(stream))
            using (var csvReader = new CsvReader(sr))
            {
                var ticks = new List<ITick>();

                csvReader.Read(); // skip header

                while (csvReader.Read())
                {
                    var tick = TickParser.Parse<ITick>(csvReader.Context.Record, ignoreEmptyRows);
                    if (tick != null)
                        ticks.Add(tick);
                }
                return ticks;
            }
        }

        private async Task<Stream> GetResponseStreamAsync(string symbol, string tickParam)
        {
            bool reset = false;
            while (true)
            {
                try
                {
                    var (client, crumb) = await ClientFactory.GetClientAndCrumbAsync(reset, ct).ConfigureAwait(false);
                    return await _GetResponseStreamAsync(client, crumb).ConfigureAwait(false);
                }
                catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == HttpStatusCode.Unauthorized && !reset)
                {
                    Debug.WriteLine("GetResponseStreamAsync: Unauthorized. Retrying.");
                    reset = true;
                }
                //catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == HttpStatusCode.NotFound)
                //  throw new Exception($"Invalid symbol '{symbol}'.", ex);
            }

            #region Local Functions

            Task<Stream> _GetResponseStreamAsync(IFlurlClient _client, string _crumb)
            {
                var url = "https://query1.finance.yahoo.com/v7/finance/download"
                    .AppendPathSegment(symbol)
                    .SetQueryParam("period1", start)
                    .SetQueryParam("period2", end)
                    .SetQueryParam("interval", $"1{frequency.Name()}")
                    .SetQueryParam("events", tickParam)
                    .SetQueryParam("crumb", _crumb);

                Debug.WriteLine(url);

                return url
                    .WithClient(_client)
                    .GetAsync(ct)
                    .ReceiveStream();
            }

            #endregion
        }
    }
}

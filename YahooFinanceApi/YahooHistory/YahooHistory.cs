using CsvHelper;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Linq;

namespace YahooFinanceApi
{
    public sealed class YahooHistory
    {
        private string[] symbols;
        private DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), end = DateTime.UtcNow;
        private Frequency frequency = Frequency.Daily;
        private string tickParam;
        private CancellationToken ct;

        private YahooHistory() { }

        // static!
        public static YahooHistory Symbols(params string[] symbols)
        {
            if (symbols == null)
                throw new ArgumentNullException(nameof(symbols));

            if (symbols.Length == 0)
                throw new ArgumentException("Empty list.", nameof(symbols));

            var pos = Array.IndexOf(symbols, null);
            if (pos >= 0)
                throw new ArgumentNullException("symbols[" + pos + "].");

            pos = Array.IndexOf(symbols.Select(s => s.Trim(' ')).ToArray(), "");
            if (pos >= 0)
                throw new ArgumentException("Empty string.", "symbols[" + pos + "].");

            var duplicates = symbols.Duplicates();
            if (duplicates.Any())
            {
                var msg = "Duplicate symbol(s): " + duplicates.Select(s => "\"" + s + "\"").ToCommaDelimitedList() + ".";
                throw new ArgumentException(msg, nameof(symbols));
            }
            return new YahooHistory { symbols = symbols };
        }

        public YahooHistory Period(DateTime start, DateTime? end = null) // UTC else local time
        {
            this.start = start;
            if (end != null)
                this.end = end.Value;
            if (start > end)
                throw new ArgumentException("start > end");
            return this;
        }

        public Task<IReadOnlyList<(string Symbol, Task<IReadOnlyList<HistoryTick>> Task)>> GetHistoryAsync(Frequency frequency = Frequency.Daily, CancellationToken ct = default)
        {
            this.frequency = frequency;
            return GetTicksAsync<HistoryTick>(ct);
        }

        public Task<IReadOnlyList<(string Symbol, Task<IReadOnlyList<DividendTick>> Task)>> GetDividendsAsync(CancellationToken ct = default)
            => GetTicksAsync<DividendTick>(ct);

        public Task<IReadOnlyList<(string Symbol, Task<IReadOnlyList<SplitTick>> Task)>> GetSplitsAsync(CancellationToken ct = default)
            => GetTicksAsync<SplitTick>(ct);

        private async Task<IReadOnlyList<(string, Task<IReadOnlyList<ITick>>)>> GetTicksAsync<ITick>(CancellationToken ct)
        {
            this.ct = ct;

            // create a list of started tasks
            var results = symbols.Select(symbol => (symbol, GetTicksAsync<ITick>(symbol))).ToList();
            try
            {
                await Task.WhenAll(results.Select(r => r.Item2)).ConfigureAwait(false);
            }
            catch { }

            return results;
        }

        private async Task<IReadOnlyList<ITick>> GetTicksAsync<ITick>(string symbol)
        {
            tickParam = TickParser.GetParamFromType<ITick>();

            using (var stream = await GetResponseStreamAsync(symbol).ConfigureAwait(false))
            using (var sr = new StreamReader(stream))
            using (var csvReader = new CsvReader(sr))
            {
                csvReader.Read(); // skip header

                var ticks = new List<ITick>();

                while (csvReader.Read())
                {
                    var tick = TickParser.Parse<ITick>(csvReader.Context.Record);
                    if (tick != null)
                        ticks.Add(tick);
                }

                return ticks;
            }
        }

        private async Task<Stream> GetResponseStreamAsync(string symbol)
        {
            bool reset = false;
            while (true)
            {
                try
                {
                    var (client, crumb) = await ClientFactory.GetClientAndCrumbAsync(reset, ct).ConfigureAwait(false);
                    return await _GetResponseStreamAsync(client, crumb).ConfigureAwait(false);
                }
                catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new Exception($"Invalid ticker or endpoint for symbol '{symbol}'.", ex);
                }
                catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Debug.WriteLine("GetResponseStreamAsync: Unauthorized.");

                    if (reset)
                        throw;
                    reset = true; // try again with a new client
                }
            }

            #region Local Functions

            Task<Stream> _GetResponseStreamAsync(IFlurlClient _client, string _crumb)
            {
                var url = "https://query1.finance.yahoo.com/v7/finance/download"
                    .AppendPathSegment(symbol)
                    .SetQueryParam("period1", start.ToUnixTimestamp())
                    .SetQueryParam("period2", end.ToUnixTimestamp())
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

    public static class ExtensionMethods
    {
        public static async Task<IReadOnlyList<T>> SingleAsync<T>(this Task<IReadOnlyList<(string Symbol, Task<IReadOnlyList<T>> Task)>> task)
        {
            var result = await task.ConfigureAwait(false);
            return await result.Single().Task.ConfigureAwait(false);
        }
    }
}

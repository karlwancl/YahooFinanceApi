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

namespace YahooFinanceApi
{
    public static partial class Yahoo
    {
        public static bool IgnoreEmptyRows { set { RowExtension.IgnoreEmptyRows = value; } }

        public static async Task<IList<Candle>> GetHistoricalAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, Period period = Period.Daily, CancellationToken token = default(CancellationToken))
		    => await GetTicksAsync(symbol, 
	                               startTime, 
	                               endTime, 
	                               period, 
	                               ShowOption.History,
                                   RowExtension.ToCandle,
                                   token);

        public static async Task<IList<DividendTick>> GetDividendsAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, CancellationToken token = default(CancellationToken))
            => await GetTicksAsync(symbol, 
                                   startTime, 
                                   endTime, 
                                   Period.Daily, 
                                   ShowOption.Dividend,
                                   RowExtension.ToDividendTick,
                                   token);

        public static async Task<IList<SplitTick>> GetSplitsAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, CancellationToken token = default(CancellationToken))
            => await GetTicksAsync(symbol,
                                   startTime,
                                   endTime,
                                   Period.Daily,
                                   ShowOption.Split,
                                   RowExtension.ToSplitTick,
                                   token);

        private static async Task<List<ITick>> GetTicksAsync<ITick>(
            string symbol,
            DateTime? startTime,
            DateTime? endTime,
            Period period,
            ShowOption showOption,
            Func<string[], ITick> instanceFunction,
            CancellationToken token
            )
        {
            using (var stream = await GetResponseStreamAsync(symbol, startTime, endTime, period, showOption.Name(), token).ConfigureAwait(false))
			using (var sr = new StreamReader(stream))
			using (var csvReader = new CsvReader(sr))
			{
                csvReader.Read(); // skip header

                var ticks = new List<ITick>();

                while (csvReader.Read())
                {
                    var tick = instanceFunction(csvReader.Context.Record);
                    if (tick != null)
                        ticks.Add(tick);
                }

                return ticks;
            }
		}

        private static async Task<Stream> GetResponseStreamAsync(string symbol, DateTime? startTime, DateTime? endTime, Period period, string events, CancellationToken token)
        {
            bool reset = false;
            while (true)
            {
                try
                {
                    var (client, crumb) = await YahooClientFactory.GetClientAndCrumbAsync(reset, token).ConfigureAwait(false);
                    return await _GetResponseStreamAsync(client, crumb, token).ConfigureAwait(false);
                }
                catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new Exception("Invalid ticker or endpoint.", ex);
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

            Task<Stream> _GetResponseStreamAsync(IFlurlClient _client, string _crumb, CancellationToken _token)
            {
                // Yahoo expects dates to be "Eastern Standard Time"
                startTime = startTime?.FromEstToUtc() ?? new DateTime(1970, 1, 1);
                endTime =   endTime?  .FromEstToUtc() ?? DateTime.UtcNow;

                var url = "https://query1.finance.yahoo.com/v7/finance/download"
                    .AppendPathSegment(symbol)
                    .SetQueryParam("period1", startTime.Value.ToUnixTimestamp())
                    .SetQueryParam("period2", endTime.Value.ToUnixTimestamp())
                    .SetQueryParam("interval", $"1{period.Name()}")
                    .SetQueryParam("events", events)
                    .SetQueryParam("crumb", _crumb);

                Debug.WriteLine(url);

                return url
                    .WithClient(_client)
                    .GetAsync(_token)
                    .ReceiveStream();
            }

            #endregion
        }
    }
}

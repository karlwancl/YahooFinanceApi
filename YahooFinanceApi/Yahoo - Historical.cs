using CsvHelper;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;

namespace YahooFinanceApi
{
    public static partial class Yahoo
    {
        // Singleton
        static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        const string QueryUrl = "https://query1.finance.yahoo.com/v7/finance/download";

        const string Period1Tag = "period1";
        const string Period2Tag = "period2";
        const string IntervalTag = "interval";
        const string EventsTag = "events";
        const string CrumbTag = "crumb";

        public static async Task<IList<Candle>> GetHistoricalAsync(string symbol, DateTime? startTime = default(DateTime?), DateTime? endTime = default(DateTime?), Period period = Period.Daily, bool ascending = false, bool leaveZeroIfInvalidRow = false, string timeZone = default(string), CancellationToken token = default(CancellationToken))
		    => await GetTicksAsync(symbol, 
	                               startTime, 
	                               endTime, 
	                               period, 
	                               ShowOption.History, 
                                   timeZone,
	                               r => r.ToCandle(timeZone, period),
                                   r => r.ToFallbackCandle(timeZone, period),
	                               ascending, 
                                   leaveZeroIfInvalidRow,
	                               token);

        public static async Task<IList<DividendTick>> GetDividendsAsync(string symbol, DateTime? startTime = default(DateTime?), DateTime? endTime = default(DateTime?), bool ascending = false, bool leaveZeroIfInvalidRow = false, string timeZone = default(string), CancellationToken token = default(CancellationToken))
            => await GetTicksAsync(symbol, 
                                   startTime, 
                                   endTime, 
                                   Period.Daily, 
                                   ShowOption.Dividend, 
                                   timeZone,
                                   r => r.ToDividendTick(timeZone, Period.Daily), 
                                   r => r.ToFallbackDividendTick(timeZone, Period.Daily),
                                   ascending, 
                                   leaveZeroIfInvalidRow,
                                   token);
                               
        public static async Task<IList<SplitTick>> GetSplitsAsync(string symbol, DateTime? startTime = default(DateTime?), DateTime? endTime = default(DateTime?), bool ascending = false, bool leaveZeroIfInvalidRow = false, string timeZone = default(string), CancellationToken token = default(CancellationToken))
            => await GetTicksAsync(symbol, 
                                   startTime, 
                                   endTime, 
                                   Period.Daily, 
                                   ShowOption.Split, 
                                   timeZone,
                                   r => r.ToSplitTick(timeZone, Period.Daily),
                                   r => r.ToFallbackSplitTick(timeZone, Period.Daily),
                                   ascending, 
                                   leaveZeroIfInvalidRow,
                                   token);

        static async Task<IList<T>> GetTicksAsync<T>(
            string symbol,
            DateTime? startTime,
            DateTime? endTime,
            Period period,
            ShowOption showOption,
            string timeZone,
            Func<string[], T> instanceFunction,
            Func<string[], T> fallbackFunction,
            bool ascending, 
            bool leaveZeroIfInvalidRow,
            CancellationToken token
            ) where T: ITick
        {
            if (instanceFunction == null)
                return new List<T>();

            var ticks = new List<T>();
			using (var stream = await GetResponseStreamAsync(symbol, startTime, endTime, period, showOption.Name(), timeZone, token).ConfigureAwait(false))
			using (var sr = new StreamReader(stream))
			using (var csvReader = new CsvReader(sr))
			{
				while (csvReader.Read())
				{
					string[] row = csvReader.CurrentRecord;
                    DateTime? dateTime = null;
                    try
                    {
                        dateTime = row[0].ToSpecDateTime(timeZone, period);
                        ticks.Add(instanceFunction(row));
                    }
                    catch
                    {
                        if (dateTime.HasValue && leaveZeroIfInvalidRow)
                            ticks.Add(fallbackFunction(row));
                    }
				}

                return ticks.OrderBy(c => c.DateTime, new DateTimeComparer(ascending)).ToList();
			}
		}

        static async Task<Stream> GetResponseStreamAsync(string symbol, DateTime? startTime, DateTime? endTime, Period period, string events, string timeZone, CancellationToken token)
        {
			var client = await YahooClientFactory.GetClientAsync().ConfigureAwait(false);
            var crumb = await YahooClientFactory.GetCrumbAsync().ConfigureAwait(false);

            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                return await LocalGetResponseStream(client, crumb).ConfigureAwait(false);
            }
            catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == HttpStatusCode.Unauthorized)
            {
                YahooClientFactory.Reset();
                client = await YahooClientFactory.GetClientAsync().ConfigureAwait(false);
                crumb = await YahooClientFactory.GetCrumbAsync().ConfigureAwait(false);
                return await LocalGetResponseStream(client, crumb).ConfigureAwait(false);
            }
            catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception("You may have used an invalid ticker, or the endpoint is invalidated", ex);
            }
            finally
            {
                _semaphoreSlim.Release();   
            }

            Task<Stream> LocalGetResponseStream(IFlurlClient localClient, string localCrumb)
            {
				var url = QueryUrl
                    .AppendPathSegment(symbol)
                    .SetQueryParam(Period1Tag, (startTime ?? new DateTime(1970, 1, 1)).ToUnixTimestamp(timeZone))
                    .SetQueryParam(Period2Tag, (endTime ?? DateTime.Now).ToUnixTimestamp(timeZone))
                    .SetQueryParam(IntervalTag, $"1{period.Name()}")
                    .SetQueryParam(EventsTag, events)
                    .SetQueryParam(CrumbTag, localCrumb);

                return localClient
                    .WithUrl(url)
                    .GetAsync(token)
                    .ReceiveStream();
            }
        }
    }
}

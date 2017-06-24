using CsvHelper;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace YahooFinanceApi
{
    public static partial class Yahoo
    {
        // Singleton
        static object InitClientAndCrumbLock = new object();
        static object WebCallLock = new object();
        static IFlurlClient YahooFinanceClient;
        static string Crumb;

        const string QueryUrl = "https://query1.finance.yahoo.com/v7/finance/download";
        const string CookieUrl = "https://finance.yahoo.com";
        const string CrumbUrl = "https://query1.finance.yahoo.com/v1/test/getcrumb";

        const string Period1Tag = "period1";
        const string Period2Tag = "period2";
        const string IntervalTag = "interval";
        const string EventsTag = "events";
        const string CrumbTag = "crumb";

        public static async Task<IList<Candle>> GetHistoricalAsync(string symbol, DateTime? startTime = default(DateTime?), DateTime? endTime = default(DateTime?), Period period = Period.Daily, bool ascending = false, CancellationToken token = default(CancellationToken))
		=> await GetTicksAsync(symbol, 
                               startTime, 
                               endTime, 
                               period, 
                               ShowOption.History, 
                               r => r.ToCandle(),
                               ascending, 
                               token);

		public static async Task<IList<DividendTick>> GetDividendsAsync(string symbol, DateTime? startTime = default(DateTime?), DateTime? endTime = default(DateTime?), bool ascending = false, CancellationToken token = default(CancellationToken))
            => await GetTicksAsync(symbol, 
                                   startTime, 
                                   endTime, 
                                   Period.Daily, 
                                   ShowOption.Dividend, 
                                   r => r.ToDividendTick(), 
                                   ascending, 
                                   token);
                               
        public static async Task<IList<SplitTick>> GetSplitsAsync(string symbol, DateTime? startTime = default(DateTime?), DateTime? endTime = default(DateTime?), bool ascending = false, CancellationToken token = default(CancellationToken))
            => await GetTicksAsync(symbol, 
                                   startTime, 
                                   endTime, 
                                   Period.Daily, 
                                   ShowOption.Split, 
                                   r => r.ToSplitTick(),
                                   ascending, 
                                   token);

        static async Task<IList<T>> GetTicksAsync<T>(
            string symbol,
            DateTime? startTime,
            DateTime? endTime,
            Period period,
            ShowOption showOption,
            Func<string[], T> instanceFunction,
            bool ascending, 
            CancellationToken token
            ) where T: ITick
        {
            if (instanceFunction == null)
                return new List<T>();

            var ticks = new List<T>();
			using (var stream = await GetResponseStreamAsync(symbol, startTime, endTime, period, showOption.Name(), token).ConfigureAwait(false))
			using (var sr = new StreamReader(stream))
			using (var csvReader = new CsvReader(sr))
			{
				while (csvReader.Read())
				{
					string[] row = csvReader.CurrentRecord;
                    try { ticks.Add(instanceFunction(row)); } catch { /* Intentionally blank, ignore all record with invalid format */ }
				}

                return ticks.OrderBy(c => c.DateTime, new DateTimeComparer(ascending)).ToList();
			}
		}

        static async Task<Stream> GetResponseStreamAsync(string symbol, DateTime? startTime, DateTime? endTime, Period period, string events, CancellationToken token)
        {
            if (YahooFinanceClient == null)
                await Task.Run(() => InitClientAndCrumb(token)).ConfigureAwait(false);

            var url = QueryUrl
                .AppendPathSegment(symbol)
                .SetQueryParam(Period1Tag, (startTime ?? new DateTime(1970, 1, 1)).ToUnixTimestamp())
                .SetQueryParam(Period2Tag, (endTime ?? DateTime.Now).ToUnixTimestamp())
                .SetQueryParam(IntervalTag, $"1{period.Name()}")
                .SetQueryParam(EventsTag, events)
                .SetQueryParam(CrumbTag, Crumb);

            return await Task.Run(() =>
            {
                lock (WebCallLock)
                    return YahooFinanceClient
                        .WithUrl(url)
                        .GetAsync(token)
                        .ReceiveStream()
                        .Result;
            }).ConfigureAwait(false);
        }

        static void InitClientAndCrumb(CancellationToken token)
        {
            lock (InitClientAndCrumbLock)
            {
                if (YahooFinanceClient == null)
                {
                    var client = new FlurlClient().EnableCookies();

                    client
                        .WithUrl(CookieUrl)
                        .GetAsync(token)
                        .Result
                        .EnsureSuccessStatusCode();

                    Crumb = client
                        .WithUrl(CrumbUrl)
                        .GetAsync(token)
                        .ReceiveString()
                        .Result;
                    
                    YahooFinanceClient = client;
                }
            }
        }
    }
}

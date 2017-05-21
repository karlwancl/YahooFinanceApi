using CsvHelper;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Flurl.Http.Configuration;

namespace YahooFinanceApi
{
    public static partial class Yahoo
    {
        // Singleton
        private static object SyncLock = new object();
        private static IFlurlClient YahooFinanceClient;
        private static string Crumb;

        private const string QueryUrl = "https://query1.finance.yahoo.com/v7/finance/download";
        private const string CookieUrl = "https://finance.yahoo.com";
        private const string CrumbUrl = "https://query1.finance.yahoo.com/v1/test/getcrumb";

        private const string Period1Tag = "period1";
        private const string Period2Tag = "period2";
        private const string IntervalTag = "interval";
        private const string EventsTag = "events";
        private const string CrumbTag = "crumb";

        private static readonly IDictionary<Period, string> PeriodMap = new Dictionary<Period, string>
        {
            {Period.Daily, "d"},
            {Period.Weekly, "wk"},
            {Period.Monthly, "mo"}
        };

        private const string HistoryValue = "history";
        private const string DividendValue = "div";

        public static async Task<IList<Candle>> GetHistoricalAsync(string symbol, DateTime? startTime = default(DateTime?), DateTime? endTime = default(DateTime?), Period period = Period.Daily, bool ascending = false, CancellationToken token = default(CancellationToken))
        {
            var candles = new List<Candle>();
            using (var stream = await GetResponseStreamAsync(symbol, startTime, endTime, period, HistoryValue, token).ConfigureAwait(false))
            using (var sr = new StreamReader(stream))
            using (var csvReader = new CsvReader(sr))
            {
                while (csvReader.Read())
                {
                    string[] row = csvReader.CurrentRecord;
                    try
                    {
                        candles.Add(new Candle(
                            Convert.ToDateTime(row[0]),
                            Convert.ToDecimal(row[1]),
                            Convert.ToDecimal(row[2]),
                            Convert.ToDecimal(row[3]),
                            Convert.ToDecimal(row[4]),
                            Convert.ToInt64(row[6]),
                            Convert.ToDecimal(row[5])));
                    }
                    catch
                    {
                        // Intentionally blank, ignore all record with invalid format
                    }
                }

                return ascending ? candles.OrderBy(c => c.DateTime).ToList() : candles.OrderByDescending(c => c.DateTime).ToList();
            }
        }

        public static async Task<IList<DividendTick>> GetHistoricalDividendsAsync(string symbol, DateTime? startTime = default(DateTime?), DateTime? endTime = default(DateTime?), bool ascending = false, CancellationToken token = default(CancellationToken))
        {
            var dividends = new List<DividendTick>();
            using (var stream = await GetResponseStreamAsync(symbol, startTime, endTime, Period.Daily, DividendValue, token).ConfigureAwait(false))
            using (var sr = new StreamReader(stream))
            using (var csvReader = new CsvReader(sr))
            {
               while (csvReader.Read())
               {
                   string[] row = csvReader.CurrentRecord;
                    try
                    {
                        dividends.Add(new DividendTick(
                            Convert.ToDateTime(row[0]),
                            Convert.ToDecimal(row[1])));
                    }
                    catch
                    {
                        // Intentionally blank, ignore all record with invalid format
                    }
                }

               return ascending ? dividends.OrderBy(c => c.DateTime).ToList() : dividends.OrderByDescending(c => c.DateTime).ToList();
            }
        }

        private static async Task<Stream> GetResponseStreamAsync(string symbol, DateTime? startTime, DateTime? endTime, Period period, string events, CancellationToken token)
        {
            await Task.Factory.StartNew(() => InitClient(token)).ConfigureAwait(false);

            var url = QueryUrl
                .AppendPathSegment(symbol)
                .SetQueryParam(Period1Tag, (startTime ?? new DateTime(1970, 1, 1)).ToUnixTimestamp())
                .SetQueryParam(Period2Tag, (endTime ?? DateTime.Now).ToUnixTimestamp())
                .SetQueryParam(IntervalTag, $"1{PeriodMap[period]}")
                .SetQueryParam(EventsTag, events)
                .SetQueryParam(CrumbTag, Crumb);

            return await YahooFinanceClient
                .WithUrl(url)
                .GetAsync(token)
                .ReceiveStream()
                .ConfigureAwait(false);
        }

        private static void InitClient(CancellationToken token)
        {
            lock (SyncLock)
            {
                if (YahooFinanceClient == null || YahooFinanceClient.Cookies.Any(c => c.Value.Expired))
                {
                    YahooFinanceClient = new FlurlClient().EnableCookies();

                    var cookieResponse = YahooFinanceClient
                        .WithUrl(CookieUrl)
                        .GetAsync(token)
                        .Result;

                    cookieResponse.EnsureSuccessStatusCode();

                    Crumb = YahooFinanceClient
                        .WithUrl(CrumbUrl)
                        .GetAsync(token)
                        .ReceiveString()
                        .Result;
                } 
            }
        }
    }
}

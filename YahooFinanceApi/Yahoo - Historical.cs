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
            {Period.Weekly, "w"},
            {Period.Monthly, "m"}
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
                   candles.Add(new Candle(
                       Convert.ToDateTime(row[0]),
                       Convert.ToDecimal(row[1]),
                       Convert.ToDecimal(row[2]),
                       Convert.ToDecimal(row[3]),
                       Convert.ToDecimal(row[4]),
                       Convert.ToInt64(row[6]),
                       Convert.ToDecimal(row[5])
                       ));
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
                   dividends.Add(new DividendTick(
                       Convert.ToDateTime(row[0]),
                       Convert.ToDecimal(row[1])
                       ));
               }

               return ascending ? dividends.OrderBy(c => c.DateTime).ToList() : dividends.OrderByDescending(c => c.DateTime).ToList();
            }
        }

        private static async Task<Stream> GetResponseStreamAsync(string symbol, DateTime? startTime, DateTime? endTime, Period period, string events, CancellationToken token)
        {
            using (var client = new FlurlClient().EnableCookies())
            {
                var crumb = await GetCrumbAsync(client, token).ConfigureAwait(false);
                if (string.IsNullOrEmpty(crumb))
                    throw new ArgumentNullException("Crumb is empty!");

                var url = QueryUrl
                    .AppendPathSegment(symbol)
                    .SetQueryParam(Period1Tag, (startTime ?? new DateTime(1970, 1, 1)).ToUnixTimestamp())
                    .SetQueryParam(Period2Tag, (endTime ?? DateTime.Now).ToUnixTimestamp())
                    .SetQueryParam(IntervalTag, $"1{PeriodMap[period]}")
                    .SetQueryParam(EventsTag, events)
                    .SetQueryParam(CrumbTag, crumb);

                return await client
                    .WithUrl(url)
                    .GetAsync(token)
                    .ReceiveStream()
                    .ConfigureAwait(false);
            }
        }

        private static async Task<string> GetCrumbAsync(IFlurlClient client, CancellationToken token)
        {
            var cookieResponse = await client
                .WithUrl(CookieUrl)
                .GetAsync(token)
                .ConfigureAwait(false);

            cookieResponse.EnsureSuccessStatusCode();

            return await client
                .WithUrl(CrumbUrl)
                .GetAsync(token)
                .ReceiveString()
                .ConfigureAwait(false);
        }
    }
}

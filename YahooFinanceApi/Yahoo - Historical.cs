using CsvHelper;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YahooFinanceApi
{
    public static partial class Yahoo
    {
        private const string YahooFinanceUrl = "http://ichart.finance.yahoo.com/table.csv";

        private const string SymbolTag = "s";

        private const string FromMonthTag = "a";
        private const string FromDayTag = "b";
        private const string FromYearTag = "c";

        private const string ToMonthTag = "d";
        private const string ToDayTag = "e";
        private const string ToYearTag = "f";

        private const string TypeTag = "g";
        private static readonly IDictionary<Period, string> PeriodMap = new Dictionary<Period, string>
        {
            {Period.Daily, "d"},
            {Period.Weekly, "w"},
            {Period.Monthly, "m"}
        };

        private const string DividendValue = "v";

        private const string IgnoreTag = "ignore";
        private const string CsvExtValue = ".csv";

        public static async Task<IList<Candle>> GetHistoricalAsync(string symbol, DateTime? startTime = default(DateTime?), DateTime? endTime = default(DateTime?), Period period = Period.Daily, bool ascending = false, CancellationToken token = default(CancellationToken))
        {
            var candles = new List<Candle>();
            using (var stream = await GetResponseStreamAsync(symbol, startTime, endTime, PeriodMap[period], token).ConfigureAwait(false))
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
                        Convert.ToInt64(row[5]),
                        Convert.ToDecimal(row[6])
                        ));
                }

                return ascending ? candles.OrderBy(c => c.DateTime).ToList() : candles.OrderByDescending(c => c.DateTime).ToList();
            }
        }

        public static async Task<IList<DividendTick>> GetHistoricalDividendsAsync(string symbol, DateTime? startTime = default(DateTime?), DateTime? endTime = default(DateTime?), bool ascending = false, CancellationToken token = default(CancellationToken))
        {
            var dividends = new List<DividendTick>();
            using (var stream = await GetResponseStreamAsync(symbol, startTime, endTime, DividendValue, token).ConfigureAwait(false))
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

        private static async Task<Stream> GetResponseStreamAsync(string symbol, DateTime? startTime, DateTime? endTime, string type, CancellationToken token)
            => await YahooFinanceUrl
                .SetQueryParam(SymbolTag, symbol)
                .SetQueryParam(FromMonthTag, startTime?.Month)
                .SetQueryParam(FromDayTag, startTime?.Day)
                .SetQueryParam(FromYearTag, startTime?.Year)
                .SetQueryParam(ToMonthTag, endTime?.Month)
                .SetQueryParam(ToDayTag, endTime?.Day)
                .SetQueryParam(ToYearTag, endTime?.Year)
                .SetQueryParam(TypeTag, type)
                .SetQueryParam(IgnoreTag, CsvExtValue)
                .GetAsync(token)
                .ReceiveStream()
                .ConfigureAwait(false);
    }
}

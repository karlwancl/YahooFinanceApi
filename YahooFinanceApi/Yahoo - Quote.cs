using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace YahooFinanceApi
{
    public partial class Yahoo
    {
        private List<string> symbols;
        private readonly HashSet<string> fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private Yahoo() { }

        // static!
        public static Yahoo Symbols(params string[] symbols)
        {
            if (symbols == null || symbols.Length == 0 || symbols.Any(x => x == null))
                throw new ArgumentException(nameof(symbols));

            return new Yahoo { symbols = new List<string>(symbols) };
        }

        public Yahoo Fields(params string[] fields)
        {
            if (fields == null || fields.Length == 0 || fields.Any(x => x == null))
                throw new ArgumentException(nameof(fields));

            this.fields.UnionWith(fields);

            return this;
        }

        public Yahoo Fields(params Field[] fields)
        {
            if (fields == null || fields.Length == 0)
                throw new ArgumentException(nameof(fields));

            this.fields.UnionWith(fields.Select(f => f.ToString()));

            return this;
        }

        public async Task<Dictionary<string, Security>> QueryAsync(CancellationToken token = default(CancellationToken))
        {
            if (!symbols.Any())
                throw new ArgumentException("No symbols indicated.");

            var url = "https://query1.finance.yahoo.com/v7/finance/quote"
                .SetQueryParam("symbols", string.Join(",", symbols));

            if (fields.Any())
                url = url.SetQueryParam("fields", string.Join(",", fields));

            Debug.WriteLine(url);

            var result = await url
                .GetAsync(token)
                .ReceiveJson() // expandoObject
                .ConfigureAwait(false);

            var quoteResponse = result.quoteResponse;

            var error = quoteResponse.error;
            if (error != null)
                throw new InvalidDataException($"QueryAsync error: {error}");

            if (quoteResponse.result.Count != symbols.Count)
                throw new InvalidDataException($"Received {quoteResponse.result.Count}/{symbols.Count} symbols.");

            var securities = new Dictionary<string, Security>();

            // Note that the returned symbol (result) may be different from the requested symbol (key).
            for (var i = 0; i < symbols.Count; i++)
                securities.Add(symbols[i], new Security(quoteResponse.result[i]));

            return securities;
        }
    }

    public class Security
    {
        public IDictionary<string, dynamic> Fields { get; internal set; }

        // ctor
        internal Security(IDictionary<string, dynamic> fields) => Fields = fields;

        public dynamic this[string fieldName] => Fields[fieldName];
        public dynamic this[Field field] => Fields[field.ToString()];

        // This list was generated automatically. These names and types have defined by Yahoo.
        public String language => Fields["language"];
        public String quoteType => Fields["quoteType"];
        public String quoteSourceName => Fields["quoteSourceName"];
        public String currency => Fields["currency"];
        public Boolean tradeable => Fields["tradeable"];
        public Double postMarketPrice => Fields["postMarketPrice"];
        public Double postMarketChange => Fields["postMarketChange"];
        public Double regularMarketChangePercent => Fields["regularMarketChangePercent"];
        public String financialCurrency => Fields["financialCurrency"];
        public Int64 averageDailyVolume3Month => Fields["averageDailyVolume3Month"];
        public Int64 averageDailyVolume10Day => Fields["averageDailyVolume10Day"];
        public Double fiftyTwoWeekLowChange => Fields["fiftyTwoWeekLowChange"];
        public Double fiftyTwoWeekLowChangePercent => Fields["fiftyTwoWeekLowChangePercent"];
        public Double postMarketChangePercent => Fields["postMarketChangePercent"];
        public Int64 postMarketTime => Fields["postMarketTime"];
        public Int64 marketCap => Fields["marketCap"];
        public Double forwardPE => Fields["forwardPE"];
        public Double priceToBook => Fields["priceToBook"];
        public Int64 sourceInterval => Fields["sourceInterval"];
        public Double fiftyTwoWeekHighChange => Fields["fiftyTwoWeekHighChange"];
        public Double fiftyTwoWeekHighChangePercent => Fields["fiftyTwoWeekHighChangePercent"];
        public Double fiftyTwoWeekLow => Fields["fiftyTwoWeekLow"];
        public Double fiftyTwoWeekHigh => Fields["fiftyTwoWeekHigh"];
        public Int64 dividendDate => Fields["dividendDate"];
        public Double bookValue => Fields["bookValue"];
        public Double fiftyDayAverage => Fields["fiftyDayAverage"];
        public Double fiftyDayAverageChange => Fields["fiftyDayAverageChange"];
        public Double fiftyDayAverageChangePercent => Fields["fiftyDayAverageChangePercent"];
        public Double twoHundredDayAverage => Fields["twoHundredDayAverage"];
        public Double twoHundredDayAverageChange => Fields["twoHundredDayAverageChange"];
        public Double twoHundredDayAverageChangePercent => Fields["twoHundredDayAverageChangePercent"];
        public Int64 earningsTimestamp => Fields["earningsTimestamp"];
        public Int64 earningsTimestampStart => Fields["earningsTimestampStart"];
        public Int64 earningsTimestampEnd => Fields["earningsTimestampEnd"];
        public Double trailingAnnualDividendRate => Fields["trailingAnnualDividendRate"];
        public Double trailingPE => Fields["trailingPE"];
        public Double trailingAnnualDividendYield => Fields["trailingAnnualDividendYield"];
        public Double epsTrailingTwelveMonths => Fields["epsTrailingTwelveMonths"];
        public Double epsForward => Fields["epsForward"];
        public Int64 sharesOutstanding => Fields["sharesOutstanding"];
        public String exchangeTimezoneShortName => Fields["exchangeTimezoneShortName"];
        public Int64 gmtOffSetMilliseconds => Fields["gmtOffSetMilliseconds"];
        public Int64 priceHint => Fields["priceHint"];
        public Double regularMarketPreviousClose => Fields["regularMarketPreviousClose"];
        public Double bid => Fields["bid"];
        public Double ask => Fields["ask"];
        public Int64 bidSize => Fields["bidSize"];
        public Int64 askSize => Fields["askSize"];
        public String messageBoardId => Fields["messageBoardId"];
        public String fullExchangeName => Fields["fullExchangeName"];
        public String longName => Fields["longName"];
        public Double regularMarketPrice => Fields["regularMarketPrice"];
        public Int64 regularMarketTime => Fields["regularMarketTime"];
        public Double regularMarketChange => Fields["regularMarketChange"];
        public Double regularMarketOpen => Fields["regularMarketOpen"];
        public Double regularMarketDayHigh => Fields["regularMarketDayHigh"];
        public Double regularMarketDayLow => Fields["regularMarketDayLow"];
        public Int64 regularMarketVolume => Fields["regularMarketVolume"];
        public String exchangeTimezoneName => Fields["exchangeTimezoneName"];
        public String shortName => Fields["shortName"];
        public String exchange => Fields["exchange"];
        public String market => Fields["market"];
        public String marketState => Fields["marketState"];
        public Int64 exchangeDataDelayedBy => Fields["exchangeDataDelayedBy"];
        public String symbol => Fields["symbol"];
    }
}

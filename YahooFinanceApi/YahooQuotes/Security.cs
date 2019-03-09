using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

#nullable enable

namespace YahooFinanceApi
{
    public class Security
    {
        public IReadOnlyDictionary<string, dynamic> Fields { get; }

        // ctor
        internal Security(IDictionary<string, dynamic> dictionary)
        {
            var fields = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in dictionary)
                fields.Add(kvp.Key.ToPascal(), kvp.Value);
            Fields = new ReadOnlyDictionary<string, dynamic>(fields);
        }

        public dynamic? this[string fieldName] => Get(fieldName);
        public dynamic? this[Field field] => Get(field.ToString());
        private dynamic? Get([CallerMemberName] string? name = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return Fields.TryGetValue(name, out dynamic val) ? val : null;
        }

        // Security.cs: 69. This list was generated automatically from names defined by Yahoo.
        public Double? Ask => Get();
        public Int64? AskSize => Get();
        public Int64? AverageDailyVolume10Day => Get();
        public Int64? AverageDailyVolume3Month => Get();
        public Double? Bid => Get();
        public Int64? BidSize => Get();
        public Double? BookValue => Get();
        public String? Currency => Get();
        public Int64? DividendDate => Get();
        public Int64? EarningsTimestamp => Get();
        public Int64? EarningsTimestampEnd => Get();
        public Int64? EarningsTimestampStart => Get();
        public Double? EpsForward => Get();
        public Double? EpsTrailingTwelveMonths => Get();
        public Boolean? EsgPopulated => Get();
        public String? Exchange => Get();
        public Int64? ExchangeDataDelayedBy => Get();
        public String? ExchangeTimezoneName => Get();
        public String? ExchangeTimezoneShortName => Get();
        public Double? FiftyDayAverage => Get();
        public Double? FiftyDayAverageChange => Get();
        public Double? FiftyDayAverageChangePercent => Get();
        public Double? FiftyTwoWeekHigh => Get();
        public Double? FiftyTwoWeekHighChange => Get();
        public Double? FiftyTwoWeekHighChangePercent => Get();
        public Double? FiftyTwoWeekLow => Get();
        public Double? FiftyTwoWeekLowChange => Get();
        public Double? FiftyTwoWeekLowChangePercent => Get();
        public String? FiftyTwoWeekRange => Get();
        public String? FinancialCurrency => Get();
        public Double? ForwardPE => Get();
        public String? FullExchangeName => Get();
        public Int64? GmtOffSetMilliseconds => Get();
        public String? Language => Get();
        public String? LongName => Get();
        public String? Market => Get();
        public Int64? MarketCap => Get();
        public String? MarketState => Get();
        public String? MessageBoardId => Get();
        public Double? PostMarketChange => Get();
        public Double? PostMarketChangePercent => Get();
        public Double? PostMarketPrice => Get();
        public Int64? PostMarketTime => Get();
        public Int64? PriceHint => Get();
        public Double? PriceToBook => Get();
        public String? QuoteSourceName => Get();
        public String? QuoteType => Get();
        public String? Region => Get();
        public Double? RegularMarketChange => Get();
        public Double? RegularMarketChangePercent => Get();
        public Double? RegularMarketDayHigh => Get();
        public Double? RegularMarketDayLow => Get();
        public String? RegularMarketDayRange => Get();
        public Double? RegularMarketOpen => Get();
        public Double? RegularMarketPreviousClose => Get();
        public Double? RegularMarketPrice => Get();
        public Int64? RegularMarketTime => Get();
        public Int64? RegularMarketVolume => Get();
        public Int64? SharesOutstanding => Get();
        public String? ShortName => Get();
        public Int64? SourceInterval => Get();
        public String? Symbol => Get();
        public Boolean? Tradeable => Get();
        public Double? TrailingAnnualDividendRate => Get();
        public Double? TrailingAnnualDividendYield => Get();
        public Double? TrailingPE => Get();
        public Double? TwoHundredDayAverage => Get();
        public Double? TwoHundredDayAverageChange => Get();
        public Double? TwoHundredDayAverageChangePercent => Get();
    }
}

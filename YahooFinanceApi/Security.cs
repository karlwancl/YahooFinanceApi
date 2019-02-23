using System;
using System.Collections.Generic;

namespace YahooFinanceApi
{
    public class Security
    {
        public IReadOnlyDictionary<string, dynamic> Fields { get; private set; }

        // ctor
        internal Security(IReadOnlyDictionary<string, dynamic> fields) => Fields = fields;

        public dynamic this[string fieldName] => Fields[fieldName];
        public dynamic this[Field field] => Fields[field.ToString()];

        // Security.cs: This list was generated automatically. These names and types have been defined by Yahoo.
        public Double Ask => this["Ask"];
        public Int64 AskSize => this["AskSize"];
        public Int64 AverageDailyVolume10Day => this["AverageDailyVolume10Day"];
        public Int64 AverageDailyVolume3Month => this["AverageDailyVolume3Month"];
        public Double Bid => this["Bid"];
        public Int64 BidSize => this["BidSize"];
        public Double BookValue => this["BookValue"];
        public String Currency => this["Currency"];
        public Int64 DividendDate => this["DividendDate"];
        public Int64 EarningsTimestamp => this["EarningsTimestamp"];
        public Int64 EarningsTimestampEnd => this["EarningsTimestampEnd"];
        public Int64 EarningsTimestampStart => this["EarningsTimestampStart"];
        public Double EpsForward => this["EpsForward"];
        public Double EpsTrailingTwelveMonths => this["EpsTrailingTwelveMonths"];
        public String Exchange => this["Exchange"];
        public Int64 ExchangeDataDelayedBy => this["ExchangeDataDelayedBy"];
        public String ExchangeTimezoneName => this["ExchangeTimezoneName"];
        public String ExchangeTimezoneShortName => this["ExchangeTimezoneShortName"];
        public Double FiftyDayAverage => this["FiftyDayAverage"];
        public Double FiftyDayAverageChange => this["FiftyDayAverageChange"];
        public Double FiftyDayAverageChangePercent => this["FiftyDayAverageChangePercent"];
        public Double FiftyTwoWeekHigh => this["FiftyTwoWeekHigh"];
        public Double FiftyTwoWeekHighChange => this["FiftyTwoWeekHighChange"];
        public Double FiftyTwoWeekHighChangePercent => this["FiftyTwoWeekHighChangePercent"];
        public Double FiftyTwoWeekLow => this["FiftyTwoWeekLow"];
        public Double FiftyTwoWeekLowChange => this["FiftyTwoWeekLowChange"];
        public Double FiftyTwoWeekLowChangePercent => this["FiftyTwoWeekLowChangePercent"];
        public String FinancialCurrency => this["FinancialCurrency"];
        public Double ForwardPE => this["ForwardPE"];
        public String FullExchangeName => this["FullExchangeName"];
        public Int64 GmtOffSetMilliseconds => this["GmtOffSetMilliseconds"];
        public String Language => this["Language"];
        public String LongName => this["LongName"];
        public String Market => this["Market"];
        public Int64 MarketCap => this["MarketCap"];
        public String MarketState => this["MarketState"];
        public String MessageBoardId => this["MessageBoardId"];
        public Int64 PriceHint => this["PriceHint"];
        public Double PriceToBook => this["PriceToBook"];
        public String QuoteSourceName => this["QuoteSourceName"];
        public String QuoteType => this["QuoteType"];
        public Double RegularMarketChange => this["RegularMarketChange"];
        public Double RegularMarketChangePercent => this["RegularMarketChangePercent"];
        public Double RegularMarketDayHigh => this["RegularMarketDayHigh"];
        public Double RegularMarketDayLow => this["RegularMarketDayLow"];
        public Double RegularMarketOpen => this["RegularMarketOpen"];
        public Double RegularMarketPreviousClose => this["RegularMarketPreviousClose"];
        public Double RegularMarketPrice => this["RegularMarketPrice"];
        public Int64 RegularMarketTime => this["RegularMarketTime"];
        public Int64 RegularMarketVolume => this["RegularMarketVolume"];
        public Double PostMarketChange => this["PostMarketChange"];
        public Double PostMarketChangePercent => this["PostMarketChangePercent"];
        public Double PostMarketPrice => this["PostMarketPrice"];
        public Int64 PostMarketTime => this["PostMarketTime"];
        public Int64 SharesOutstanding => this["SharesOutstanding"];
        public String ShortName => this["ShortName"];
        public Int64 SourceInterval => this["SourceInterval"];
        public String Symbol => this["Symbol"];
        public Boolean Tradeable => this["Tradeable"];
        public Double TrailingAnnualDividendRate => this["TrailingAnnualDividendRate"];
        public Double TrailingAnnualDividendYield => this["TrailingAnnualDividendYield"];
        public Double TrailingPE => this["TrailingPE"];
        public Double TwoHundredDayAverage => this["TwoHundredDayAverage"];
        public Double TwoHundredDayAverageChange => this["TwoHundredDayAverageChange"];
        public Double TwoHundredDayAverageChangePercent => this["TwoHundredDayAverageChangePercent"];
    }
}

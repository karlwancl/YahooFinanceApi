using System;
using System.Collections.Generic;

namespace YahooFinanceApi
{
    public class Security
    {
        public IDictionary<string, dynamic> Fields { get; private set; }

        // ctor
        internal Security(IDictionary<string, dynamic> fields) => Fields = fields;

        public dynamic this[string fieldName] => Fields[fieldName.ToLowerCamel()];
        public dynamic this[Field field] => Fields[field.ToString().ToLowerCamel()];

        // This list was generated automatically. These names and types have been defined by Yahoo.
        public String Language => Fields["language"];
        public String QuoteType => Fields["quoteType"];
        public String QuoteSourceName => Fields["quoteSourceName"];
        public String Currency => Fields["currency"];
        public Boolean Tradeable => Fields["tradeable"];
        public Double PostMarketPrice => Fields["postMarketPrice"];
        public Double PostMarketChange => Fields["postMarketChange"];
        public Double RegularMarketChangePercent => Fields["regularMarketChangePercent"];
        public String FinancialCurrency => Fields["financialCurrency"];
        public Int64 AverageDailyVolume3Month => Fields["averageDailyVolume3Month"];
        public Int64 AverageDailyVolume10Day => Fields["averageDailyVolume10Day"];
        public Double FiftyTwoWeekLowChange => Fields["fiftyTwoWeekLowChange"];
        public Double FiftyTwoWeekLowChangePercent => Fields["fiftyTwoWeekLowChangePercent"];
        public Double PostMarketChangePercent => Fields["postMarketChangePercent"];
        public Int64 PostMarketTime => Fields["postMarketTime"];
        public Int64 MarketCap => Fields["marketCap"];
        public Double ForwardPE => Fields["forwardPE"];
        public Double PriceToBook => Fields["priceToBook"];
        public Int64 SourceInterval => Fields["sourceInterval"];
        public Double FiftyTwoWeekHighChange => Fields["fiftyTwoWeekHighChange"];
        public Double FiftyTwoWeekHighChangePercent => Fields["fiftyTwoWeekHighChangePercent"];
        public Double FiftyTwoWeekLow => Fields["fiftyTwoWeekLow"];
        public Double FiftyTwoWeekHigh => Fields["fiftyTwoWeekHigh"];
        public Int64 DividendDate => Fields["dividendDate"];
        public Double BookValue => Fields["bookValue"];
        public Double FiftyDayAverage => Fields["fiftyDayAverage"];
        public Double FiftyDayAverageChange => Fields["fiftyDayAverageChange"];
        public Double FiftyDayAverageChangePercent => Fields["fiftyDayAverageChangePercent"];
        public Double TwoHundredDayAverage => Fields["twoHundredDayAverage"];
        public Double TwoHundredDayAverageChange => Fields["twoHundredDayAverageChange"];
        public Double TwoHundredDayAverageChangePercent => Fields["twoHundredDayAverageChangePercent"];
        public Int64 EarningsTimestamp => Fields["earningsTimestamp"];
        public Int64 EarningsTimestampStart => Fields["earningsTimestampStart"];
        public Int64 EarningsTimestampEnd => Fields["earningsTimestampEnd"];
        public Double TrailingAnnualDividendRate => Fields["trailingAnnualDividendRate"];
        public Double TrailingPE => Fields["trailingPE"];
        public Double TrailingAnnualDividendYield => Fields["trailingAnnualDividendYield"];
        public Double EpsTrailingTwelveMonths => Fields["epsTrailingTwelveMonths"];
        public Double EpsForward => Fields["epsForward"];
        public Int64 SharesOutstanding => Fields["sharesOutstanding"];
        public String ExchangeTimezoneShortName => Fields["exchangeTimezoneShortName"];
        public Int64 GmtOffSetMilliseconds => Fields["gmtOffSetMilliseconds"];
        public Int64 PriceHint => Fields["priceHint"];
        public Double RegularMarketPreviousClose => Fields["regularMarketPreviousClose"];
        public Double Bid => Fields["bid"];
        public Double Ask => Fields["ask"];
        public Int64 BidSize => Fields["bidSize"];
        public Int64 AskSize => Fields["askSize"];
        public String MessageBoardId => Fields["messageBoardId"];
        public String FullExchangeName => Fields["fullExchangeName"];
        public String LongName => Fields["longName"];
        public Double RegularMarketPrice => Fields["regularMarketPrice"];
        public Int64 RegularMarketTime => Fields["regularMarketTime"];
        public Double RegularMarketChange => Fields["regularMarketChange"];
        public Double RegularMarketOpen => Fields["regularMarketOpen"];
        public Double RegularMarketDayHigh => Fields["regularMarketDayHigh"];
        public Double RegularMarketDayLow => Fields["regularMarketDayLow"];
        public Int64 RegularMarketVolume => Fields["regularMarketVolume"];
        public String ExchangeTimezoneName => Fields["exchangeTimezoneName"];
        public String ShortName => Fields["shortName"];
        public String Exchange => Fields["exchange"];
        public String Market => Fields["market"];
        public String MarketState => Fields["marketState"];
        public Int64 ExchangeDataDelayedBy => Fields["exchangeDataDelayedBy"];
        public String Symbol => Fields["symbol"];
    }
}

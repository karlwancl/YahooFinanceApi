using System.Runtime.Serialization;

namespace YahooFinanceApi
{
    public enum Tag
    {
        [EnumMember(Value = "a")]
        Ask,
        [EnumMember(Value = "a2")]
        AverageDailyVolume,
        [EnumMember(Value = "a5")]
        AskSize,
        [EnumMember(Value = "b")]
        Bid,
        [EnumMember(Value = "b2")]
        AskRealtime,
        [EnumMember(Value = "b3")]
        BidRealtime,
        [EnumMember(Value = "b4")]
        BookValue,
        [EnumMember(Value = "b6")]
        BidSize,
        [EnumMember(Value = "c")]
        ChangeAndPercentChange,
        [EnumMember(Value = "c1")]
        Change,
        [EnumMember(Value = "c3")]
        Commission,
        [EnumMember(Value = "c4")]
        Currency,
        [EnumMember(Value = "c6")]
        ChangeRealtime,
        [EnumMember(Value = "c8")]
        AfterHoursChangeRealtime,
        [EnumMember(Value = "d")]
        DividendPerShare,
        [EnumMember(Value = "d1")]
        LastTradeDate,
        [EnumMember(Value = "d2")]
        TradeDate,
        [EnumMember(Value = "e")]
        EarningsPerShare,
        [EnumMember(Value = "e1")]
        ErrorIndication,
        [EnumMember(Value = "e7")]
        EpsEstimateCurrentYear,
        [EnumMember(Value = "e8")]
        EpsEstimateNextYear,
        [EnumMember(Value = "e9")]
        EpsEstimateNextQuarter,
        [EnumMember(Value = "f6")]
        FloatShares,
        [EnumMember(Value = "g")]
        DaysLow,
        [EnumMember(Value = "h")]
        DaysHigh,
        [EnumMember(Value = "j")]
        _52WeekLow,
        [EnumMember(Value = "k")]
        _52WeekHigh,
        [EnumMember(Value = "g1")]
        HoldingsGainPercent,
        [EnumMember(Value = "g3")]
        AnnualizedGain,
        [EnumMember(Value = "g4")]
        HoldingsGain,
        [EnumMember(Value = "g5")]
        HoldingsGainPercentRealtime,
        [EnumMember(Value = "g6")]
        HoldingsGainRealtime,
        [EnumMember(Value = "i")]
        MoreInfo,
        [EnumMember(Value = "i5")]
        OrderBookRealtime,
        [EnumMember(Value = "j1")]
        MarketCapitalization,
        [EnumMember(Value = "j3")]
        MarketCapRealtime,
        [EnumMember(Value = "j4")]
        Ebitda,
        [EnumMember(Value = "j5")]
        ChangeFrom52WeekLow,
        [EnumMember(Value = "j6")]
        PercentChangeFrom52WeekLow,
        [EnumMember(Value = "k1")]
        LastTradeRealtimeWithTime,
        [EnumMember(Value = "k2")]
        ChangePercentRealtime,
        [EnumMember(Value = "k3")]
        LastTradeSize,
        [EnumMember(Value = "k4")]
        ChangeFrom52WeekHigh,
        [EnumMember(Value = "k5")]
        PercentChangeFrom52WeekHigh,
        [EnumMember(Value = "l")]
        LastTradeWithTime,
        [EnumMember(Value = "l1")]
        LastTradePriceOnly,
        [EnumMember(Value = "l2")]
        HighLimit,
        [EnumMember(Value = "l3")]
        LowLimit,
        [EnumMember(Value = "m")]
        DaysRange,
        [EnumMember(Value = "m2")]
        DaysRangeRealtime,
        [EnumMember(Value = "m3")]
        _50DayMovingAverage,
        [EnumMember(Value = "m4")]
        _200DayMovingAverage,
        [EnumMember(Value = "m5")]
        ChangeFrom200DayMovingAverage,
        [EnumMember(Value = "m6")]
        PercentChangeFrom200DayMovingAverage,
        [EnumMember(Value = "m7")]
        ChangeFrom50DayMovingAverage,
        [EnumMember(Value = "m8")]
        PercentChangeFrom50DayMovingAverage,
        [EnumMember(Value = "n")]
        Name,
        [EnumMember(Value = "n4")]
        Notes,
        [EnumMember(Value = "o")]
        Open,
        [EnumMember(Value = "p")]
        PreviousClose,
        [EnumMember(Value = "p1")]
        PricePaid,
        [EnumMember(Value = "p2")]
        ChangeInPercent,
        [EnumMember(Value = "p5")]
        PricePerSales,
        [EnumMember(Value = "p6")]
        PricePerBook,
        [EnumMember(Value = "q")]
        ExdividendDate,
        [EnumMember(Value = "r")]
        PeRatio,
        [EnumMember(Value = "r1")]
        DividendPayDate,
        [EnumMember(Value = "r2")]
        PeRatioRealtime,
        [EnumMember(Value = "r5")]
        PegRatio,
        [EnumMember(Value = "r6")]
        PricePerEpsEstimateCurrentYear,
        [EnumMember(Value = "r7")]
        PricePerEpsEstimateNextYear,
        [EnumMember(Value = "s")]
        Symbol,
        [EnumMember(Value = "s1")]
        SharesOwned,
        [EnumMember(Value = "s7")]
        ShortRatio,
        [EnumMember(Value = "t1")]
        LastTradeTime,
        [EnumMember(Value = "t6")]
        TradeLinks,
        [EnumMember(Value = "t7")]
        TickerTrend,
        [EnumMember(Value = "t8")]
        _1YearTargetPrice,
        [EnumMember(Value = "v")]
        Volume,
        [EnumMember(Value = "v1")]
        HoldingsValue,
        [EnumMember(Value = "v7")]
        HoldingsValueRealtime,
        [EnumMember(Value = "w")]
        _52WeekRange,
        [EnumMember(Value = "w1")]
        DaysValueChange,
        [EnumMember(Value = "w4")]
        DaysValueChangeRealtime,
        [EnumMember(Value = "x")]
        StockExchange,
        [EnumMember(Value = "y")]
        DividendYield,
        [EnumMember(Value = "s6")]
        Revenue
    }
}

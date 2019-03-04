# YahooFinanceApi
[![Build status](https://ci.appveyor.com/api/projects/status/138s6on1y0wnaxms?svg=true)](https://ci.appveyor.com/project/lppkarl/yahoofinanceapi)
[![NuGet](https://img.shields.io/nuget/vpre/YahooFinanceApi.svg)](https://www.nuget.org/packages/YahooFinanceApi/)
[![NuGet](https://img.shields.io/nuget/dt/YahooFinanceApi.svg)](https://www.nuget.org/packages/YahooFinanceApi/)
[![license](https://img.shields.io/github/license/lppkarl/YahooFinanceApi.svg)](https://github.com/lppkarl/YahooFinanceApi/blob/master/LICENSE)

A handy Yahoo! Finance api wrapper, based on .NET Standard 2.0

## Features
* Get quotes
* Get historical data
* Get dividend data
* Get stock split data

## Notes
This library is intended for personal use only, any improper use of this library is not recommended.

## Install Note
Dependencies: NodaTime

## Supported Platforms
* .NET Core 2.0
* .NET framework 4.6.1 or above
* Xamarin.iOS
* Xamarin.Android
* Universal Windows Platform

## How To Install
You can find the package through Nuget

    PM> Install-Package YahooFinanceApi

## How To Use

### Add namespoace reference

    using YahooFinanceApi;

### Get stock quotes
```csharp
var securities = await new YahooQuotes()
        .GetAsync(new [] { "C", "AAPL" });
security = securities["C"];
var price = security.RegularMarketPrice;
```
### Supported fields for stock quote
Ask, AskSize, AverageDailyVolume10Day, AverageDailyVolume3Month, Bid, BidSize, BookValue, Currency, DividendDate, EarningsTimestamp, EarningsTimestampEnd, EarningsTimestampStart, EpsForward, EpsTrailingTwelveMonths, EsgPopulated, Exchange, ExchangeDataDelayedBy, ExchangeTimezoneName, ExchangeTimezoneShortName, FiftyDayAverage, FiftyDayAverageChange, FiftyDayAverageChangePercent, FiftyTwoWeekHigh, FiftyTwoWeekHighChange, FiftyTwoWeekHighChangePercent, FiftyTwoWeekLow, FiftyTwoWeekLowChange, FiftyTwoWeekLowChangePercent, FiftyTwoWeekRange, FinancialCurrency, ForwardPE, FullExchangeName, GmtOffSetMilliseconds, Language, LongName, Market, MarketCap, MarketState, MessageBoardId, PriceHint, PriceToBook, QuoteSourceName, QuoteType, Region, RegularMarketChange, RegularMarketChangePercent, RegularMarketDayHigh, RegularMarketDayLow, RegularMarketDayRange, RegularMarketOpen, RegularMarketPreviousClose, RegularMarketPrice, RegularMarketTime, RegularMarketVolume, SharesOutstanding, ShortName, SourceInterval, Symbol, Tradeable, TrailingAnnualDividendRate, TrailingAnnualDividendYield, TrailingPE, TwoHundredDayAverage, TwoHundredDayAverageChange, TwoHundredDayAverageChangePercent
### Ignore invalid rows
Sometimes, yahoo returns broken rows for historical calls, you could decide if these invalid rows is ignored or not by the following statement:
```csharp
Yahoo.IgnoreEmptyRows = true;
```    

### Get historical data for a stock
```csharp
var securities = await new YahooHistory()
        .Period(Duration.FromDays(10))
        .GetHistoryAsync(new[] { "C", "AAPL" });
var historyTicks = securities["C"];
var firstClose = historyTicks[0].Close;
```
### Get dividend history for a stock
```csharp
var securities = await new YahooHistory()
        .Period(Duration.FromDays(10))
        .GetDividendsAsync(new[] { "C", "AAPL" });
```
### Get stock split history for a stock
```csharp
var securities = await new YahooHistory()
        .Period(Duration.FromDays(10))
        .GetSplitsAsync(new[] { "C", "AAPL" });
```
### Powered by
* [Flurl](https://github.com/tmenier/Flurl) ([@tmenier](https://github.com/tmenier)) - A simple & elegant fluent-style REST api library 

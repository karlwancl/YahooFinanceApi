# YahooFinanceApi
[![Build status](https://ci.appveyor.com/api/projects/status/138s6on1y0wnaxms?svg=true)](https://ci.appveyor.com/project/lppkarl/yahoofinanceapi)
[![NuGet](https://img.shields.io/nuget/vpre/YahooFinanceApi.svg)](https://www.nuget.org/packages/YahooFinanceApi/)
[![NuGet](https://img.shields.io/nuget/dt/YahooFinanceApi.svg)](https://www.nuget.org/packages/YahooFinanceApi/)
[![license](https://img.shields.io/github/license/lppkarl/YahooFinanceApi.svg)](https://github.com/lppkarl/YahooFinanceApi/blob/master/LICENSE)

A handy Yahoo! Finance API wrapper supporting .NET Standard 2.0
### Features
* Delayed quotes
* Historical quotes
* Dividend history
* Stock split history
### Supported Platforms
.NET Standard 2.0
### Installation
```csharp
PM> Install-Package YahooFinanceApi
```
```csharp
using YahooFinanceApi;
```
Dependencies: NodaTime
### Delayed Quotes
```csharp
var securities = await new YahooQuotes()
   .GetAsync(new [] { "C", "AAPL" });
security = securities["C"];
var price = security.RegularMarketPrice;
```
***Supported Fields:***

Ask, AskSize, AverageDailyVolume10Day, AverageDailyVolume3Month, Bid, BidSize, BookValue, Currency, DividendDate, EarningsTimestamp, EarningsTimestampEnd, EarningsTimestampStart, EpsForward, EpsTrailingTwelveMonths, EsgPopulated, Exchange, ExchangeDataDelayedBy, ExchangeTimezoneName, ExchangeTimezoneShortName, FiftyDayAverage, FiftyDayAverageChange, FiftyDayAverageChangePercent, FiftyTwoWeekHigh, FiftyTwoWeekHighChange, FiftyTwoWeekHighChangePercent, FiftyTwoWeekLow, FiftyTwoWeekLowChange, FiftyTwoWeekLowChangePercent, FiftyTwoWeekRange, FinancialCurrency, ForwardPE, FullExchangeName, GmtOffSetMilliseconds, Language, LongName, Market, MarketCap, MarketState, MessageBoardId, PriceHint, PriceToBook, QuoteSourceName, QuoteType, Region, RegularMarketChange, RegularMarketChangePercent, RegularMarketDayHigh, RegularMarketDayLow, RegularMarketDayRange, RegularMarketOpen, RegularMarketPreviousClose, RegularMarketPrice, RegularMarketTime, RegularMarketVolume, SharesOutstanding, ShortName, SourceInterval, Symbol, Tradeable, TrailingAnnualDividendRate, TrailingAnnualDividendYield, TrailingPE, TwoHundredDayAverage, TwoHundredDayAverageChange, TwoHundredDayAverageChangePercent.
### Historical Quotes
```csharp
var securities = await new YahooHistory()
   .Period(Duration.FromDays(10))
   .GetHistoryAsync(new[] { "C", "AAPL" });
var security = securities["C"];
var firstClose = security[0].Close;
```
### Dividend History
```csharp
var securities = await new YahooHistory()
   .Period(Duration.FromDays(10))
   .GetDividendsAsync(new[] { "C", "AAPL" });
```
### Stock Split History
```csharp
var securities = await new YahooHistory()
   .Period(Duration.FromDays(10))
   .GetSplitsAsync(new[] { "C", "AAPL" });
```
### Invalid Rows
Sometimes, yahoo returns broken rows for historical calls, you could decide if these invalid rows is ignored or not by the following statement:
```csharp
Yahoo.IgnoreEmptyRows = true;
```    
### Notes
This library is intended for personal use only, any improper use of this library is not recommended.

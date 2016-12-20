# YahooFinanceApi
A handy Yahoo! Finance api wrapper, based on .NET Standard 1.4

## Features
* Get stock quotes
* Get historical data for stock

## Notes
This library is intended for personal use only, any improper use of this library is not recommended.

## Supported Platforms
* .NET Core 1.0
* .NET framework 4.6.1 or above
* Xamarin.iOS
* Xamarin.Android
* Universal Windows Platform

## How To Install
You can find the package through Nuget

    PM> Install-Package YahooFinanceApi

## How To Use
### Get stock quotes

    var quotes = await Yahoo.Create().Symbol("AAPL", "GOOG").Tag(Tag.LastTradePriceOnly, Tag,ChangeAndPercentChange, Tag.DaysLow, Tag.DaysHigh).GetAsync();
    var aapl = quotes["AAPL"];
    var price = aapl[Tag.LastTradePriceOnly];

### Get historical data for a stock

    // You should be able to query data from various markets including US, HK, TW
    var history = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2016, 1, 1), new DateTime(2016, 7, 1), Period.Daily);
    foreach (var candle in history)
    {
        Console.WriteLine($"DateTime: {candle.DateTime}, Open: {candle.Open}, High: {candle.High}, Low: {candle.Low}, Close: {candle.Close}, Volume: {candle.Volume}, AdjustedClose: {candle.AdjustedClose}");
    }

### Get dividend history for a stock

    // You should be able to query data from various markets including US, HK, TW
    var dividendHistory = await Yahoo.GetHistoricalDividendsAsync("AAPL", new DateTime(2016, 1, 1), new DateTime(2016, 7, 1));
    foreach (var candle in dividendHistory)
    {
        Console.WriteLine($"DateTime: {candle.DateTime}, Dividend: {candle.Dividend}");
    }

### Powered by
* [Flurl](https://github.com/tmenier/Flurl) ([@tmenier](https://github.com/tmenier)) - A simple & elegant fluent-style REST api library 

### License
This library is under [MIT License](https://github.com/salmonthinlion/YahooFinanceApi/blob/master/LICENSE)


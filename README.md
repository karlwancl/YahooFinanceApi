# YahooFinanceApi
[![Build status](https://ci.appveyor.com/api/projects/status/138s6on1y0wnaxms?svg=true)](https://ci.appveyor.com/project/lppkarl/yahoofinanceapi)
[![NuGet](https://img.shields.io/nuget/v/YahooFinanceApi.svg)](https://www.nuget.org/packages/YahooFinanceApi/)
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
For traditional .NET framework user, if you find a "System.Runtime.Serialization.Primitives" missing exception is thrown when using this library, you have to install the missing package manually as nuget does not auto install this reference for you (Bugged?)

## Notice [21/5/2017]
Yahoo has recently changed the URL and the mechanism of retrieving stock history, the old version of the Api no longer works properly. Please make sure that you have updated your version of YahooFinanceApi to get the thing works.

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

### Add reference

    using YahooFinanceApi;

### Get stock quotes

    var quotes = await Yahoo.Symbol("AAPL", "GOOG").Tag(Tag.LastTradePriceOnly, Tag.Open, Tag.DaysHigh, Tag.DaysLow, Tag.PreviousClose).GetAsync();
    var aapl = quotes["AAPL"];
    var price = aapl[Tag.LastTradePriceOnly];

### Get historical data for a stock

    // You should be able to query data from various markets including US, HK, TW
    var history = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2016, 1, 1), new DateTime(2016, 7, 1), Period.Daily);

    // For other market other than US, please specify timezone in order to get a correct range of data, e.g. For AX market
    var history = await Yahoo.GetHistoricalAsync("rxp.ax", new DateTime(2016, 1, 1), new DateTime(2016, 7, 1), Period.Daily, timeZone: "AUS Eastern Standard Time");

    // You can use the IANA timezone or the windows timezone
    // Windows: http://www.xiirus.net/articles/article-_net-convert-datetime-from-one-timezone-to-another-7e44y.aspx

    foreach (var candle in history)
    {
        Console.WriteLine($"DateTime: {candle.DateTime}, Open: {candle.Open}, High: {candle.High}, Low: {candle.Low}, Close: {candle.Close}, Volume: {candle.Volume}, AdjustedClose: {candle.AdjustedClose}");
    }

### Get dividend history for a stock

    // You should be able to query data from various markets including US, HK, TW
    var dividends = await Yahoo.GetDividendsAsync("AAPL", new DateTime(2016, 1, 1), new DateTime(2016, 7, 1));
    foreach (var candle in dividends)
    {
        Console.WriteLine($"DateTime: {candle.DateTime}, Dividend: {candle.Dividend}");
    }

### Get stock split history for a stock
    var splits = await Yahoo.GetSplitsAsync("AAPL", new DateTime(2014, 6, 8), new DateTime(2014, 6, 10));
    foreach (var s in splits)
    {
        Console.WriteLine($"DateTime: {s.DateTime}, AfterSplit: {s.AfterSplit}, BeforeSplit: {s.BeforeSplit}");
    }

### Powered by
* [Flurl](https://github.com/tmenier/Flurl) ([@tmenier](https://github.com/tmenier)) - A simple & elegant fluent-style REST api library 

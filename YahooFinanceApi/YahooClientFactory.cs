﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Flurl.Http;

namespace YahooFinanceApi
{
    internal static class YahooClientFactory
    {
        private static IFlurlClient _client;
        private static string _crumb;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        internal static async Task<(IFlurlClient,string)> GetClientAndCrumbAsync(bool reset, CancellationToken token)
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                if (_client == null || reset)
                {
                    _client = await CreateClientAsync(token).ConfigureAwait(false);
                    _crumb = await GetCrumbAsync(_client, token).ConfigureAwait(false);
                }
            }
            finally
            {
                _semaphore.Release();
            }
            return (_client, _crumb);
        }

        private static async Task<IFlurlClient> CreateClientAsync(CancellationToken token)
        {
            const int MaxRetryCount = 5;
            for (int retryCount = 0; retryCount < MaxRetryCount; retryCount++)
            {
                const string userAgentKey = "User-Agent";
                const string userAgentValue = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";

                // random query to avoid cached response
                var client = new FlurlClient($"https://finance.yahoo.com?{Helper.GetRandomString(8)}")
                    .WithHeader(userAgentKey, userAgentValue)
                    .EnableCookies();
                
                await client.Request().GetAsync(token).ConfigureAwait(false);

                if (client.Cookies?.Count > 0)
                    return client;

                Debug.WriteLine("Failure to create client.");

                await Task.Delay(100, token).ConfigureAwait(false);
            }

            throw new Exception("Failure to create client.");
        }

        private static Task<string> GetCrumbAsync(IFlurlClient client, CancellationToken token) =>
            "https://query1.finance.yahoo.com/v1/test/getcrumb"
            .WithClient(client)
            .GetAsync(token)
            .ReceiveString();
    }
}

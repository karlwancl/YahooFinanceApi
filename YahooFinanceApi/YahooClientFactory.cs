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

        internal static async Task<(IFlurlClient,string)> GetClientAndCrumbAsync(bool reset, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_client == null || reset)
                {
                    _client = await CreateClientAsync(ct).ConfigureAwait(false);
                    _crumb = await GetCrumbAsync(_client, ct).ConfigureAwait(false);
                }
            }
            finally
            {
                _semaphore.Release();
            }
            return (_client, _crumb);
        }

        private static async Task<IFlurlClient> CreateClientAsync(CancellationToken ct)
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
                
                await client.Request().GetAsync(ct).ConfigureAwait(false);

                if (client.Cookies?.Count > 0)
                    return client;

                Debug.WriteLine("Failure to create client.");

                await Task.Delay(100, ct).ConfigureAwait(false);
            }

            throw new Exception("Failure to create client.");
        }

        private static Task<string> GetCrumbAsync(IFlurlClient client, CancellationToken ct) =>
            "https://query1.finance.yahoo.com/v1/test/getcrumb"
            .WithClient(client)
            .GetAsync(ct)
            .ReceiveString();
    }
}

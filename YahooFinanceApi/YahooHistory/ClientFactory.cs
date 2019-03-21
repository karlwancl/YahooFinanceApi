﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Logging;

#nullable enable

namespace YahooFinanceApi
{
    internal static class ClientFactory
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        private static IFlurlClient? Client = null;
        private static string? Crumb;

        internal static async Task<(IFlurlClient,string)> GetClientAndCrumbAsync(bool reset, ILogger logger, CancellationToken ct)
        {
            await Semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (Client == null || reset)
                {
                    Client = await CreateClientAsync(logger, ct).ConfigureAwait(false);
                    if (Client == null)
                        throw new Exception("Null client.");
                    Crumb = await GetCrumbAsync(Client, ct).ConfigureAwait(false);
                }
            }
            finally
            {
                Semaphore.Release();
            }
            if (Client == null)
                throw new Exception("Null client.");
            if (Crumb == null)
                throw new Exception("Null crumb.");
            return (Client, Crumb);
        }

        private static async Task<IFlurlClient> CreateClientAsync(ILogger logger, CancellationToken ct)
        {
            const int MaxRetryCount = 5;
            for (int retryCount = 0; retryCount < MaxRetryCount; retryCount++)
            {
                const string userAgentKey = "User-Agent";
                const string userAgentValue = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";

                // random query to avoid cached response
                var client = new FlurlClient($"https://finance.yahoo.com?{Utility.GetRandomString(8)}")
                    .WithHeader(userAgentKey, userAgentValue)
                    .EnableCookies();
                
                await client.Request().GetAsync(ct).ConfigureAwait(false);

                if (client.Cookies?.Count > 0)
                    return client;

                logger.LogDebug("Failure to create client. Retrying...");

                await Task.Delay(100, ct).ConfigureAwait(false);
            }

            throw new Exception("Failure to create client.");
        }

        private static async Task<string> GetCrumbAsync(IFlurlClient client, CancellationToken ct) =>
            await "https://query1.finance.yahoo.com/v1/test/getcrumb"
            .WithClient(client)
            .GetAsync(ct)
            .ReceiveString()
            .ConfigureAwait(false);
    }
}

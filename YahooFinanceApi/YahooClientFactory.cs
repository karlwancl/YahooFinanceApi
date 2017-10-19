﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;

namespace YahooFinanceApi
{
    public static class YahooClientFactory
    {
        static SemaphoreSlim _clientSemaphore = new SemaphoreSlim(1, 1);
        static SemaphoreSlim _crumbSemaphore = new SemaphoreSlim(1, 1);

        const string CookieUrl = "https://finance.yahoo.com";
        const string CrumbUrl = "https://query1.finance.yahoo.com/v1/test/getcrumb";

        const string UserAgentKey = "User-Agent";
        const string UserAgentValue = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";

        static IFlurlClient _client;
        public static async Task<IFlurlClient> GetClientAsync()
        {
            if (_client == null)
            {
                await _clientSemaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (_client == null)
                        _client = await CreateNewClientAsync().ConfigureAwait(false);
                }
                finally
                {
                    _clientSemaphore.Release();
                }
            }
            return _client;
        }

        static string _crumb;
        public static async Task<string> GetCrumbAsync()
        {
            if (string.IsNullOrEmpty(_crumb))
            {
                await _crumbSemaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (string.IsNullOrEmpty(_crumb))
                        _crumb = await CrumbUrl.WithClient(_client)
                                               .GetAsync()
                                               .ReceiveString()
                                               .ConfigureAwait(false);
				}
                finally
                {
                    _crumbSemaphore.Release();
                }
            }
            return _crumb;
        }

        public static void Reset()
        {
            _client = null;
            _crumb = null;
        }

        static async Task<IFlurlClient> CreateNewClientAsync()
        {
            IFlurlClient temp = null;

            const int MaxRetryCount = 5;
            int retryCount;
            for (retryCount = 0; retryCount < MaxRetryCount; retryCount++)
            {
                temp = new FlurlClient($"{CookieUrl}?{Helper.GetRandomString(8)}")  // Random query param to avoid cached response
                    .WithHeader(UserAgentKey, UserAgentValue)
                    .EnableCookies();   
                
                await temp.Request().GetAsync().ConfigureAwait(false);

                if (temp.Cookies?.Count > 0)
                    break;

                await Task.Delay(100).ConfigureAwait(false);
            }

            if (retryCount == MaxRetryCount)
                throw new Exception("Connection has failed, please try to connect later");

            return temp;
        }
    }
}

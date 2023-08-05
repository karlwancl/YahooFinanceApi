using Flurl.Http;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace YahooFinanceApi
{
    /// <summary>
    /// Holds state for Yahoo HTTP calls
    /// </summary>
    internal static class YahooSession
    {
        private static string _crumb;
        private static FlurlCookie _cookie;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Gets or sets the auth crumb.
        /// </summary>
        /// <value>
        /// The crumb.
        /// </value>
        public static string Crumb
        {
            get
            {
                return _crumb;
            }
        }

        /// <summary>
        /// Gets or sets the auth cookie.
        /// </summary>
        /// <value>
        /// The cookie.
        /// </value>
        public static FlurlCookie Cookie
        {
            get
            {
                return _cookie;
            }
        }

        /// <summary>
        /// Initializes the session asynchronously.
        /// </summary>
        /// <param name="token">The cancelation token.</param>
        /// <exception cref="System.Exception">Failure to create client.</exception>
        public static async Task InitAsync(CancellationToken token = default)
        {
            if (_crumb != null)
            {
                return;
            }

            await _semaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                const string userAgentKey = "User-Agent";
                const string userAgentValue = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";

                var response = await "https://fc.yahoo.com"
                    .AllowHttpStatus("404")
                    .WithHeader(userAgentKey, userAgentValue)
                    .GetAsync()
                    .ConfigureAwait(false);

                if (response.Cookies.Count == 0)
                {
                    throw new Exception("Failed to obtain Yahoo auth cookie.");
                }
                else
                {
                    _cookie = response.Cookies[0];

                    _crumb = await "https://query1.finance.yahoo.com/v1/test/getcrumb"
                        .WithCookie(_cookie.Name, _cookie.Value)
                        .WithHeader(userAgentKey, userAgentValue)
                        .GetAsync(token)
                        .ReceiveString();

                    if (string.IsNullOrEmpty(_crumb))
                    {
                        throw new Exception("Failed to retrieve Yahoo crumb.");
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}

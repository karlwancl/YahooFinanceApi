using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace YahooFinanceApi
{
    public sealed partial class Yahoo
    {
        private string[] _symbols;
        private readonly List<string> _fields = new List<string>();

        private Yahoo() { }

        // static!
        public static Yahoo Symbols(params string[] symbols)
        {
            if (symbols == null || symbols.Length == 0 || symbols.Any(x => x == null))
            {
                throw new ArgumentException(nameof(symbols));
            }

            return new Yahoo { _symbols = symbols };
        }

        public Yahoo Fields(params string[] fields)
        {
            if (fields == null || fields.Length == 0 || fields.Any(x => x == null))
            {
                throw new ArgumentException(nameof(fields));
            }

            _fields.AddRange(fields);

            return this;
        }

        public Yahoo Fields(params Field[] fields)
        {
            if (fields == null || fields.Length == 0)
                throw new ArgumentException(nameof(fields));

            _fields.AddRange(fields.Select(f => f.ToString()));

            return this;
        }

        public async Task<IReadOnlyDictionary<string, Security>> QueryAsync(CancellationToken token = default)
        {
            if (!_symbols.Any())
            {
                throw new InvalidOperationException("Symbols must be set before this method is called.");
            }

            var duplicateSymbol = _symbols.Duplicates().FirstOrDefault();
            if (duplicateSymbol != null)
            {
                throw new InvalidOperationException($"Symbols contain a duplicate: {duplicateSymbol}.");
            }

            var url = "https://query1.finance.yahoo.com/v7/finance/quote"
                .SetQueryParam("symbols", string.Join(",", _symbols));

            if (_fields.Count > 0)
            {
                var duplicateField = _fields.Duplicates().FirstOrDefault();
                if (duplicateField != null)
                {
                    throw new InvalidOperationException($"Fields contain a duplicate: {duplicateField}.");
                }

                url = url.SetQueryParam("fields", string.Join(",", _fields.Select(s => s.ToLowerCamel())));
            }

            await YahooSession.InitAsync(token);

            url.SetQueryParam("crumb", YahooSession.Crumb);

            // Invalid symbols as part of a request are ignored by Yahoo.
            // So the number of symbols returned may be less than requested.
            // If there are no valid symbols, an exception is thrown by Flurl.
            // This exception is caught (below) and an empty dictionary is returned.
            // There seems to be no easy way to reliably identify changed symbols.

            dynamic data = null;

            try
            {
                data = await url
                    .WithCookie(YahooSession.Cookie.Name, YahooSession.Cookie.Value)
                    .GetAsync(token)
                    .ReceiveJson()
                    .ConfigureAwait(false);
            }
            catch (FlurlHttpException ex)
            {
                if (ex.Call.Response.StatusCode == (int)System.Net.HttpStatusCode.NotFound)
                {
                    return new Dictionary<string, Security>();
                }
                else
                {
                    throw;
                }
            }

            var response = data.quoteResponse;

            var error = response.error;
            if (error != null)
            {
                throw new InvalidDataException($"An error was returned by Yahoo: {error}");
            }

            var securities = new Dictionary<string, Security>();

            foreach (IDictionary<string, dynamic> map in response.result)
            {
                // Change the Yahoo field names to start with upper case.
                var pascalDictionary = map.ToDictionary(x => x.Key.ToPascal(), x => x.Value);
                securities.Add(pascalDictionary["Symbol"], new Security(pascalDictionary));
            }

            return securities;
        }
    }
}

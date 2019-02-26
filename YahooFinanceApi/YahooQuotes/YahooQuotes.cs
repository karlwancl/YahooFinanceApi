using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace YahooFinanceApi
{
    public sealed class YahooQuotes
    {
        private string[] symbols;
        private readonly List<string> fields = new List<string>();

        private YahooQuotes() { }

        // static!
        public static YahooQuotes Symbols(params string[] symbols)
        {
            if (symbols == null || symbols.Length == 0 || symbols.Any(x => string.IsNullOrEmpty(x)))
               throw new ArgumentException(nameof(symbols));

            return new YahooQuotes { symbols = symbols };
        }

        public YahooQuotes Fields(params string[] fields)
        {
            if (fields == null || fields.Length == 0 || fields.Any(x => string.IsNullOrEmpty(x)))
                throw new ArgumentException(nameof(fields));

            this.fields.AddRange(fields);

            return this;
        }

        public YahooQuotes Fields(params Field[] fields)
        {
            if (fields == null || fields.Length == 0)
                throw new ArgumentException(nameof(fields));

            this.fields.AddRange(fields.Select(f => f.ToString()));

            return this;
        }

        public async Task<IReadOnlyDictionary<string, Security>> GetAsync(CancellationToken ct = default)
        {
            var duplicateSymbol = symbols.Duplicates().FirstOrDefault();
            if (duplicateSymbol != null)
                throw new ArgumentException($"Duplicate symbol: {duplicateSymbol}.");

            var url = "https://query1.finance.yahoo.com/v7/finance/quote"
                .SetQueryParam("symbols", string.Join(",", symbols), true); 
            // IsEncoded = true: do not encode commas

            if (fields.Any())
            {
                var duplicateField = fields.Duplicates().FirstOrDefault();
                if (duplicateField != null)
                    throw new ArgumentException($"Duplicate field: {duplicateField}.");

                url = url.SetQueryParam("fields", string.Join(",", fields.Select(s => s.ToLowerCamel())), true);
                // IsEncoded = true: do not encode commas
            }

            Debug.WriteLine(url);

            // Invalid symbols as part of a request are ignored by Yahoo.
            // So the number of symbols returned may be less than requested.
            // If there are no valid symbols, an exception is thrown by Flurl.
            // This exception is caught (below) and an empty dictionary is returned.

            // There seems to be no easy way to identify changed symbols.

            dynamic expando = null;

            try
            {
                expando = await url
                    .GetAsync(ct)
                    .ReceiveJson() // ExpandoObject
                    .ConfigureAwait(false);
            }
            catch (FlurlHttpException ex)
            {   
                if (ex.Call.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return new Dictionary<string, Security>(); // Return empty dictionary if symbol not found.
                else throw;
            }

            var quoteExpando = expando.quoteResponse;

            if (quoteExpando.error != null)
                throw new InvalidDataException($"QueryAsync error: {quoteExpando.error}");

            var securities = new Dictionary<string, Security>();

            foreach (IDictionary<string, dynamic> dictionary in quoteExpando.result)
            {
                // Change the Yahoo field names to start with upper case.
                var pascalDictionary = dictionary.ToDictionary(x => x.Key.ToPascal(), x => x.Value);
                securities.Add(pascalDictionary["Symbol"], new Security(pascalDictionary));
            }

            return securities;
        }

    }
}

using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace YahooFinanceApi
{
    // Invalid symbols are often, but not always, ignored by Yahoo.
    // So the number of symbols returned may be less than requested.
    // There seems to be no easy way to identify changed symbols.

    public sealed class YahooQuotes
    {
        private readonly CancellationToken ct;
        private readonly List<string> fields = new List<string>();

        public YahooQuotes(CancellationToken ct = default) => this.ct = ct;

        public YahooQuotes Fields(params Field[] fields) => Fields(fields?.ToList());
        public YahooQuotes Fields(IList<Field> fields) => Fields(fields?.Select(f => f.ToString()).ToList());
        public YahooQuotes Fields(params string[] fields) => Fields(fields?.ToList());
        public YahooQuotes Fields(IList<string> fields)
        {
            if (fields == null || fields.Any(f => f == null))
                throw new ArgumentException(nameof(fields));
            if (fields.Any(x => string.IsNullOrWhiteSpace(x) || !fields.Any()))
                throw new ArgumentException(nameof(fields));
            this.fields.AddRange(fields);
            var duplicates = this.fields.CaseInsensitiveDuplicates().FirstOrDefault();
            if (duplicates != null)
                throw new ArgumentException($"Duplicate field: {duplicates}.", nameof(fields));
            return this;
        }

        public async Task<Security?> GetAsync(string symbol)
        {
            dynamic expando;

            try
            {
                expando = await MakeRequest(new[] { symbol }, fields).ConfigureAwait(false);
            }
            catch (FlurlHttpException ex) when(ex.Call.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // invalid symbol
            }

            dynamic quoteExpando = expando.quoteResponse;

            if (quoteExpando.error != null)
                throw new InvalidDataException($"QueryAsync error: {quoteExpando.error}");

            IDictionary<string, dynamic> dictionary = quoteExpando.result[0];

            var security = new Security(dictionary);

            return security;
        }

        public async Task<Dictionary<string, Security?>> GetAsync(IList<string> symbols)
        {
            var securities = new Dictionary<string, Security?>(StringComparer.OrdinalIgnoreCase);
            foreach (var symbol in symbols)
                securities.Add(symbol, null);

            dynamic expando;

            try
            {
                expando = await MakeRequest(symbols, fields).ConfigureAwait(false);
            }
            catch (FlurlHttpException ex) when (ex.Call.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If there are no valid symbols, this exception is thrown by Flurl
                return securities;
            }

            dynamic quoteExpando = expando.quoteResponse;

            if (quoteExpando.error != null)
                throw new InvalidDataException($"QueryAsync error: {quoteExpando.error}");

            foreach (IDictionary<string, dynamic> dictionary in quoteExpando.result)
                securities[dictionary["symbol"]] = new Security(dictionary);

            return securities;
        }

        private async Task<dynamic> MakeRequest(IList<string> symbols, IList<string> fields)
        {
            if (symbols == null || symbols.Any(x => x == null))
                throw new ArgumentNullException(nameof(symbols));
            if (symbols.Any(x => string.IsNullOrWhiteSpace(x)) || !symbols.Any())
                throw new ArgumentException(nameof(symbols));
            var duplicateSymbol = symbols.CaseInsensitiveDuplicates().FirstOrDefault();
            if (duplicateSymbol != null)
                throw new ArgumentException($"Duplicate symbol: {duplicateSymbol}.");

            // IsEncoded = true: do not encode commas
            var url = "https://query2.finance.yahoo.com/v7/finance/quote"
                .SetQueryParam("symbols", string.Join(",", symbols), true);

            if (fields.Any())
                url = url.SetQueryParam("fields", string.Join(",", fields), true);

            Debug.WriteLine(url);

            return await url
                .GetAsync(ct)
                .ReceiveJson() // ExpandoObject
                .ConfigureAwait(false);
        }
    }
}

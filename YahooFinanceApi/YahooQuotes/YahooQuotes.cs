using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace YahooFinanceApi
{
    // Invalid symbols are often, but not always, ignored by Yahoo.
    // So the number of symbols returned may be less than requested.

    public sealed class YahooQuotes
    {
        private readonly ILogger Logger;
        private readonly CancellationToken Ct;
        private readonly List<string> FieldNames = new List<string>();

        public YahooQuotes(ILogger<YahooQuotes>? logger = null, CancellationToken ct = default)
        {
            Logger = logger ?? NullLogger<YahooQuotes>.Instance;
            Ct = ct;
        }

        public YahooQuotes Fields(params Field[] fields) => Fields(fields.ToList());
        public YahooQuotes Fields(IList<Field> fields) => Fields(fields.Select(f => f.ToString()).ToList());
        public YahooQuotes Fields(params string[] fields) => Fields(fields.ToList());
        public YahooQuotes Fields(IList<string> fields)
        {
            if (!fields.Any() || fields.Any(x => string.IsNullOrWhiteSpace(x)))
                throw new ArgumentException(nameof(fields));
            FieldNames.AddRange(fields);
            var duplicate = FieldNames.CaseInsensitiveDuplicates().FirstOrDefault();
            if (duplicate != null)
                throw new ArgumentException($"Duplicate field: {duplicate}.", nameof(fields));
            return this;
        }

        public async Task<Security?> GetAsync(string symbol)
        {
            dynamic expando;

            try
            {
                expando = await MakeRequest(new[] { symbol }, FieldNames).ConfigureAwait(false);
            }
            catch (FlurlHttpException ex) when(ex.Call.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // invalid symbol
            }

            dynamic quoteExpando = expando.quoteResponse;

            if (quoteExpando.error != null)
                throw new InvalidDataException($"GetAsync error: {quoteExpando.error}");

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
                expando = await MakeRequest(symbols, FieldNames).ConfigureAwait(false);
            }
            catch (FlurlHttpException ex) when (ex.Call.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If there are no valid symbols, this exception is thrown by Flurl
                return securities;
            }

            dynamic quoteExpando = expando.quoteResponse;

            if (quoteExpando.error != null)
                throw new InvalidDataException($"GetAsync error: {quoteExpando.error}");

            foreach (IDictionary<string, dynamic> dictionary in quoteExpando.result)
                securities[dictionary["symbol"]] = new Security(dictionary);

            return securities;
        }

        private async Task<dynamic> MakeRequest(IList<string> symbols, IList<string> fields)
        {
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

            Logger.LogInformation(url);

            return await url
                .GetAsync(Ct)
                .ReceiveJson() // ExpandoObject
                .ConfigureAwait(false);
        }
    }
}

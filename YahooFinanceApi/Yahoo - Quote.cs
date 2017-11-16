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
    public partial class Yahoo
    {
        private List<string> symbols;
        private readonly HashSet<string> fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private Yahoo() { }

        // static!
        public static Yahoo Symbols(params string[] symbols)
        {
            if (symbols == null || symbols.Length == 0 || symbols.Any(x => x == null))
                throw new ArgumentException(nameof(symbols));

            return new Yahoo { symbols = new List<string>(symbols) };
        }

        public Yahoo Fields(params string[] fields)
        {
            if (fields == null || fields.Length == 0 || fields.Any(x => x == null))
                throw new ArgumentException(nameof(fields));

            this.fields.UnionWith(fields.Select(f => f.ToLowerCamel()));

            return this;
        }

        public Yahoo Fields(params Field[] fields)
        {
            if (fields == null || fields.Length == 0)
                throw new ArgumentException(nameof(fields));

            this.fields.UnionWith(fields.Select(f => f.ToString().ToLowerCamel()));

            return this;
        }

        public async Task<Dictionary<string, Security>> QueryAsync(CancellationToken token = default(CancellationToken))
        {
            if (!symbols.Any())
                throw new ArgumentException("No symbols indicated.");

            var url = "https://query1.finance.yahoo.com/v7/finance/quote"
                .SetQueryParam("symbols", string.Join(",", symbols));

            if (fields.Any())
                url = url.SetQueryParam("fields", string.Join(",", fields));

            Debug.WriteLine(url);

            var result = await url
                .GetAsync(token)
                .ReceiveJson() // expandoObject
                .ConfigureAwait(false);

            var quoteResponse = result.quoteResponse;

            var error = quoteResponse.error;
            if (error != null)
                throw new InvalidDataException($"QueryAsync error: {error}");

            if (quoteResponse.result.Count != symbols.Count)
                throw new InvalidDataException($"Received {quoteResponse.result.Count}/{symbols.Count} symbols.");

            var securities = new Dictionary<string, Security>();

            // Note that the returned symbol (result) may be different from the requested symbol (key).
            for (var i = 0; i < symbols.Count; i++)
                securities.Add(symbols[i], new Security(quoteResponse.result[i]));

            return securities;
        }
    }
}

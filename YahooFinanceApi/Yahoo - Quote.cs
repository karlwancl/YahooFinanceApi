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
        private string[] symbols;
        private readonly List<string> fields = new List<string>();

        private Yahoo() { }

        // static!
        public static Yahoo Symbols(params string[] symbols)
        {
            if (symbols == null || symbols.Length == 0 || symbols.Any(x => x == null))
                throw new ArgumentException(nameof(symbols));

            return new Yahoo { symbols = symbols };
        }

        public Yahoo Fields(params string[] fields)
        {
            if (fields == null || fields.Length == 0 || fields.Any(x => x == null))
                throw new ArgumentException(nameof(fields));

            this.fields.AddRange(fields);

            return this;
        }

        public Yahoo Fields(params Field[] fields)
        {
            if (fields == null || fields.Length == 0)
                throw new ArgumentException(nameof(fields));

            this.fields.AddRange(fields.Select(f => f.ToString()));

            return this;
        }

        public async Task<Dictionary<string, Security>> QueryAsync(CancellationToken token = default)
        {
            if (!symbols.Any())
                throw new ArgumentException("No symbols indicated.");

            var duplicateSymbol = symbols.Duplicates().FirstOrDefault();
            if (duplicateSymbol != null)
                throw new ArgumentException($"Duplicate symbol: {duplicateSymbol}.");

            var url = "https://query1.finance.yahoo.com/v7/finance/quote"
                .SetQueryParam("symbols", string.Join(",", symbols));

            if (fields.Any())
            {
                var duplicateField = fields.Duplicates().FirstOrDefault();
                if (duplicateField != null)
                    throw new ArgumentException($"Duplicate field: {duplicateField}.");

                url = url.SetQueryParam("fields", string.Join(",", fields.Select(s => s.ToLowerCamel())));
            }

            Debug.WriteLine(url);

            var result = await url
                .GetAsync(token)
                .ReceiveJson() // ExpandoObject
                .ConfigureAwait(false);

            var quoteResponse = result.quoteResponse;

            var error = quoteResponse.error;
            if (error != null)
                throw new InvalidDataException($"QueryAsync error: {error}");

            if (quoteResponse.result.Count != symbols.Count())
                throw new InvalidDataException($"Received {quoteResponse.result.Count}/{symbols.Count()} symbols.");

            var securities = new Dictionary<string, Security>();

            // Note that the symbol returned may be different from the symbol requested (the key).
            for (var i = 0; i < symbols.Count(); i++)
                securities.Add(symbols[i], new Security(quoteResponse.result[i]));

            return securities;
        }

    }
}

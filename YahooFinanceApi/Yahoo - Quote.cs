using CsvHelper;
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
        private IList<string> symbols, fields;

        private Yahoo() { }

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

            this.fields = fields;

            return this;
        }

        public async Task<IDictionary<string, IDictionary<string, dynamic>>> 
            QueryAsync(CancellationToken token = default(CancellationToken))
        {
            if (!symbols.Any())
                throw new ArgumentException("No symbols specified.");

            var url = "https://query1.finance.yahoo.com/v7/finance/quote"
                .SetQueryParam("symbols", string.Join(",", symbols));

            if (fields != null && fields.Any())
                url = url.SetQueryParam("fields", string.Join(",", fields));

            Debug.WriteLine(url);

            var result = await url
                .GetAsync(token)
                .ReceiveJson() // expandoObject
                .ConfigureAwait(false);

            var quoteResponse = result.quoteResponse;

            var error = quoteResponse.error;
            if (error != null)
                throw new InvalidDataException($"Yahoo.GetJsonAsync() error: {error}");

            var dictionary = new Dictionary<string, IDictionary<string, dynamic>>();

            foreach (var security in quoteResponse.result)
                dictionary.Add(security.symbol, security);

            return dictionary;
        }
    }


}

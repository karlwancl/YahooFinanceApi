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

        public async Task<IDictionary<string, Security>> QueryAsync(CancellationToken token = default)
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


            // Invalid symbols are simply ignored!
            // Except when there are no valid symbols, when an exception os thrown.
            // The excpetion is caught here and an empty list is returned.
            // So the number of symbols returned may be less than requested.
            // And there is no easy way to reliably identify changed symbols.

            dynamic result = null;

            try
            {
                result = await url
                    .GetAsync(token)
                    .ReceiveJson() // ExpandoObject
                    .ConfigureAwait(false);
            }
            catch (FlurlHttpException ex) // when there ared no valid symbols, this exception is thrown
            {
                if (ex.Call.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return new Dictionary<string, Security>();
                else throw;
            }

            var quoteResponse = result.quoteResponse;

            var error = quoteResponse.error;
            if (error != null)
                throw new InvalidDataException($"QueryAsync error: {error}");

            var securities = new Dictionary<string, Security>();

            foreach (IDictionary<string, dynamic> security in quoteResponse.result)
                securities.Add(security["symbol"], new Security(security));

            return securities;
        }

    }
}

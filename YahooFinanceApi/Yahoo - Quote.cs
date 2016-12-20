using CsvHelper;
using CsvHelper.Configuration;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YahooFinanceApi
{
    public static partial class Yahoo
    {
        private const string YahooFinanceQuoteUrl = "http://finance.yahoo.com/d/quotes.csv";

        private const string FormatTag = "f";

        public static async Task<IDictionary<string, IDictionary<Tag, string>>> GetAsync(this Builder builder, CancellationToken token = default(CancellationToken))
        {
            var symbols = builder.Symbols;
            var tags = builder.Tags;

            using (var s = await YahooFinanceQuoteUrl
                .SetQueryParam(SymbolTag, string.Join("+", symbols))
                .SetQueryParam(FormatTag, string.Join("", tags.Select(t => t.Name())))
                .GetAsync(token)
                .ReceiveStream()
                .ConfigureAwait(false))
            using (var sr = new StreamReader(s))
            using (var csvReader = new CsvReader(sr, new CsvConfiguration { HasHeaderRecord = false }))
            {
                var output = new Dictionary<string, IDictionary<Tag, string>>();
                int currentSymbolIndex = 0;
                while (csvReader.Read())
                {
                    var outputPerSymbol = new Dictionary<Tag, string>();
                    var row = csvReader.CurrentRecord;
                    for (int i = 0; i < tags.Count(); i++)
                        outputPerSymbol.Add(tags[i], row[i]);
                    output.Add(symbols[currentSymbolIndex++], outputPerSymbol);
                }
                return output;
            }
        }
    }
}

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
    public static partial class Yahoo
    {
        public static Builder Symbol(params string[] symbols) => new Builder(symbols, null);

        public class Builder
        {
            readonly IList<string> _symbols;
            readonly IList<Tag> _tags;

            internal Builder(IList<string> symbols = null, IList<Tag> tags = null)
            {
                _symbols = symbols ?? new List<string>();
                _tags = tags ?? new List<Tag>();
            }

            public Builder Symbol(params string[] symbols)
            {
                foreach (var symbol in symbols)
                    _symbols.Add(symbol);
                return new Builder(_symbols, _tags);
            }

            [Obsolete("The tag method is useless now, please use Yahoo.Symbol(<symbolName>).QueryAsync()[<symbolName>][<tagName>] instead")]
            public Builder Tag(params Tag[] tags)
            {
                foreach (var tag in tags)
                    _tags.Add(tag);
                return new Builder(_symbols, _tags);
            }

            internal IReadOnlyList<string> Symbols => _symbols.ToList();

            internal IReadOnlyList<Tag> Tags => _tags.ToList();

            [Obsolete("The service is terminated by Yahoo, please use QueryAsync instead")]
            public async Task<IDictionary<string, IDictionary<Tag, string>>> GetAsync(CancellationToken token = default(CancellationToken))
            {
                var url = "https://download.finance.yahoo.com/d/quotes.csv"
                    .SetQueryParam("s", string.Join("+", _symbols))
                    .SetQueryParam("f", string.Join("", _tags.Select(t => t.Name())));

                Debug.WriteLine(url);

                using (var s = await url
                    .GetAsync(token)
                    .ReceiveStream()
                    .ConfigureAwait(false))
                using (var sr = new StreamReader(s))
                using (var csvReader = new CsvReader(sr))
                {
                    var output = new Dictionary<string, IDictionary<Tag, string>>();
                    int currentSymbolIndex = 0;
                    while (csvReader.Read())
                    {
                        var outputPerSymbol = new Dictionary<Tag, string>();
                        var row = csvReader.Context.Record;
                        for (int i = 0; i < _tags.Count(); i++)
                            outputPerSymbol.Add(_tags[i], row[i]);
                        output.Add(_symbols[currentSymbolIndex++], outputPerSymbol);
                    }
                    return output;
                }
            }

            public async Task<IDictionary<string, IDictionary<string, string>>> QueryAsync(CancellationToken token = default(CancellationToken))
            {
                if (!_symbols.Any())
                    throw new ArgumentException("No symbols specified.");

                var url = "https://query1.finance.yahoo.com/v7/finance/quote"
                    .SetQueryParam("symbols", string.Join(",", _symbols));

                Debug.WriteLine(url);

                var result = await url
                    .GetAsync(token)
                    .ReceiveJson<dynamic>()
                    .ConfigureAwait(false);

                var quoteResponse = result["quoteResponse"];

                var error = quoteResponse["error"].ToObject<string>();
                if (error != null)
                    throw new InvalidDataException($"Yahoo.GetJsonAsync() error: {error}");

                var securities = quoteResponse["result"];

                var dictionary = new Dictionary<string, IDictionary<string, string>>();

                foreach (var security in securities)
                {
                    string symbol = security["symbol"].ToObject<string>();

                    var tagData = new Dictionary<string, string>();

                    foreach (var tag in security)
                        tagData.Add(tag.Name, tag.Value.ToObject<string>());

                    dictionary.Add(symbol, tagData);
                }

                return dictionary;
            }
        }
    }
}

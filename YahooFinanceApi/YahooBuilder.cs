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
        public static Builder Symbol(params string[] symbols) => new Builder(symbols, null);

        public class Builder
        {
            private const string YahooFinanceQuoteUrl = "http://finance.yahoo.com/d/quotes.csv";
            private const string SymbolTag = "s";
            private const string FormatTag = "f";

            private IList<string> _symbols;
            private IList<Tag> _tags;

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

            public Builder Tag(params Tag[] tags)
            {
                foreach (var tag in tags)
                    _tags.Add(tag);
                return new Builder(_symbols, _tags);
            }

            internal IReadOnlyList<string> Symbols => _symbols.ToList();

            internal IReadOnlyList<Tag> Tags => _tags.ToList();

            public async Task<IDictionary<string, IDictionary<Tag, string>>> GetAsync(CancellationToken token = default(CancellationToken))
            {
                using (var s = await YahooFinanceQuoteUrl
                    .SetQueryParam(SymbolTag, string.Join("+", _symbols))
                    .SetQueryParam(FormatTag, string.Join("", _tags.Select(t => t.Name())))
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
                        for (int i = 0; i < _tags.Count(); i++)
                            outputPerSymbol.Add(_tags[i], row[i]);
                        output.Add(_symbols[currentSymbolIndex++], outputPerSymbol);
                    }
                    return output;
                }
            }
        }
    }
}

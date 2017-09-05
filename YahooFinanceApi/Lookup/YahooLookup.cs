using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YahooFinanceApi.Lookup
{
    public static class YahooLookup
    {
        const string LookupUrl = "https://finance.yahoo.com/_finance_doubledown/api/resource/finance.yfinlist.symbol_lookup";
        const string SearchTag = "s";
        const string LookupTypeTag = "t";
        const string MarketTag = "m";
        const string OffsetTag = "b";
        const string IsIncludeSimilarTag = "p";

        public static async Task<IEnumerable<LookupSymbol>> GetLookupSymbolsAsync(string search, LookupType lookupType, MarketType marketType, CancellationToken token = default(CancellationToken))
        {
            Func<string, int, Task<Stream>> responseFunc = (s, i) => GetSymbolListResponseStreamAsync(s, lookupType, marketType, i, token);
            var childSearches = await GetLookupSearchesAsync(search, lookupType, marketType, token);

            var list = new List<LookupSymbol>();
            foreach (var childSearch in childSearches)
            {
                int offset = 0;
                while (true)
                {
                    using (var s = await responseFunc(childSearch, offset))
                    using (var sr = new StreamReader(s))
                    {
                        var o = JObject.Parse(sr.ReadToEnd());
                        var symbols = o["result"].ToArray();
                        list.AddRange(symbols.Select(sym => JsonConvert.DeserializeObject<LookupSymbol>(sym.ToString())));

                        var nav = o["navLink"];
                        var navAsDict = (IDictionary<string, JToken>)nav;
                        if (!navAsDict.ContainsKey("nextPage") || !navAsDict.ContainsKey("lastPage"))
                            break;

                        offset = nav["nextPage"]["start"].Value<int>();
                    }
                }
            }

            return list;
        }

        public static async Task<IEnumerable<string>> GetLookupSearchesAsync(string search, LookupType lookupType, MarketType marketType, CancellationToken token = default(CancellationToken))
        {
            Func<int, Task<Stream>> responseFunc = i => GetSymbolListResponseStreamAsync(search, lookupType, marketType, i, token);
            var list = new List<string>();

            if (await IsLookupSizeTooLargeAsync())
            {
                foreach (var subSearch in search.AddSuffixes())
                    list.AddRange(await GetLookupSearchesAsync(subSearch, lookupType, marketType, token));
                return list;
            }

            return new string[] { search };

            async Task<bool> IsLookupSizeTooLargeAsync()
            {
                const int LookupSizeLimit = 2000;

                using (var s = await responseFunc(0))
                using (var sr = new StreamReader(s))
                {
                    var o = JObject.Parse(sr.ReadToEnd());
                    var nav = o["navLink"];
                    var navAsDict = (IDictionary<string, JToken>)nav;
                    if (!navAsDict.ContainsKey("lastPage"))
                        return false;

                    return nav["lastPage"]["start"].Value<int>() >= LookupSizeLimit;
                }
            }
        }

        public static async Task<Stream> GetSymbolListResponseStreamAsync(string search, LookupType lookupType, MarketType marketType, int offset, CancellationToken token = default(CancellationToken))
            => await new StringBuilder()
                .Append(LookupUrl)
                .Append($";{SearchTag}={search}")
                .Append($";{LookupTypeTag}={lookupType.Name()}")
                .Append($";{MarketTag}={marketType.Name()}")
                .Append($";{OffsetTag}={offset}")
                .Append($";{IsIncludeSimilarTag}=1")
                .ToString()
                .GetStreamAsync(token);
    }
}

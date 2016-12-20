using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YahooFinanceApi
{
    public static partial class Yahoo
    {
        public static Builder Create() => new Builder();

        public class Builder
        {
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
        }
    }
}

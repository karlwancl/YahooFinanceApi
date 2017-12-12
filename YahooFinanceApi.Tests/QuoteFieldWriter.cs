using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace YahooFinanceApi.Tests
{
    public class QuoteFieldWriter
    {
        private readonly Action<string> Write;
        public QuoteFieldWriter(ITestOutputHelper output) => Write = output.WriteLine;

        private async Task<List<KeyValuePair<string, dynamic>>> GetFields()
        {
            return (await Yahoo.Symbols("C").QueryAsync())
                .Single()
                .Value
                .Fields
                .OrderBy(x => x.Key)
                .ToList();
        }

        [Fact]
        public async Task MakeEnumList()
        {
            var fields = await GetFields();

            Write("// Fields.cs enums. This list was generated automatically. These names have been defined by Yahoo.");
            Write(String.Join("," + Environment.NewLine, fields.Select(x => x.Key)));
            Write(Environment.NewLine);
        }

        [Fact]
        public async Task MakePropertyList()
        {
            var fields = await GetFields();

            Write("// Security.cs: This list was generated automatically. These names and types have been defined by Yahoo.");
            foreach (var field in fields)
                Write($"public {field.Value.GetType().Name} {field.Key} => this[\"{field.Key}\"];");
            Write(Environment.NewLine);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

#nullable enable

namespace YahooFinanceApi.Tests
{
    public class QuoteFieldWriter
    {
        private readonly Action<string> Write;
        public QuoteFieldWriter(ITestOutputHelper output) => Write = output.WriteLine;

        private async Task<List<KeyValuePair<string, dynamic>>> GetFields()
        {
            var result = await new YahooQuotes().GetAsync("C");
            if (result == null)
                throw new Exception("invalid symbol");
            return result.Fields.OrderBy(x => x.Key).ToList();
        }

        [Fact]
        public async Task MakeEnumList()
        {
            var fields = await GetFields();

            Write($"// Fields.cs: {fields.Count}. This list was generated automatically from names been defined by Yahoo.");
            Write(string.Join("," + Environment.NewLine, fields.Select(x => x.Key)));
            Write(Environment.NewLine);
        }

        [Fact]
        public async Task MakePropertyList()
        {
            var fields = await GetFields();

            Write($"// Security.cs: {fields.Count}. This list was generated automatically from names defined by Yahoo.");
            foreach (var field in fields)
                Write($"public {field.Value.GetType().Name}? {field.Key} => Get();");
            Write(Environment.NewLine);
        }

        [Fact]
        public async Task CompareEnums()
        {
            var currentList = Enum.GetNames(typeof(Field)).ToList();
            var newList = (await GetFields()).Select(f => f.Key).ToList();

            var combinedList = currentList.ToList();
            combinedList.AddRange(newList);
            combinedList = combinedList.Distinct().OrderBy(s => s).ToList();

            Write("Field updates:");

            foreach (var item in combinedList)
            {
                if (currentList.Find(x => x == item) == null)
                    Write(item + " new");
                if (newList.Find(x => x == item) == null)
                    Write(item + " removed");
            }
        }
    }
}

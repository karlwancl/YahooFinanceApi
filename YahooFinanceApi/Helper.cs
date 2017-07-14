using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;

namespace YahooFinanceApi
{
    static class Helper
    {
        readonly static DateTime DefaultUnixDateTime = new DateTime(1970, 1, 1);

        public static string Name<T>(this T @enum)
        {
            string name = @enum.ToString();
            if (typeof(T).GetMember(name).First().GetCustomAttribute(typeof(EnumMemberAttribute)) is EnumMemberAttribute attr && attr.IsValueSetExplicitly)
                name = attr.Value;
            return name;
        }

        public static string ToUnixTimestamp(this DateTime dateTime)
            => ((DateTimeOffset)dateTime).ToUnixTimeSeconds().ToString();

        public static string GetRandomString(int length)
        {
            const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            return string.Join("", Enumerable.Range(0, length).Select(i => Chars[new Random().Next(Chars.Length)]));
        }
    }
}

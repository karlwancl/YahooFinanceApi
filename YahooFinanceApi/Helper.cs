using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace YahooFinanceApi
{
    static class Helper
    {
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

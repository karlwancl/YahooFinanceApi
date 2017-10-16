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

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly TimeZoneInfo TzEst = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        private static DateTime ConvertTimeFromESTtoUTC(this DateTime dt) => TimeZoneInfo.ConvertTimeToUtc(dt, TzEst);

        internal static string ToUnixTimestamp(this DateTime dt)
            => DateTime.SpecifyKind(dt, DateTimeKind.Unspecified)
                .ConvertTimeFromESTtoUTC()
                .Subtract(Epoch)
                .TotalSeconds
                .ToString("F0");

        /*
        internal static DateTime AdjustIfCurrency(this DateTime dt, string symbol)
        {
            if (symbol.Length == 8 && symbol.EndsWith("=X", StringComparison.OrdinalIgnoreCase))
                dt = dt.AddHours(48); // ???
            return dt;
        }
        */

        public static string GetRandomString(int length)
        {
            const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            return string.Join("", Enumerable.Range(0, length).Select(i => Chars[new Random().Next(Chars.Length)]));
        }
    }
}

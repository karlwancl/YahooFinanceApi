using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace YahooFinanceApi
{
    static class Helper
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static DateTime ConvertTimeFromEstToUtc(this DateTime dt) 
            => TimeZoneInfo.ConvertTimeToUtc(dt, TzEst);

        private static readonly TimeZoneInfo TzEst = TimeZoneInfo
            .GetSystemTimeZones()
            .Where(tz => tz.Id == "Eastern Standard Time" || tz.Id == "America/New_York")
            .Single();

        internal static string ToUnixTimestamp(this DateTime dt)
            => DateTime.SpecifyKind(dt, DateTimeKind.Unspecified)
                .ConvertTimeFromEstToUtc()
                .Subtract(Epoch)
                .TotalSeconds
                .ToString("F0");

        public static string Name<T>(this T @enum)
        {
            string name = @enum.ToString();
            if (typeof(T).GetMember(name).First().GetCustomAttribute(typeof(EnumMemberAttribute)) is EnumMemberAttribute attr && attr.IsValueSetExplicitly)
                name = attr.Value;
            return name;
        }

        public static string GetRandomString(int length)
        {
            const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            return string.Join("", Enumerable.Range(0, length).Select(i => Chars[new Random().Next(Chars.Length)]));
        }
    }
}

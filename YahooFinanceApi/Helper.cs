using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace YahooFinanceApi
{
    static class Helper
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly TimeZoneInfo TzEst = TimeZoneInfo
            .GetSystemTimeZones()
            .Single(tz => tz.Id == "Eastern Standard Time" || tz.Id == "America/New_York");

        private static DateTime ToUtcFrom(this DateTime dt, TimeZoneInfo tzi)
            => TimeZoneInfo.ConvertTimeToUtc(dt, tzi);

        internal static DateTime FromEstToUtc(this DateTime dt)
            => DateTime.SpecifyKind(dt, DateTimeKind.Unspecified)
               .ToUtcFrom(TzEst);

        internal static string ToUnixTimestamp(this DateTime dt)
            => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                .Subtract(Epoch)
                .TotalSeconds
                .ToString("F0");

        internal static string Name<T>(this T @enum)
        {
            string name = @enum.ToString();
            if (typeof(T).GetMember(name).First().GetCustomAttribute(typeof(EnumMemberAttribute)) is EnumMemberAttribute attr && attr.IsValueSetExplicitly)
                name = attr.Value;
            return name;
        }

        internal static string GetRandomString(int length)
            => Guid.NewGuid().ToString().Substring(0, length);

    }
}

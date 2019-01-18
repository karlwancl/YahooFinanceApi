using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace YahooFinanceApi
{
    public static class Helper
    {
        private static readonly TimeZoneInfo TzEst = TimeZoneInfo
            .GetSystemTimeZones()
            .Single(tz => tz.Id == "Eastern Standard Time" || tz.Id == "America/New_York");

        private static DateTime ToUtcFrom(this DateTime dt, TimeZoneInfo tzi) =>
            TimeZoneInfo.ConvertTimeToUtc(dt, tzi);

        internal static DateTime FromEstToUtc(this DateTime dt) =>
            DateTime.SpecifyKind(dt, DateTimeKind.Unspecified)
               .ToUtcFrom(TzEst);

        internal static string ToUnixTimestamp(this DateTime dt) =>
            new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc)).ToUnixTimeSeconds().ToString("F0");

        public static DateTime FromUnixTimeSeconds(this long seconds) =>
            DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;

        internal static DateTime ToLocalTimeIn(this DateTime dt, string tzId) =>
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dt, tzId);

        internal static string Name<T>(this T @enum)
        {
            string name = @enum.ToString();
            if (typeof(T).GetMember(name).First().GetCustomAttribute(typeof(EnumMemberAttribute)) is EnumMemberAttribute attr && attr.IsValueSetExplicitly)
                name = attr.Value;
            return name;
        }

        internal static string GetRandomString(int length) =>
            Guid.NewGuid().ToString().Substring(0, length);

        internal static string ToLowerCamel(this string pascal) =>
            pascal.Substring(0, 1).ToLower() + pascal.Substring(1);

        internal static string ToPascal(this string lowerCamel) =>
            lowerCamel.Substring(0, 1).ToUpper() + lowerCamel.Substring(1);

        internal static IEnumerable<string> Duplicates(this IEnumerable<string> items)
        {
            var hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return items.Where(item => !hashSet.Add(item));
        }

    }
}

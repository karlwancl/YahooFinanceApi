using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NodaTime;

namespace YahooFinanceApi
{
    public static class Utility
    {
        public static IClock Clock { get; internal set; } = SystemClock.Instance;

        internal static string GetRandomString(int length) =>
            Guid.NewGuid().ToString().Substring(0, length);
    }

    public static class ExtensionMethods
    {
        public static DateTimeZone ToDateTimeZone(this string name) =>
            DateTimeZoneProviders.Tzdb.GetZoneOrNull(name);

        public static ZonedDateTime ToZonedDateTime(this long unixTimeSeconds, DateTimeZone zone) =>
            Instant.FromUnixTimeSeconds(unixTimeSeconds).InZone(zone);

        internal static string Name<T>(this T @enum) where T : Enum
        {
            string name = @enum.ToString();
            if (typeof(T).GetMember(name).First().GetCustomAttribute(typeof(EnumMemberAttribute)) is EnumMemberAttribute attr && attr.IsValueSetExplicitly)
                name = attr.Value;
            return name;
        }

        internal static string ToLowerCamel(this string pascal) =>
            pascal.Substring(0, 1).ToLower() + pascal.Substring(1);

        internal static string ToPascal(this string lowerCamel) =>
            lowerCamel.Substring(0, 1).ToUpper() + lowerCamel.Substring(1);

        internal static List<string> Duplicates(this IEnumerable<string> strings)
        {
            var hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return strings.Where(str => !hashSet.Add(str)).ToList();
        }

        internal static string ToCommaDelimitedList(this IEnumerable<string> strings) => string.Join(", ", strings);
    }


}

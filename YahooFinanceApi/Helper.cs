using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace YahooFinanceApi
{
    public static class Helper
    {
        public static string Name<T>(this T @enum)
        {
            string name = @enum.ToString();
            if (typeof(T).GetMember(name).First().GetCustomAttribute(typeof(EnumMemberAttribute)) is EnumMemberAttribute attr && attr.IsValueSetExplicitly)
                name = attr.Value;
            return name;
        }

        public static T GetValue<T>(this string str)
            => (T)Convert.ChangeType(str, typeof(T));
    }
}

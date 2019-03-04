using System.Runtime.Serialization;

#nullable enable

namespace YahooFinanceApi
{
    public enum Frequency
    {
        [EnumMember(Value = "d")]
        Daily,

        [EnumMember(Value = "wk")]
        Weekly,

        [EnumMember(Value = "mo")]
        Monthly
    }
}

using System;
using System.Runtime.Serialization;

namespace YahooFinanceApi.Lookup
{
    public enum LookupType
    {
        [EnumMember(Value = "A")]
        All,
        [EnumMember(Value = "S")]
        Stocks,
        [EnumMember(Value = "M")]
        MutualFunds,
        [EnumMember(Value = "E")]
        ETFs,
        [EnumMember(Value = "I")]
        Indices,
        [EnumMember(Value = "F")]
        Futures,
        [EnumMember(Value = "C")]
        Currencies
    }
}

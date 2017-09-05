using System;
using System.Runtime.Serialization;

namespace YahooFinanceApi.Lookup
{
    public enum MarketType
    {
        [EnumMember(Value = "ALL")]
        All,
        [EnumMember(Value = "US")]
        US_Canada
    }
}

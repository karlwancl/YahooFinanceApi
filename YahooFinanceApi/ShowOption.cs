using System.Runtime.Serialization;

namespace YahooFinanceApi
{
    enum ShowOption
    {
        [EnumMember(Value = "history")]
        History,

        [EnumMember(Value = "div")]
        Dividend,

        [EnumMember(Value = "split")]
        Split
    }
}

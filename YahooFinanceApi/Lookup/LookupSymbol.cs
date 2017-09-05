using System;
using System.Collections.Generic;

namespace YahooFinanceApi.Lookup
{
    public class LookupSymbol
    {
        public string Symbol { get; }
        public string CompanyName { get; }
        //public decimal LastValue { get; }
        //public decimal Change { get; }
        //public decimal ChangePercent { get; }
        public string IndustryName { get; }
        public string IndustryLink { get; }
        public string Type { get; }
        public string Exchange { get; }

        public LookupSymbol(string symbol, string companyName, /*decimal lastValue, decimal change, decimal changePercent, */
                      string industryName, string industryLink, string type, string exchange)
        {
            Exchange = exchange;
            Type = type;
            IndustryLink = industryLink;
            IndustryName = industryName;
            //ChangePercent = changePercent;
            //Change = change;
            //LastValue = lastValue;
            CompanyName = companyName;
            Symbol = symbol;
        }
    }
}

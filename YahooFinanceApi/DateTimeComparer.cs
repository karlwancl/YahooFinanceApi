using System;
using System.Collections.Generic;

namespace YahooFinanceApi
{
    public class DateTimeComparer: IComparer<DateTime>
    {
        public bool Ascending { get; }

        public DateTimeComparer(bool ascending)
        {
            Ascending = ascending;
        }

        public int Compare(DateTime x, DateTime y)
        {
            var value = (x > y) ? 1 : (x == y) ? 0 : -1;
            return value * (Ascending ? 1 : -1);
        }
    }
}

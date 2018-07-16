using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowFunctionsTT.Helper
{
    public static class DateExtensions
    {
        public static int GetFromMonthName(this string monthName)
        {
            var month = new string[]
            {
                "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober",
                "November", "Dezember"
            };

            return Array.IndexOf(month, monthName)+1;
        }
    }
}

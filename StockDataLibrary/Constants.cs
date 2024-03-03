using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDataLibrary
{
    internal static class Constants
    {
        /// <summary>
        /// Template for requesting JSON dumps for companies by CIK
        /// </summary>
        public static readonly string SECFilingRequestTemplate = "https://data.sec.gov/api/xbrl/companyfacts/CIK{0}.json"; 
        
    }
}

using System.Net;

namespace StockApp.Utilities
{
    /// <summary>
    /// This class handles requesting, parsing, and assigning data requested from the SEC companyfacts xbrl api
    /// </summary>
    public static class FinancialData
    {
        
        /// <summary>
        /// Requests a filing history of this specific company from SEC edgar using the CIK
        /// </summary>
        /// <param name="client"> This is the httpclient object to use </param>
        /// <param name="CIK"> This is the company identifier on edgar</param>
        /// <returns>We return a string representing the json dump of filing history for this company</returns>
        public static async Task<Task<String>> requestJSONDump(HttpClient client, UInt64 CIK)
        {
            // need to pad the CIK to 10 digits (adding leading zeros)
            String cikstr = String.Format("{0:D10}", CIK);
            String requestURL = "https://data.sec.gov/api/xbrl/companyfacts/CIK" + cikstr+".json";
            // forming the request
            
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestURL);
                request.Headers.Add("Connection", "keep-alive");
                request.Headers.Add("Host", "data.sec.gov");
                request.Headers.Add("User-Agent", "PostmanRuntime/7.32.3");
                
                using HttpResponseMessage response = 
                    await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync();
            } catch (Exception ex)
            {
                Console.WriteLine("encountered exception requesting financial data for CIK: " + cikstr);
                Console.WriteLine(ex.ToString());
                return null!;
            }
            
        }

        public static void parseFinancials(String dataJSON)
        {

        }
    }
}

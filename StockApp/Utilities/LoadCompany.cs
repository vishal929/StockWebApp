using Microsoft.AspNetCore.Connections.Features;
using StockApp.Models;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace StockApp.Utilities
{
    /// <summary>
    /// Class which provides helpers for loading company model data from SEC Edgar database
    /// </summary>
    public static class LoadCompany
    {
        /// <summary>
        /// Requests the json body of company data from SEC Edgar
        /// This will be parsed into a list of company models
        /// </summary>
        /// <param name="httpClient"> This is the url of the request</param>
        /// <returns> Task that results in a json string</returns>
        public static async Task<Task<string>> GetAsyncJSONCompanies(HttpClient httpClient)
        {
            using HttpResponseMessage response = await httpClient.GetAsync("https://www.sec.gov/files/company_tickers_exchange.json");

            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync();
            //Console.WriteLine($"{jsonResponse}\n");
        }
       
        /// <summary>
        /// Helper method that handles a json node containing company data
        /// </summary>
        /// <param name="companyNode"> This is the json parent node containing company metadata for SEC Edgar</param>
        /// <returns>We return a tuple of (CIK,ticker, company name, and exchange)</returns>
        private static Tuple<uint,string,string,string>? handleJSONNode(JsonNode? companyNode)
        {
            if (companyNode == null) { return null; }

            for (int i=0;i< 4; i++)
            {
                if (companyNode[i] == null) { return null; }
            }

            return Tuple.Create((uint)companyNode[0]!, // cik
                companyNode[2]!.ToString(), // ticker
                companyNode[1]!.ToString(), // company name
                companyNode[3]!.ToString()); // exchange
        }
        
        /// <summary>
        /// Function that takes in a json string of company metadata and returns a list of company model objects.
        /// </summary>
        /// <param name="jsonResponse">This is the json string response to the company metadata request</param>
        /// <returns>We return a list of company model objects to use with the app.
        /// In case json parsing fails or the jsonresponse is empty, we return null </returns>
        /// <exception cref="Exception">We throw an exception in the case </exception>
        public static List<CompanyModel>? ParseCompaniesJSON(string jsonResponse)
        {
            // check if the json response is empty
            if (String.IsNullOrEmpty(jsonResponse))
            {
                Console.WriteLine("Json request for company metadata is empty!");
                return null;
            }
            List<CompanyModel> companies = new List<CompanyModel>();
            JsonNode? root = null;
            try
            {
                root = JsonNode.Parse(jsonResponse);
                if (root == null)
                {
                    Console.WriteLine("Unexpected null when parsing company json!");
                    return null;
                }
            } catch (Exception ex)
            {
                Console.Write("Encountered exception" + ex.ToString() + "in parsing company json");
                return null;
            }
            JsonNode? inner = root["data"];
            if (inner == null) { return null!; }
            JsonArray data = inner.AsArray();
            for (int i = 0; i < data.Count(); i++)
            {
                Tuple<uint, string, string, string>? extracted = handleJSONNode(data[i]);
                if (extracted == null) { continue; }
                (uint cik, string ticker, string company_name, string exchange) =
                    extracted;

                // we only want to consider stocks on nyse or nasdaq
                // no OTC for us!
                if (string.Compare(exchange,"Nasdaq")==0 || string.Compare(exchange, "NYSE")==0)
                {
                    companies.Add(new CompanyModel(cik,ticker,company_name));
                }
            }
            
            return companies;

        }


    }
}

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


        public static List<CompanyModel> ParseCompaniesJSON(string jsonResponse)
        {
            List<CompanyModel> companies = new List<CompanyModel>();
            JsonNode root = JsonNode.Parse(jsonResponse);
            JsonArray data = root["data"].AsArray();
            for (int i = 0; i < data.Count(); i++)
            {
                JsonNode companyData = data![i];
                string cik = companyData[0].ToString();
                string ticker = companyData[2].ToString();
                string company_name = companyData[1].ToString();
                string exchange = companyData[3].ToString();
                // we only want to consider stocks on nyse or nasdaq
                // no OTC for us!
                if (string.Compare(exchange,"Nasdaq")==0 || string.Compare(exchange, "NYSE")==0)
                {
                    companies.Add(new CompanyModel(cik,ticker,company_name));
                }
            }
            
            /*
            foreach (var company in companies)
            {
                Console.WriteLine(company.Company_Name);
            }
            */
            return companies;

        }


    }
}

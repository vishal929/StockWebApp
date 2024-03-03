using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml;

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
            String requestURL = String.Format(StockDataLibrary.Constants.SECFilingRequestTemplate, cikstr);
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
        
        /// <summary>
        /// Given a file representing fieldnames of a financial statement, we return the field names
        /// </summary>
        /// <param name="definitionFile"> filepath of a financial statement in the gaap reporting schema</param>
        /// <returns>We return a list of field names</returns>
        public static async Task<List<String>> GetFieldNames(string definitionFile)
        {
            HashSet<String> fieldNames = new HashSet<String>();
            //List<String> fieldNames = new List<String>();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Async = true;

            using (XmlReader reader = XmlReader.Create(definitionFile, settings))
            {
                while (await reader.ReadAsync())
                {
                    // we only grab field names from definitionArc nodes and there are only a certain number of attributes
                    if (!reader.LocalName.Equals("definitionArc") || reader.AttributeCount < 4)
                    {
                        continue;
                    };
                    
                    if (reader.GetAttribute(1).Equals("http://xbrl.org/int/dim/arcrole/domain-member"))
                    {
                        // need to remove the "loc_" part before the field name and we do not want dupes
                        // so we add this to a set
                        string firstField = reader.GetAttribute(3).Substring(4);
                        string secondField = reader.GetAttribute(2).Substring(4);
                        fieldNames.Add(firstField);
                        fieldNames.Add(secondField);
                        //fieldNames.Add(reader.GetAttribute(3).Substring(4));
                        //fieldNames.Add(reader.GetAttribute(2).Substring(4));
                        //Console.WriteLine(reader.GetAttribute(2));
                        //Console.WriteLine(reader.GetAttribute(3).Substring(4));
                    }
                }
            }
            return fieldNames.ToList<String>();
        }
        
        /// <summary>
        /// Helper to just iterate through field names from a statement and then add them to a mappings dictionary
        /// </summary>
        /// <param name="mappings"> gaap mappings from fieldname to financial statement</param>
        /// <param name="fieldnames"> list of fieldnames given</param>
        /// <param name="statement"> associated statement enumeration </param>
        private static void InsertMappingsHelper(Dictionary<String,List<Statement>> mappings, List<String> fieldnames, Statement statement)
        {
            foreach (string fieldname in fieldnames)
            {
                if (!mappings.ContainsKey(fieldname))
                {

                    mappings[fieldname] = new List<Statement>();
                }
                mappings[fieldname].Add(statement);
            }
        }

        /// <summary>
        /// 
        /// Getting GAAP mappings from financial field to statement id based on the year.
        /// Since gaap fieldnames can be included and deprecated from year to year, we need to 
        /// specify the year as a parameter
        /// </summary>
        /// <param name="year"> This is the year of the gaap schema to choose</param>
        /// <returns></returns>
        public static async Task<Dictionary<String, List<Statement>>> GetGaapMappings(int year)
        {
            Dictionary<String, List<Statement>> mappings = new Dictionary<String, List<Statement>>();
            // based on the year we find the template
            // we first point to the root of the statements in the reporting schema
            string stmRoot = Path.Combine("Resources", "GAAPTemplates", year.ToString(), "stm");

            // statement of financial position classified
            string balanceSheetPattern = @"^us-gaap-stm-sfp-cls-def-\w*";
            // statement of income
            string incomeStatementPattern = @"^us-gaap-stm-soi-def-\w*";
            // statement of cash flow
            string cashFlowStatementPattern = @"^us-gaap-stm-scf-dbo-def-\w*";

            Task<List<String>>? balanceSheetAsyncTask = null;
            Task<List<String>>? incomeStatementAsyncTask = null;
            Task<List<String>>? cashFlowStatementAsyncTask = null;

            // matching filenames
            string[] filenames = System.IO.Directory.GetFiles(stmRoot);

            foreach (string file in filenames)
            {
                string basepath = Path.GetFileName(file);
                if (Regex.IsMatch(basepath,balanceSheetPattern))
                {
                    balanceSheetAsyncTask = GetFieldNames(file);
                } else if (Regex.IsMatch(basepath,incomeStatementPattern))
                {
                    incomeStatementAsyncTask = GetFieldNames(file);
                } else if (Regex.IsMatch(basepath,cashFlowStatementPattern))
                {
                    cashFlowStatementAsyncTask = GetFieldNames(file);
                }
                // if tasks are assigned we can stop
                if (balanceSheetAsyncTask!=null && incomeStatementAsyncTask!=null && cashFlowStatementAsyncTask!=null)
                {
                    break;

                }
            }

            // null check (if null then something went terribly wrong!)
            if (balanceSheetAsyncTask==null)
            {
                throw new Exception("Field names for balance sheet could not be grabbed. " +
                "Async task for parsing fields was not assigned! ");
            }
            if (cashFlowStatementAsyncTask==null)
            {
                throw new Exception("Field names for cash flow statement could not be grabbed. " +
                "Async task for parsing fields was not assigned! ");
            }
            if (incomeStatementAsyncTask==null)
            {
                throw new Exception("Field names for income statement could not be grabbed. " +
                "Async task for parsing fields was not assigned! ");
            }

            // adding all mappings to a dictionary
            InsertMappingsHelper(mappings, await balanceSheetAsyncTask, Statement.BalanceSheet);
            InsertMappingsHelper(mappings, await incomeStatementAsyncTask, Statement.IncomeStatement);
            InsertMappingsHelper(mappings, await cashFlowStatementAsyncTask, Statement.CashFlowStatement);

            return mappings;
        }
        
        public static Dictionary<int,Dictionary<String,Statement>> getMappingsFromGaapTemplates()
        {
            // based on which year we are pulling data from, we get a mapping from string to string
            // the key is the year of the template file
            // the value is another dictionary where the key is the reporting field and the
            // the value is which statement it belongs to (balance sheet, income statement, or cash flow statement) as an enum
            // this mapping is obtained by parsing the year-specific gaap template in the resources folder
            return null!;
        }

        public static void parseFinancials(String dataJSON)
        {
            // this function should parse the json dump of reported financials for a company 
            // for now, lets see the maximum number of unique elements that are reported in
            // a 10K or 10Q (this will help with database design)

            // when considering dates, we consider END dates, not filing dates!
            // this means that a 2018 10-k will be considered as representing status in the year 2018
            // not a 2018 filing that represents 2017 status!

            // FOR DB design we can consider the following options to handle flexibility in gaap
            // 1) expand table as new fields are added/removed over the years
            // 2) create a new table for each year (i.e balanceSheet 2017, balanceSheet 2018, etc.)

            // asynchronously getting mappings (since this requires I/O)

            // looking for the us-gaap json element
            JsonNode? root = JsonNode.Parse(dataJSON);
            JsonNode gaap_elements = root["facts"]["us-gaap"];
            JsonObject gaap_obj = gaap_elements.AsObject();
            
            // getting the field items in the gaap specification
            int num_fields = gaap_obj.Count();
            for (int i=0;i<1; i++)
            {
                // the field to process here will be something like "accountspayablecurrent" etc.
                Tuple<string, List<Tuple<long, DateOnly, string, string>>>
                    dataHistory = processField(gaap_obj.ElementAt(i));

                
            }
            
        }

        private static Tuple<String,List<Tuple<Int64,DateOnly,String,String>>> processField(KeyValuePair<String,JsonNode?> field)
        {
            // this function should handle mapping of a field json object
            // specifically, we should map the field to a financial statement
            // Balance Sheet, Cash Flow Statement, or Income Statement

            // after mapping, we need to extract the numeric values, reporting type, and date end
            // reporting type is either 10-Q or 10-K for quarterly or yearly
            // also, we should provide a little metadata for filing, like Q1,Q2,Q3,Q4, or Y for 10-k
            // date end will be the end date that the report covers, not the date filed

            // we should return something like:
            // field_name: [(value, date_end, reportQuarter, reportType), (...), (...), ...]
            String fieldName = field.Key;
            JsonArray values = field.Value["units"]["USD"].AsArray();
            List<Tuple<Int64, DateOnly, String, String>> l = new List<Tuple<Int64, DateOnly, String, String>>();

            for (int i = 0; i < values.Count(); i++)
            {
                JsonObject reportData = values[i].AsObject();
                DateOnly date = DateOnly.ParseExact(reportData["end"].ToString(), "yyyy-mm-dd");
                Int64 value = (long) reportData["val"].AsValue();
                String period = reportData["fp"].ToString();
                String reportType = reportData["form"].ToString();
                l.Add(Tuple.Create(value, date, period, reportType));
            }
            return Tuple.Create(fieldName, l);
        }
    }
}

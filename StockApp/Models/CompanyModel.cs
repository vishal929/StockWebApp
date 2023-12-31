﻿using System.Collections;
using System.Numerics;

namespace StockApp.Models
{
   /// <summary>
   /// <c> CompanyModel</c> Models IDs for a company in edgar: (CIK ID, ticker, company name)   
   /// </summary>
    public class CompanyModel
    {
        
        private UInt64 cik;

        
        private string ticker;

        
        private string company_name;

        /// <summary>
        /// A TickersModel is a record representing a unique company in the SEC edgar database
        /// </summary>
        /// <param name="cik"> This is the CIK ID unique ID given to corps in the edgar database
        /// The cik is a 10 digit identifier, we store this in the program as an unsigned 64 bit integer to represent 10 digits
        /// We could do better by maybe using a bitmap, but I will do that if I run into issues :)
        /// </param>
        /// <param name="ticker"> This is the ticker for a company on a stock exchange</param>
        /// <param name="company_name"> This is the full name of a company</param>
        public CompanyModel(UInt64 cik, string ticker, string company_name)
        {
            if (string.IsNullOrEmpty(ticker)) throw new ArgumentNullException(nameof(ticker));
            if (string.IsNullOrEmpty(company_name)) throw new ArgumentNullException(nameof(company_name));

            this.cik = cik;
            this.ticker = ticker;
            this.company_name = company_name;
        }
        
        /// <summary>
        /// CIK index is the unique identifier for a company in the edgar database
        /// </summary>
        public UInt64 CIK { get => cik; set => cik = value; }

        /// <summary>
        /// Ticker is the symbol of the company on its exchange
        /// </summary>
        public string Ticker { get => ticker; set => ticker = value; }

        /// <summary>
        /// The full name of the corporation
        /// </summary>
        public string Company_Name { get => company_name; set => company_name = value; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inn2PowerDataStreamUpdater.Misc;

namespace Inn2PowerDataStreamUpdater.BLL
{
    public class Logic
    {
        private ResultObject _badResult;
        private ResultObject _succesResult;
        
        public Logic()
        {
            this._badResult = new ResultObject();
            this._succesResult = new ResultObject();          
        }

        /// <summary>
        /// Goes trough companies from stream and compares them with API companies.
        /// Splits stream companies into existing companies and new companies.
        /// </summary>
        /// <param name="dataStreamCompanies">Companies from stream.</param>
        /// <param name="apiCompanies">Companies from out API</param>
        /// <returns></returns>
        public ResultObject PrepareCompanies(
            List<DataStreamCompany> dataStreamCompanies, 
            List<APICompany> apiCompanies)
        {          
            if (!dataStreamCompanies.Any() || ! apiCompanies.Any())
            {
                this._badResult.IsSuccesFull = false;
                this._badResult.ErrorMessage = "One of the lists are empty. Please restart applikation.";
                return this._badResult;
            }

            //Converts stream data to API Company format
            var convertedStreamCompanies = ConvertStreamCompanies(dataStreamCompanies);

            //Preperes to sort throug the stream data.
            var existingCompanies = new List<APICompany>();
            var newCompanies = new List<APICompany>();
            foreach (var convertedStreamCompany in convertedStreamCompanies)
            {
                var streamKey = (convertedStreamCompany.CompanyName + convertedStreamCompany.Country).Trim().ToLower();
                var added = false;

                foreach (var apiCompany in apiCompanies)
                {
                    var apiKey = (apiCompany.CompanyName + apiCompany.Country).Trim().ToLower();

                    if (streamKey.Equals(apiKey))
                    {
                        //Adds if exists in DB.
                        convertedStreamCompany.Id = apiCompany.Id;
                        existingCompanies.Add(convertedStreamCompany);
                        added = true;
                    }
                }
                if (added == false)
                {
                    //Adds if not exists in DB.
                    newCompanies.Add(convertedStreamCompany);
                }               
            }
            //creating the return object.
            var subResult = new ListsSubResult();
            subResult.ExistingCompanies = existingCompanies;
            subResult.NewCompanies = newCompanies;

            this._succesResult.IsSuccesFull = true;
            this._succesResult.Payload = subResult;
            return this._succesResult;
        }

        /// <summary>
        /// Converts stream companies to API company format.
        /// </summary>
        /// <param name="streamCompanies">Commapny from stream</param>
        /// <returns>Formatet companies</returns>
        private List<APICompany> ConvertStreamCompanies(List<DataStreamCompany> streamCompanies)
        {
            var convertedCompanies = new List<APICompany>();

            foreach (var item in streamCompanies)
            {
                var company = new APICompany();
                company.CompanyName = item.company_name;
                company.Country = item.country;
                company.Website = item.website;                
                company.SME = item.sme_status;               
                if (!item.offices.Any())
                {
                    company.Address = "";
                    company.Latitude = 0;
                    company.Longitude = 0;
                    company.Created = DateTime.Now;
                }
                else
                {
                    var office = item.offices.ElementAt(0);
                    company.Address = office.address;
                    company.Latitude = Decimal.Parse(office.lat);
                    company.Longitude = Decimal.Parse(office.lng);
                    company.Created = DateTime.Now;                   
                }                
                convertedCompanies.Add(company);
            }

            return convertedCompanies;
        }
    }
}

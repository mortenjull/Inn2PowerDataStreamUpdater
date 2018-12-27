using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Inn2PowerDataStreamUpdater.Misc;
using Inn2PowerDataStreamUpdater.Misc.Entities;

namespace Inn2PowerDataStreamUpdater.BLL
{
    public class Logic
    {
        private ResultObject _badResult;
        private ResultObject _succesResult;

        private List<string> ErrorCompanies;
        
        public Logic()
        {
            //List containing companies with bad data
            this.ErrorCompanies = new List<string>();

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
            List<APICompany> apiCompanies,
            List<SupplyChainRole> SupplyChainRoles,
            List<SupplyChainCategory> SuppleChainCategories)
        {          
            if (!dataStreamCompanies.Any() || !apiCompanies.Any() || !SupplyChainRoles.Any() || !SuppleChainCategories.Any())
            {
                this._badResult.IsSuccesFull = false;
                this._badResult.ErrorMessage = "One of the lists are empty. Please restart applikation.";
                return this._badResult;
            }

            //Converts stream data to API Company format
            var convertedStreamCompanies = ConvertStreamCompanies(dataStreamCompanies, SupplyChainRoles, SuppleChainCategories);

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
                        break;
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
        private List<APICompany> ConvertStreamCompanies(
            List<DataStreamCompany> streamCompanies, 
            List<SupplyChainRole> SupplyChainroles,
            List<SupplyChainCategory> SuppleChainCategories
            )
        {
            var convertedCompanies = new List<APICompany>();
            
                foreach (var item in streamCompanies)
                {
                    //Skips the invalid item.
                    if(!ValidateDataStreamCompany(item))
                        continue;

                    var company = new APICompany();

                    company.CompanyName = item.company_name;
                    company.Country = item.country;
                    company.Website = item.website;
                    company.SME = item.sme_status;

                    var roleresult = ConvertSupplyChainRoles(item.supply_chain_roles, SupplyChainroles, item);
                    var categoryresult = ConvertSupplyChainCategories(item.supply_chain_categories, SuppleChainCategories);
                    company.SupplyChainCategories = categoryresult;
                    company.SupplyChainRoles = roleresult;

                    var office = item.offices.ElementAt(0);
                        company.Address = office.address;
                        company.Latitude = Decimal.Parse(office.lat);
                        company.Longitude = Decimal.Parse(office.lng);
                        company.Created = DateTime.Now;

                    convertedCompanies.Add(company);
                }
            return convertedCompanies;
        }

        /// <summary>
        /// DataStreamCompany is added for error logging.
        /// </summary>
        /// <param name="StreamChainStrings"></param>
        /// <param name="SupplyChainRoles"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private List<SupplyChainRole> ConvertSupplyChainRoles(
            List<string>StreamChainStrings, 
            List<SupplyChainRole>SupplyChainRoles,
            DataStreamCompany item)
        {
            if(StreamChainStrings == null || !StreamChainStrings.Any())
                return new List<SupplyChainRole>();

            var convertedRoles = new List<SupplyChainRole>();

            foreach (var stringcode in StreamChainStrings)
            {
                int code;
                bool success = Int32.TryParse(stringcode, out code);
                if (success)
                {
                    foreach (var role in SupplyChainRoles)
                    {
                        if (role.SupplyChainRoleCode == code)
                        {
                            convertedRoles.Add(role);
                            break;
                        }                         
                    }
                }
                else
                {
                    this.ErrorCompanies.Add($"Entry number: {item.entry_reference_number}. Error SupplyChainRole. N/A object:");
                    //Role code string from streame api contains n/a.
                    //This should be cout in The Validation method. But we will keep this just for good measueres.
                    //We should never hit this.
                }
            }
            return convertedRoles;
        }

        private List<SupplyChainCategory> ConvertSupplyChainCategories(
            List<string> StreamCategoryStrings, 
            List<SupplyChainCategory> SupplyChainCategories)
        {
            if (StreamCategoryStrings == null || !StreamCategoryStrings.Any())
                return new List<SupplyChainCategory>();
            
            var convertedCategories = new List<SupplyChainCategory>();

            foreach (var stringcode in StreamCategoryStrings)
            {
                foreach (var category in SupplyChainCategories)
                {
                    if (category.SupplyChainCategoryCode.ToString().Equals(stringcode))
                    {
                        convertedCategories.Add(category);
                        break;
                    }
                }
            }
            return convertedCategories;
        }

        /// <summary>
        /// Because of invalid data from datastream, we use this method to validate each element from the stream.
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        private bool ValidateDataStreamCompany(DataStreamCompany company)
        {
            //Validates Offices lat, lng and Address.
            if (company.offices == null)
                return false;
            if (!company.offices.Any())
                return false;
            else
            {
                var office = company.offices.ElementAt(0);
                if (String.IsNullOrEmpty(office.lat) || String.IsNullOrWhiteSpace(office.lat)
                                                     || String.IsNullOrEmpty(office.lng) || String.IsNullOrWhiteSpace(office.lng))
                    return false;
                if (String.IsNullOrEmpty(office.address) || String.IsNullOrWhiteSpace(office.address))
                    return false;
            }

            //Validates Categories and Roles.
            if (company.supply_chain_categories == null)
                return false;
            if (!company.supply_chain_categories.Any())
                return false;
            else
            {
                if (company.supply_chain_categories.Contains("n/a"))
                    return false;
            }
            if (company.supply_chain_roles == null)
                return false;
            if (!company.supply_chain_roles.Any())
                return false;
            else
            {
                if (company.supply_chain_roles.Contains("n/a"))
                    return false;
            }

            //Returns true if data is valid within the testet areas.
            return true;
        }
       
    }
}

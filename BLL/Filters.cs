﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Inn2PowerDataStreamUpdater.Misc;
using Inn2PowerDataStreamUpdater.Misc.Entities;
using NPOI.HSSF.UserModel;
using NPOI.OpenXmlFormats.Dml.WordProcessing;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Inn2PowerDataStreamUpdater.BLL
{
    public class Filters
    {
        private ResultObject _badResult;
        private ResultObject _succesResult;
        private List<APICompany> _potentialDuplicates;
        private List<PotentialDiplicate> _datastreamPotentialDuplicates;
        public Filters()
        {
            this._badResult = new ResultObject();
            this._succesResult = new ResultObject();
            this._potentialDuplicates = new List<APICompany>();
            this._datastreamPotentialDuplicates = new List<PotentialDiplicate>();
        }


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

            //Filter på tværs af de to lister ved at sammenligne navne. 
            //Dette sortere eventuelle duplicates fra inden vi formatere datastream til api company.
            var filteredDatastreamCompanies = FindPotentialDuplicates(dataStreamCompanies, apiCompanies);
            
            //formater hvert enkelt datastream til api company.            
            var convertedStreamCompanies =
                ConvertToApiCompany(filteredDatastreamCompanies, SupplyChainRoles, SuppleChainCategories);
            
            //sortere de converterede stream companies ud på nye og eksisterende companies.
            var subResult = SortCompaniesByNewAndExisting(convertedStreamCompanies, apiCompanies);

            this._succesResult.IsSuccesFull = true;
            this._succesResult.Payload = subResult;

            PrintDuplicates();

            return this._succesResult;
        }

        /// <summary>
        /// First reduceses the datastream data by comparing it with itself and removing duplicates.
        /// Secund compares with apidata and again removes duplicates if stream data appears in api data.
        /// Special: If api data contains Reff key we know for sertain it originates from datastream
        /// wee want too keep those for update perpause. We dont consider the api company for matching if it contains reff key.
        /// </summary>
        /// <param name="dataStreamCompanies"></param>
        /// <param name="apiCompanies"></param>
        /// <returns></returns>
        private List<DataStreamCompany> FindPotentialDuplicates(
            List<DataStreamCompany> dataStreamCompanies, 
            List<APICompany> apiCompanies)
        {
            var reducedDatastreamCompanies = new List<DataStreamCompany>();
            var filteredDataStreamCompanies = new List<DataStreamCompany>();
          
            foreach (var item in dataStreamCompanies)
            {
                var match = false;
                var matchItem = new DataStreamCompany();
                //Check the item status. Skips the item if its not one of theses two.
                if (!item.status.Equals("Approved") && !item.status.Equals("Partner Added"))
                {
                    continue;
                }
                //Filteres potential duplicated from datastreamcompanies via comparing it to itself.
                foreach (var x in dataStreamCompanies)
                {
                    if(item.entry_reference_number.Equals(x.entry_reference_number))
                        continue;
                    //if countries match wee will go to next step
                    if (MatchCountries(x.country).Equals(MatchCountries(item.country)))
                    {
                        //If name is IN ANY WAY contined within another company we consider it a potential duplicate.
                        //If true, Item is added to reduced and potentialduplicates.
                        var xname = x.company_name.Trim().ToLowerInvariant();
                        var itemname = item.company_name.Trim().ToLowerInvariant();
                        if (xname.Contains(itemname))
                        {
                            match = true;
                            matchItem = x;
                            break;
                        }
                        else
                        {
                            match = false;                           
                        }
                    }
                    else
                    {
                        match = false;                      
                    }
                }                            
                if (match == false)
                    reducedDatastreamCompanies.Add(item);
                else
                    this._datastreamPotentialDuplicates.Add(new PotentialDiplicate(item, matchItem, null));
            }          
            //filteres portential duplicates away by comparing the reduced list with companies from the API.
            //If y has Reff key. It origins from datasteam and we add it to filtered it because its difinitive the same and we might want to update it.
            //If y has no reff key and x does not appear in apiCompanies we add it to reduced
            reducedDatastreamCompanies.ForEach(x =>
            {
                var match = false;
                var matchItem = new APICompany();
                foreach (var y in apiCompanies)
                {
                    if (y.CompanyDirectoryEntryReffNumber == null || y.CompanyDirectoryEntryReffNumber.Equals(""))
                    {
                        //if countries match wee will go to next step
                        if (MatchCountries(x.country).Equals(MatchCountries(y.Country)))
                        {
                            //If name is IN ANY WAY contined within another company we consider it a potential duplicate.
                            //If true, Item is added to reduced and potentialduplicates.
                            var xname = x.company_name.Trim().ToLowerInvariant();
                            var yname = y.CompanyName.Trim().ToLowerInvariant();
                            if (xname.Contains(yname))
                            {
                                match = true;
                                matchItem = y;
                                break;
                            }
                            else
                            {
                                match = false;
                            }
                        }
                        else
                        {
                            match = false;
                        }
                    }
                    else if (x.entry_reference_number.Equals(y.CompanyDirectoryEntryReffNumber))
                    {
                        match = false;
                    }
                }               
                if (match == false)
                    filteredDataStreamCompanies.Add(x);
                else
                    this._datastreamPotentialDuplicates.Add(new PotentialDiplicate(x, null, matchItem));
            });        
            return filteredDataStreamCompanies;
        }

        private void PrintDuplicates()
        {
            if (!this._datastreamPotentialDuplicates.Any())
            {
                return;
            }

            var date = "";
            date = DateTime.Now.ToString();
            var x = date.Replace("/", "-");
            var y = x.Replace(":", "-");
            string fileName = "duplicatesReport(" + y + ").xlsx";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);                     
            using (var fs = new FileStream(Path.Combine(path, fileName), FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook = new XSSFWorkbook();

                ISheet excelSheet = workbook.CreateSheet("duplicatesReport");
                IRow row = excelSheet.CreateRow(0);

                row.CreateCell(0).SetCellValue("PotentialDuplicate: Name");
                row.CreateCell(1).SetCellValue("PotentialDuplicate: Country");
                row.CreateCell(2).SetCellValue("PotentialDuplicate: ReffKey");
                row.CreateCell(3).SetCellValue("DataStream Match: Name");
                row.CreateCell(4).SetCellValue("DataStream Match: Country");
                row.CreateCell(5).SetCellValue("DataStream Match: ReffKey");
                row.CreateCell(6).SetCellValue("ApiData Match: Name");
                row.CreateCell(7).SetCellValue("ApiData Match: Country");
                row.CreateCell(8).SetCellValue("ApiData Match: ID");

                for (int i = 0; i < this._datastreamPotentialDuplicates.Count; i++)
                {
                    var item = this._datastreamPotentialDuplicates[i];
                    if (item.Match2 == null)
                    {
                        row = excelSheet.CreateRow(i + 1);
                        row.CreateCell(0).SetCellValue(item.Duplicate.company_name);
                        row.CreateCell(1).SetCellValue(MatchCountries(item.Duplicate.country));
                        row.CreateCell(2).SetCellValue(item.Duplicate.entry_reference_number);
                        row.CreateCell(3).SetCellValue(item.Match.company_name);
                        row.CreateCell(4).SetCellValue(MatchCountries(item.Match.country));
                        row.CreateCell(5).SetCellValue(item.Match.entry_reference_number);
                        row.CreateCell(6).SetCellValue("Null");
                        row.CreateCell(7).SetCellValue("Null");
                        row.CreateCell(8).SetCellValue("Null");

                    }
                    else
                    {
                        row = excelSheet.CreateRow(i + 1);
                        row.CreateCell(0).SetCellValue(item.Duplicate.company_name);
                        row.CreateCell(1).SetCellValue(MatchCountries(item.Duplicate.country));
                        row.CreateCell(2).SetCellValue(item.Duplicate.entry_reference_number);
                        row.CreateCell(3).SetCellValue("Null");
                        row.CreateCell(4).SetCellValue("Null");
                        row.CreateCell(5).SetCellValue("Null");
                        row.CreateCell(6).SetCellValue(item.Match2.CompanyName);
                        row.CreateCell(7).SetCellValue(item.Match2.Country);
                        row.CreateCell(8).SetCellValue(item.Match2.Id);
                    }
                }               
                workbook.Write(fs);
            }
        }

        private List<APICompany> ConvertToApiCompany(
            List<DataStreamCompany> dataStreamCompanies,
            List<SupplyChainRole> SupplyChainRoles,
            List<SupplyChainCategory> SuppleChainCategories)
        {
            var ApiCompanies = new List<APICompany>();

            dataStreamCompanies.ForEach(item =>
            {
                var company = new APICompany();
                //Special for Country.
                {
                    if (item.country.Equals("Netherlands"))
                        item.country = "The Netherlands";
                    if (item.country.Equals("UK"))
                        item.country = "United Kingdom";
                }
                company.CompanyName = item.company_name;
                company.Country = item.country;
                company.Website = item.website;
                company.SME = item.sme_status;
                company.CompanyDirectoryEntryReffNumber = item.entry_reference_number;
                company.Status = item.status;
                company.SupplyChainCategories = ConvertSupplyChainCategories(item.supply_chain_categories, SuppleChainCategories);
                company.SupplyChainRoles = ConvertSupplyChainRoles(item.supply_chain_roles, SupplyChainRoles);

                if (item.offices.Any())
                {                  
                    var office = item.offices.ElementAt(0);

                    company.Address = office.address ?? "";
                    company.Latitude = Decimal.Parse(office.lat ?? "0");
                    company.Longitude = Decimal.Parse(office.lng ?? "0");
                    company.Created = DateTime.Now;
                }
                else
                {
                    company.Address = "";
                    company.Latitude = 0;
                    company.Longitude = 0;
                    company.Created = DateTime.Now;                   
                }

                ApiCompanies.Add(company);
            });

            return ApiCompanies;
        }

        private ListsSubResult SortCompaniesByNewAndExisting(List<APICompany> listToSort, List<APICompany> listToCompareWith)
        {
            var newCompanies = new List<APICompany>();
            var existingCompanies = new List<APICompany>();
            listToSort.ForEach(x =>
            {
                var match = false;
                foreach (var y in listToCompareWith)
                {
                    if(String.IsNullOrWhiteSpace(y.CompanyDirectoryEntryReffNumber))
                        continue;
                    if (x.CompanyDirectoryEntryReffNumber.Equals(y.CompanyDirectoryEntryReffNumber))
                    {
                        existingCompanies.Add(x);
                        match = true;;
                        break;
                    }                       
                }
                if(match == false)
                    newCompanies.Add(x);
            });
            var subResult = new ListsSubResult();
            subResult.ExistingCompanies = existingCompanies;
            subResult.NewCompanies = newCompanies;
            return subResult;
        }

        /// <summary>
        /// Makes shure that the naming conventien for country is opheld when matching them.
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        private string MatchCountries(string country)
        {
            if (country.Equals("Netherlands"))
                return "The Netherlands";
            if (country.Equals("UK"))
                return "United Kingdom";
            return country;
        }
        
        private List<SupplyChainRole> ConvertSupplyChainRoles(
            List<string> StreamChainStrings,
            List<SupplyChainRole> SupplyChainRoles)
        {
            if (StreamChainStrings == null || !StreamChainStrings.Any())
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
                    //Role code string from streame api contains n/a.
                    //If we want to handle anything regarding that.
                    //Place the code here.
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
    }
}

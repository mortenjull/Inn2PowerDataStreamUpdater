using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inn2PowerDataStreamUpdater.BLL;
using Inn2PowerDataStreamUpdater.Misc;
using Inn2PowerDataStreamUpdater.Misc.Entities;
using Inn2PowerDataStreamUpdater.Services;

namespace Inn2PowerDataStreamUpdater.Menues
{
    public class Menu1
    {       
        private readonly ApiService _apiService;
        private readonly DataStreamService _dataStreamService;
        private readonly Logic _logic;
        private readonly Filters _filters;

        private readonly string _apiConnectionString;
        private readonly string _datastreamUrl;
        private string _token;

        private List<APICompany> _newCompaniesFromStream;
        private List<APICompany> _companiesFromStreamAlreadyInDB;
              
        public Menu1(string apiconnectionstring, string token, string datastreamURL)
        {            
            this._apiConnectionString = apiconnectionstring;
            this._datastreamUrl = datastreamURL;
            this._token = token;           
           
            this._apiService = new ApiService(this._apiConnectionString);
            this._dataStreamService = new DataStreamService(this._datastreamUrl);
            this._logic = new Logic();
            this._filters = new Filters();

            this._newCompaniesFromStream = new List<APICompany>();
            this._companiesFromStreamAlreadyInDB = new List<APICompany>();


        }

        public void RunMenu1()
        {
            Console.Clear();
            Console.WriteLine("Preparing Data....");
            PrepData();

            Console.Clear();
            Console.WriteLine($"There are New Companies From Stream: {this._newCompaniesFromStream.Count}");
            Console.WriteLine($"There are, already existing in DB Companies From Stream: {this._companiesFromStreamAlreadyInDB.Count}");
            Console.WriteLine("");
            
            var done = false;
            while (!done)
            {              
                Menu1Dialog();
                var input = Console.ReadLine().ToLower().Trim();
                switch (input)
                {
                    case "1":
                        UpdateAndCreateCompanies();
                        break;
                    case "2":                        
                        CreateCompanies(this._newCompaniesFromStream);
                        break;
                    case "3":
                        UpdateCompanies(this._companiesFromStreamAlreadyInDB);
                        break;
                    case "4":
                        var selectmenuCreate = new SelectMenu(this._newCompaniesFromStream);
                        var result = selectmenuCreate.RunMenu();
                        this._newCompaniesFromStream = result.Companies;
                        CreateCompanies((List<APICompany>) result.SelectedCompanies);
                        break;
                    case "5":
                        var selectmenuUpdate = new SelectMenu(this._companiesFromStreamAlreadyInDB);
                        var result2 = selectmenuUpdate.RunMenu();
                        this._companiesFromStreamAlreadyInDB = result2.Companies;
                        UpdateCompanies((List<APICompany>) result2.SelectedCompanies);
                        break;
                    case "e":
                        done = true;
                        break;
                    default:
                        break;
                }                
            } 
            
            Console.Clear();
            Console.WriteLine("Press Enter to exit Applikation.");
            Console.ReadLine();
        }

        /// <summary>
        /// Uses Prep method placed in logic to prepare data from stream.
        /// </summary>
        private void PrepData()
        {
            var dataStreamResult = this._dataStreamService.GetDataFromStream().Result;
            var apiCompanyResult = this._apiService.GetApiCompanies(this._token).Result;
            var categoriesResult = this._apiService.GetSupplyChainCategorys(this._token).Result;
            var roleResult = this._apiService.GetSupplyChainRoles(this._token).Result;

            if (dataStreamResult.IsSuccesFull && apiCompanyResult.IsSuccesFull && categoriesResult.IsSuccesFull &&
                roleResult.IsSuccesFull)
            {
                var prepResult = this._filters.PrepareCompanies(
                                                            (List<DataStreamCompany>) dataStreamResult.Payload,
                                                            (List<APICompany>) apiCompanyResult.Payload,
                                                            (List<SupplyChainRole>) roleResult.Payload,
                                                            (List<SupplyChainCategory>) categoriesResult.Payload);

                if (prepResult.IsSuccesFull)
                {
                    var result = (ListsSubResult)prepResult.Payload;
                    this._newCompaniesFromStream = result.NewCompanies;
                    this._companiesFromStreamAlreadyInDB = result.ExistingCompanies;
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Something went wrong preparing data from Stream and API.");
                    Console.WriteLine("Please restart Application.");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Something went wrong getting data from Api and Stream.");
                Console.WriteLine("Please restart Application.");
                Console.ReadLine();
            }

        }
        

        /// <summary>
        /// Combines the update and create method herrein.
        /// </summary>
        /// <returns>ResultObject</returns>
        private bool UpdateAndCreateCompanies()
        {
            var badResult = new ResultObject();
            var succesresult = new ResultObject();
             
            if (!UpdateCompanies(this._companiesFromStreamAlreadyInDB))
            {
                return false;
            }           
            if (!CreateCompanies(this._newCompaniesFromStream))
            {
                return false;
            }            
            return true;
        }

        /// <summary>
        /// Tries to update companies into DB via ApiService.
        /// Handles relogin is token is expired allows maks of 2 tries.
        /// </summary>
        /// <param name="existingCompanies">Companies to update</param>
        /// <returns>ResultObject</returns>
        private bool UpdateCompanies(List<APICompany> existingCompanies)
        {            
            if (!existingCompanies.Any())
            {
                Console.WriteLine("Companies to Update is EMPTY. Press enter:");
                Console.ReadLine();
                return false;
            }            

            var result = this._apiService.UpdateCompanies(this._token, existingCompanies).Result;
            if (result.IsSuccesFull == true)
            {
                Console.WriteLine("Companies have been Updated. Press Enter:");
                Console.ReadLine();
                return true;
            }
            else
            {
                Console.WriteLine("Companies COULD NOT be Updated. Press Enter:");
                Console.ReadLine();
                return false;
            }
        }            
        

        /// <summary>
        /// Tries to create companies into DB via ApiService.
        /// Handles relogin is token is expired allows maks of 2 tries.
        /// </summary>
        /// <param name="newCompanies">New companies</param>
        /// <returns>ResultObject</returns>
        private bool CreateCompanies(List<APICompany> newCompanies)
        {           
            if (!newCompanies.Any())
            {
                Console.WriteLine("Companies to Create is EMPTY. Press enter:");
                Console.ReadLine();
                return false;
            }
            
            var result = this._apiService.CreateCompanies(this._token, newCompanies).Result;
            if (result.IsSuccesFull == true)
            {
                Console.WriteLine("Companies have been Created. Press Enter:");
                Console.ReadLine();
                return true;
            }
            else
            {
                Console.WriteLine("Companies COULD NOT be Created. Press Enter:");
                Console.ReadLine();
                return false;
            }
        }           
       
       

        private void Menu1Dialog()
        {
            Console.Clear();
            Console.WriteLine($"New Companies: {this._newCompaniesFromStream.Count}");
            Console.WriteLine($"ExistingCompanies: {this._companiesFromStreamAlreadyInDB.Count}");
            Console.WriteLine("");
            Console.WriteLine("Select an option by typen its NUMBER.");
            Console.WriteLine("");
            Console.WriteLine("( 1 ): Update Database with NEW and EXISTING companies.");
            Console.WriteLine("");
            Console.WriteLine("( 2 ): Create NEW companies in Database.");
            Console.WriteLine("");
            Console.WriteLine("( 3 ): Update Database with EXISTING companies.");
            Console.WriteLine("");
            Console.WriteLine("( 4 ): Manuel select CREATE companies.");
            Console.WriteLine("");
            Console.WriteLine("( 5 ): Manuel select UPDATE companies.");
            Console.WriteLine("");
            Console.WriteLine("( E ): Type E to Exit. Wont execute ANY options to DB.");
            Console.WriteLine("");            
        }
        
    }
}

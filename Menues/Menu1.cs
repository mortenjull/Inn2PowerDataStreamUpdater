using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inn2PowerDataStreamUpdater.Misc;
using Inn2PowerDataStreamUpdater.Services;

namespace Inn2PowerDataStreamUpdater.Menues
{
    public class Menu1
    {
        private readonly AuthService _authService;
        private readonly ApiService _apiService;
        private readonly string _apiConnectionString;
        private string _token;
        private readonly string _username;
        private readonly string _password;

        private List<APICompany> _companiesToUpdate;
        private List<APICompany> _newCompanies;
        private List<APICompany> _existingCompanies;

        public Menu1(
            List<APICompany> newCompanies, 
            List<APICompany> existingCompanies,
            string apiconnectionstring,
            string token,
            string username,
            string password)
        {
            this._companiesToUpdate = new List<APICompany>();
            this._newCompanies = new List<APICompany>();
            this._existingCompanies = new List<APICompany>();
            
            this._newCompanies = newCompanies;
            this._existingCompanies = existingCompanies;
            this._apiConnectionString = apiconnectionstring;
            this._token = token;
            this._username = username;
            this._password = password;

            this._authService = new AuthService(this._apiConnectionString);
            this._apiService = new ApiService(this._apiConnectionString);
        }

        public ResultObject RunMenu1()
        {
            Console.Clear();
            Console.WriteLine($"There are NewCompanies: {this._newCompanies.Count}");
            Console.WriteLine($"There are ExistingCompanies: {this._existingCompanies.Count}");
            Console.WriteLine("");

            var resultobject = new ResultObject();
            var selectResult = new SelectMenuResultObject();
            var done = false;
            while (!done)
            {              
                Menu1Dialog();
                var input = Console.ReadLine().ToLower().Trim();
                switch (input)
                {
                    case "1":
                        resultobject = UpdateAndCreateCompanies();
                        break;
                    case "2":
                        
                        resultobject = CreateCompanies(this._newCompanies);
                        break;
                    case "3":
                        resultobject = UpdateCompanies(this._existingCompanies);
                        break;
                    case "4":
                        var selectmenuCreate = new SelectMenu(this._newCompanies);
                        var result = selectmenuCreate.RunMenu();
                        this._newCompanies = result.Companies;
                        resultobject = CreateCompanies((List<APICompany>) result.SelectedCompanies);
                        break;
                    case "5":
                        var selectmenuUpdate = new SelectMenu(this._existingCompanies);
                        var result2 = selectmenuUpdate.RunMenu();
                        this._existingCompanies = result2.Companies;
                        resultobject = UpdateCompanies((List<APICompany>) result2.SelectedCompanies);
                        break;
                    case "e":
                        done = true;
                        break;
                    default:
                        break;
                }
                
                if (!done)
                {
                    if (resultobject.IsSuccesFull)
                    {
                        Console.WriteLine("The chosen operation was a succes.");
                        Console.WriteLine("Press ENTER to Continiue");
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine(resultobject.ErrorMessage);
                        Console.WriteLine("Please restart applikation.");
                        Console.WriteLine("Press ENTER to Continiue");
                        Console.ReadLine();
                    }
                }               
                
            }

            resultobject.Payload = this._token;
            return resultobject;
        }
        

        /// <summary>
        /// Combines the update and create method herrein.
        /// </summary>
        /// <returns>ResultObject</returns>
        private ResultObject UpdateAndCreateCompanies()
        {
            var badResult = new ResultObject();
            var succesresult = new ResultObject();

            var result = UpdateCompanies(this._existingCompanies);
            if (result.IsSuccesFull == false)
            {
                badResult.IsSuccesFull = false;
                badResult.ErrorMessage = "Could not Update companies.";                
                return badResult;
            }

            var result2 = CreateCompanies(this._newCompanies);
            if (result2.IsSuccesFull == false)
            {
                badResult.IsSuccesFull = false;
                badResult.ErrorMessage = "Could not Create companies.";
                return badResult;
            }

            succesresult.IsSuccesFull = true;
            succesresult.Payload = this._token;
            return succesresult;
        }

        /// <summary>
        /// Tries to update companies into DB via ApiService.
        /// Handles relogin is token is expired allows maks of 2 tries.
        /// </summary>
        /// <param name="existingCompanies">Companies to update</param>
        /// <returns>ResultObject</returns>
        private ResultObject UpdateCompanies(List<APICompany> existingCompanies)
        {
            var badResult = new ResultObject();
            var succesresult = new ResultObject();
            if (!existingCompanies.Any())
            {
                succesresult.IsSuccesFull = true;
                return succesresult;
            }
            var done = false;
            var tries = 0;
            while (!done)
            {
                if (tries == 1)
                {
                    done = true;
                   
                    badResult.IsSuccesFull = false;
                    badResult.ErrorMessage = "Tries reached. Could not login.";
                    return badResult;
                }

                var result = this._apiService.UpdateCompanies(this._token, existingCompanies).Result;
                if (result.IsSuccesFull == true)
                {
                    done = true;
                    
                    succesresult.IsSuccesFull = true;
                    succesresult.Payload = result.Payload;
                    return succesresult;
                }
                else
                {
                    if (!ReLogin())
                    {
                        done = true;

                        
                        badResult.IsSuccesFull = false;
                        badResult.ErrorMessage = "Could not login";
                        return badResult;
                    }

                    tries++;
                }
            }
            //placed to saticfy return at all path condition eventhough unreachable

            badResult.IsSuccesFull = false;
            badResult.ErrorMessage = "Unreachable is reached. This shuold not be posible";
            return badResult;
        }

        /// <summary>
        /// Tries to create companies into DB via ApiService.
        /// Handles relogin is token is expired allows maks of 2 tries.
        /// </summary>
        /// <param name="newCompanies">New companies</param>
        /// <returns>ResultObject</returns>
        private ResultObject CreateCompanies(List<APICompany> newCompanies)
        {
            var badResult = new ResultObject();
            var succesresult = new ResultObject();
            if (!newCompanies.Any())
            {
                succesresult.IsSuccesFull = true;
                return succesresult;
            }
            
            var done = false;
            var tries = 0;
            while (!done)
            {
                if (tries == 1)
                {
                    done = true;
                
                    badResult.IsSuccesFull = false;
                    badResult.ErrorMessage = "Tries reached. Could not login.";
                    return badResult;
                }

                var result = this._apiService.CreateCompanies(this._token, newCompanies).Result;
                if (result.IsSuccesFull == true)
                {
                    done = true;

                    succesresult.IsSuccesFull = true;
                    succesresult.Payload = this._token;
                    return succesresult;
                }
                else
                {
                    if (!ReLogin())
                    {
                        done = true;

                        badResult.IsSuccesFull = false;
                        badResult.ErrorMessage = "Could not login";
                        return badResult;
                    }

                    tries++;
                }
            }
            //placed to saticfy return at all path condition eventhough unreachable

            badResult.IsSuccesFull = false;
            badResult.ErrorMessage = "Unreachable is reached. This shuold not be posible";
            return badResult;
        }

        /// <summary>
        /// Tries to ReeLogin in case of expired token.
        /// </summary>
        /// <returns>true or false</returns>
        private bool ReLogin()
        {
            var result = this._authService.Login(this._username, this._password).Result;
            if (result.IsSuccesFull)
            {
                this._token = result.Payload.ToString();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Menu1Dialog()
        {
            Console.Clear();
            Console.WriteLine($"New Companies: {this._newCompanies.Count}");
            Console.WriteLine($"ExistingCompanies: {this._existingCompanies.Count}");
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

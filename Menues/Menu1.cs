using System;
using System.Collections.Generic;
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

        public void RunMenu1()
        {
            Console.Clear();
            Console.WriteLine($"There are NewCompanies: {this._newCompanies.Count}");
            Console.WriteLine($"There are ExistingCompanies: {this._existingCompanies.Count}");
            Console.WriteLine("");

            while (true)
            {              
                Menu1Dialog();
                var input = Console.ReadLine().ToLower().Trim();
                switch (input)
                {
                    case "1":
                        UpdateAndCreateCompanies();
                        break;
                    case "2":
                        break;
                    case "3":
                        break;
                    case "4":
                        break;
                    case "5":
                        break;
                    case "e":
                        break;
                    default:
                        break;
                }
            }
        }

        private ResultObject UpdateAndCreateCompanies()
        {
            return null;
        }

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
            Console.WriteLine("Select an option by typen its NUMBER.");
            Console.WriteLine("");
            Console.WriteLine("( 1 ): Update Database with NEW and EXISTING companies.");
            Console.WriteLine("");
            Console.WriteLine("( 2 ): Update Database with NEW companies.");
            Console.WriteLine("");
            Console.WriteLine("( 3 ): Update Database with EXISTING companies.");
            Console.WriteLine("");
            Console.WriteLine("( 4 ): Manuel select New companies.");
            Console.WriteLine("");
            Console.WriteLine("( 5 ): Manuel select Existing companies.");
            Console.WriteLine("");
            Console.WriteLine("( E ): Type E to Exit. Wont execute ANY options to DB.");
            Console.WriteLine("");            
        }
        
    }
}

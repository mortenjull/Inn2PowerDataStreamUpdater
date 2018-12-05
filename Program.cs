using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Inn2PowerDataStreamUpdater.BLL;
using Inn2PowerDataStreamUpdater.Menues;
using Inn2PowerDataStreamUpdater.Misc;
using Inn2PowerDataStreamUpdater.Services;

namespace Inn2PowerDataStreamUpdater
{
    class Program
    {
        //Services -----
        private static AuthService _autherService;
        private static DataStreamService _dataStreamService;
        private static ApiService _apiService;

        //BLL-----------
        private static Logic _logic;

        //Global Varibles -------
        private static string _username;
        private static string _password;
        private static string _bearerToken;

        private static List<DataStreamCompany> _dataStramCompanies;
        private static List<APICompany> _apiCompanies;

        private static List<APICompany> _newCompanies;
        private static List<APICompany> _existingCompanies;
        private static List<APICompany> _companiesToUpdate;

        private const string DATASTREAM_URL = "http://inn2power.eu/mapping/api/feed?key=0lUwFrGpTqfI9oSNISCUF7m5UYzWLtCU";
        private const string API_CONNECTIONSTRING = "https://localhost:44346";

        //While loop toogles----
        private static bool _loggedInd = false;
        private static bool _gotDataFromDataStream = false;
        private static bool _gotDataFromAPI = false;

        //Menues-----------
        private static Menu1 _menu1;
        
        static void Main(string[] args)
        {
            _autherService = new AuthService(API_CONNECTIONSTRING);
            _dataStreamService = new DataStreamService(DATASTREAM_URL);
            _apiService = new ApiService(API_CONNECTIONSTRING);
            _logic = new Logic();

            _dataStramCompanies = new List<DataStreamCompany>();
            _newCompanies = new List<APICompany>();
            _existingCompanies = new List<APICompany>();

            Console.WriteLine("Welcome to Inn2Power Datastream DB Updater!");            
            Console.WriteLine("Press Enter to login.");
            Console.ReadLine();

            RunInitialMenues();
            RunMenues();
        }

        static void RunMenues()
        {
            _menu1 = new Menu1(
                _newCompanies,
                _existingCompanies, 
                API_CONNECTIONSTRING,
                _bearerToken, 
                _username, 
                _password);

            var result =_menu1.RunMenu1();
            _bearerToken = result.Payload.ToString();

            //Signal temination, test code
            {
                Console.Clear();
                Console.WriteLine("end of the line");
                Console.ReadLine();
            }
        }

        static void RunInitialMenues()
        {
            //Running IitialMenues.!
            LoginMenu();
            DataStreamMenu();
            ApiMenu();
            InitialWorkMenu();
        }

        /// <summary>
        /// Containing login related dialog and logic calls.
        /// </summary>
        static void LoginMenu()
        {
            while (!_loggedInd)
            {
                Console.Clear();
                Console.WriteLine("To use this application please provide valid login information,");
                Console.WriteLine("to acces the Inn2Power API.");
                Console.WriteLine("Please enter Username:");
                var username = Console.ReadLine().Trim();
                Console.WriteLine("Please enter Password:");
                var password = Console.ReadLine().Trim();

                var result = _autherService.Login(username, password).Result;
                if (result.IsSuccesFull == false)
                {
                    Console.WriteLine("");
                    Console.WriteLine(result.ErrorMessage);
                    Console.WriteLine("Press Enter to try login again.");
                    Console.ReadLine();
                }
                else
                {
                    //sets global variables
                    _password = password;
                    _username = username;
                    _bearerToken = result.Payload.ToString();

                    Console.Clear();
                    Console.WriteLine("You are now logged in!!");
                    _loggedInd = true;
                    Console.WriteLine("Press any key to continue to menues.");
                    Console.ReadLine();
                }
            }
        }

        /// <summary>
        /// Containing DataStream related dialog and logic calls.
        /// </summary>
        static void DataStreamMenu()
        {
            while (!_gotDataFromDataStream)
            {
                Console.Clear();
                Console.WriteLine("Getting Data from DataStream....");
                var result = _dataStreamService.GetDataFromStream().Result;
                if (!result.IsSuccesFull)
                {
                    Console.Clear();
                    Console.WriteLine("We were unable to fetch Datastream data due to:");
                    Console.WriteLine(result.ErrorMessage);
                    Console.WriteLine("Press enter to try again.");
                    Console.ReadLine();
                }
                else
                {
                    _dataStramCompanies = (List<DataStreamCompany>) result.Payload;

                    Console.Clear();
                    Console.WriteLine($"Wee have recieved {_dataStramCompanies.Count} items from the stream.");                    
                    _gotDataFromDataStream = true;
                    Console.WriteLine("Press Enter to Continue.");
                    //Console.ReadLine();
                }
            }            
        }

        /// <summary>
        /// Containing Inn2POWER GetEverything realated dialog and logic calls.
        /// </summary>
        static void ApiMenu()
        {
            while (!_gotDataFromAPI)
            {
                Console.Clear();
                Console.WriteLine("Getting Data from Inn2Power API....");
                var result = _apiService.GetApiCompanies(_bearerToken).Result;
                if (!result.IsSuccesFull)
                {
                    Console.Clear();
                    Console.WriteLine("We were unable to fetch Datastream data due to:");
                    Console.WriteLine(result.ErrorMessage);
                    Console.WriteLine("Press enter to try again.");
                    Console.ReadLine();
                }
                else
                {
                    _apiCompanies = (List<APICompany>)result.Payload;

                    Console.Clear();
                    Console.WriteLine($"Wee have recieved {_apiCompanies.Count} items from the API.");
                    _gotDataFromAPI = true;
                    Console.WriteLine("Press Enter to Continue.");
                    //Console.ReadLine();
                }
            }
        }

        /// <summary>
        /// Contains dialog and logic calls related to data preperation.
        /// </summary>
        static void InitialWorkMenu()
        {
            Console.Clear();
            Console.WriteLine("Prepering data for Work.....");
            try
            {
                var result = _logic.PrepareCompanies(_dataStramCompanies, _apiCompanies);
                if (result.IsSuccesFull == false)
                {
                    Console.Clear();
                    Console.WriteLine(result.ErrorMessage);
                }
                else
                {
                    ListsSubResult subResult = (ListsSubResult) result.Payload;
                    _newCompanies = subResult.NewCompanies;
                    _existingCompanies = subResult.ExistingCompanies;

                    Console.Clear();
                    Console.WriteLine($"There are NewCompanies: {_newCompanies.Count}");
                    Console.WriteLine($"There are ExistingCompanies: {_existingCompanies.Count}");
                    Console.WriteLine("Press Enter to continue.");
                   // Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.Clear();
                Console.WriteLine("Something went wrong in (logic.PrepareCompanies). Please restart applikation.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Inn2PowerDataStreamUpdater.BLL;
using Inn2PowerDataStreamUpdater.Menues;
using Inn2PowerDataStreamUpdater.Misc;
using Inn2PowerDataStreamUpdater.Misc.Entities;
using Inn2PowerDataStreamUpdater.Services;

namespace Inn2PowerDataStreamUpdater
{
    class Program
    {
        //Services -----
        private static AuthService _autherService;             
        
        private const string DATASTREAM_URL = "http://inn2power.eu/mapping/api/feed?key=0lUwFrGpTqfI9oSNISCUF7m5UYzWLtCU";
        //private const string API_CONNECTIONSTRING = "http://api.mjapps.dk";
        private const string API_CONNECTIONSTRING = "https://localhost:44346";
        private static string BEARER_TOKEN = "";
       
        //Menues-----------
        private static Menu1 _menu1;
        
        static void Main(string[] args)
        {
            _autherService = new AuthService(API_CONNECTIONSTRING);
            BEARER_TOKEN = LoginMenu();
            _menu1 = new Menu1(API_CONNECTIONSTRING, BEARER_TOKEN, DATASTREAM_URL);                      
            _menu1.RunMenu1();
        }
              
        /// <summary>
        /// Containing login related dialog and logic calls.
        /// </summary>
        static string LoginMenu()
        {
            bool _loggedInd = false;
            var token = "";

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

                    Console.Clear();
                    Console.WriteLine("You are now logged in!!");
                    _loggedInd = true;
                    Console.WriteLine("Press any key to continue to menues.");
                    Console.ReadLine();
                    token = result.Payload.ToString();
                }
            }
            return token;
        }       
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Inn2PowerDataStreamUpdater.Misc;
using Newtonsoft.Json;

namespace Inn2PowerDataStreamUpdater
{
    public class AuthService
    {
        private readonly string _authURL = "/auth/signin";

        private readonly string _apiConnectionstring;
        private readonly HttpClient client;

        public AuthService(string apiconnectionString)
        {
            if(string.IsNullOrWhiteSpace(apiconnectionString))
                throw new ArgumentNullException(nameof(apiconnectionString));

            _apiConnectionstring = apiconnectionString;
            this.client = new HttpClient();
        }

        public async Task<ResultObject> Login(string username, string password)
        {
            //retuns if bad username and pawword inputs
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                var resultobject = new ResultObject();
                resultobject.IsSuccesFull = false;
                resultobject.ErrorMessage = "Please enter valid Username and Password!";

                return resultobject;
            }
            try
            {               
                var expected = JsonConvert.SerializeObject(new { username = username, password = password });

                var response = client.PostAsync(_apiConnectionstring + _authURL,
                    new StringContent(expected, Encoding.UTF8, "application/json")).Result;

                //returns if we could not login.
                if (response.IsSuccessStatusCode == false)
                {
                    var resultobject1 = new ResultObject();
                    resultobject1.IsSuccesFull = false;
                    resultobject1.ErrorMessage = "Username or Password was wrong.";
                    return resultobject1;
                }

                //returns if we logged in.
                var resultobject = new ResultObject();
                resultobject.IsSuccesFull = true;
                resultobject.Payload = response.Content.ReadAsStringAsync().Result;

                client.Dispose();

                return resultobject;
            }
            catch (Exception e)
            {
                //returns if something went wrong contatcting the API.
                var resultobject = new ResultObject();
                resultobject.IsSuccesFull = false;
                resultobject.ErrorMessage = "Something went wrong. Try again.";
                return resultobject;
            }
                        
        }       
    }
}

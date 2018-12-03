using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Inn2PowerDataStreamUpdater.Misc;
using Newtonsoft.Json;

namespace Inn2PowerDataStreamUpdater.Services
{
    public class ApiService
    {
        private readonly string _getAllURL = "/company/everything";
        private readonly string _updateCompanieURL = "/datastream/update";
        private readonly string _CreateCompanieURL = "/datastream/create";

        private readonly string _apiconnectionstring;       
        private readonly HttpClient client;

        public ApiService(string apiconnectionstring)
        {
            if(string.IsNullOrWhiteSpace(apiconnectionstring))
                throw new ArgumentNullException(nameof(apiconnectionstring));
            
            this._apiconnectionstring = apiconnectionstring;           
            this.client = new HttpClient();
        }

        public async Task<ResultObject> GetApiCompanies(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                var badresultobject = new ResultObject();
                badresultobject.IsSuccesFull = false;
                badresultobject.ErrorMessage = "Token was empty. Please restart applikation.";
                return badresultobject;
            }               

            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);

                HttpResponseMessage response = client.GetAsync(new Uri(_apiconnectionstring + _getAllURL)).Result;

                var dataResult = response.Content.ReadAsStringAsync().Result;

                var result = JsonConvert.DeserializeObject<List<APICompany>>(dataResult);

                var resultobject = new ResultObject();
                resultobject.IsSuccesFull = true;
                resultobject.Payload = result;
                return resultobject;
            }
            catch (Exception e)
            {
                var badresultobject = new ResultObject();
                badresultobject.IsSuccesFull = false;
                badresultobject.ErrorMessage = "Something went wrong getting API companies.";
                return badresultobject;
            }
        }

        //Still to be tested!!!!
        public async Task<ResultObject> UpdateCompanies(string token, List<APICompany> companies)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                var badresultobject = new ResultObject();
                badresultobject.IsSuccesFull = false;
                badresultobject.ErrorMessage = "Token was empty. Please restart applikation.";
                return badresultobject;
            }
            if (!companies.Any())
            {
                var succesResult = new ResultObject();
                succesResult.IsSuccesFull = true;
                succesResult.Payload = companies;
                return succesResult;
            }

            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);

                var payload = JsonConvert.SerializeObject(companies);

                var response = client.PostAsync(_apiconnectionstring + _updateCompanieURL,
                    new StringContent(payload, Encoding.UTF8, "application/json")).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var badresultobject = new ResultObject();
                    badresultobject.IsSuccesFull = false;
                    badresultobject.ErrorMessage = "Something went wrong.";
                    return badresultobject;
                }
                else
                {
                    var succesResult = new ResultObject();
                    succesResult.IsSuccesFull = true;
                    succesResult.Payload = companies;
                    return succesResult;
                }                
            }
            catch (Exception e)
            {
                var badresultobject = new ResultObject();
                badresultobject.IsSuccesFull = false;
                badresultobject.ErrorMessage = "Something went wrong.";
                return badresultobject;
            }           
        }

        public async Task<ResultObject> CreateCompanies(string token, List<APICompany> companies)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                var badresultobject = new ResultObject();
                badresultobject.IsSuccesFull = false;
                badresultobject.ErrorMessage = "Token was empty. Please restart applikation.";
                return badresultobject;
            }
            if (!companies.Any())
            {
                var succesResult = new ResultObject();
                succesResult.IsSuccesFull = true;
                succesResult.Payload = companies;
                return succesResult;
            }

            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);

                var payload = JsonConvert.SerializeObject(companies);

                var response = client.PostAsync(_apiconnectionstring + _CreateCompanieURL,
                    new StringContent(payload, Encoding.UTF8, "application/json")).Result;


            }
            catch (Exception e)
            {
                var badresultobject = new ResultObject();
                badresultobject.IsSuccesFull = false;
                badresultobject.ErrorMessage = "Something went wrong.";
                return badresultobject;
            }

            return null;
        }
    }
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Inn2PowerDataStreamUpdater.Misc;
using Inn2PowerDataStreamUpdater.Misc.Entities;
using Newtonsoft.Json;

namespace Inn2PowerDataStreamUpdater.Services
{
    public class ApiService
    {
        private readonly string _getAllURL = "/company/everything";
        private readonly string _getSupplyChaincategories = "/SupplyChainCategory/GetSupplyChainCategorys";
        private readonly string _getSupplyChainRoles = "/SupplyChainRole/GetSypplyChainRoles";
        private readonly string _updateCompanieURL = "/datastream/UpdateCompanies";
        private readonly string _CreateCompanieURL = "/datastream/CreateCompanies";

        private readonly string _apiconnectionstring;       
        

        public ApiService(string apiconnectionstring)
        {
            if(string.IsNullOrWhiteSpace(apiconnectionstring))
                throw new ArgumentNullException(nameof(apiconnectionstring));
            
            this._apiconnectionstring = apiconnectionstring;           
            
        }

        public async Task<ResultObject> GetSupplyChainCategorys(string token)
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
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 200);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = client.GetAsync(new Uri(_apiconnectionstring + _getSupplyChaincategories)).Result;

                    if (response.IsSuccessStatusCode == false)
                    {
                        var badresultobject = new ResultObject();
                        badresultobject.IsSuccesFull = false;
                        badresultobject.ErrorMessage = "Something went wrong getting API companies.";
                        return badresultobject;
                    }

                    var dataResult = response.Content.ReadAsStringAsync().Result;

                    var result = JsonConvert.DeserializeObject<List<SupplyChainCategory>>(dataResult);

                    var resultobject = new ResultObject();
                    resultobject.IsSuccesFull = true;
                    resultobject.Payload = result;
                    return resultobject;
                }              
            }
            catch (Exception e)
            {
                var badresultobject = new ResultObject();
                badresultobject.IsSuccesFull = false;
                badresultobject.ErrorMessage = "Something went wrong getting API SupplyChainCategories.";
                return badresultobject;
            }

        }

        public async Task<ResultObject> GetSupplyChainRoles(string token)
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
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 200);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = client.GetAsync(new Uri(_apiconnectionstring + _getSupplyChainRoles)).Result;

                    if (response.IsSuccessStatusCode == false)
                    {
                        var badresultobject = new ResultObject();
                        badresultobject.IsSuccesFull = false;
                        badresultobject.ErrorMessage = "Something went wrong getting API companies.";
                        return badresultobject;
                    }

                    var dataResult = response.Content.ReadAsStringAsync().Result;

                    var result = JsonConvert.DeserializeObject<List<SupplyChainRole>>(dataResult);

                    var resultobject = new ResultObject();
                    resultobject.IsSuccesFull = true;
                    resultobject.Payload = result;
                    return resultobject;
                }                
            }
            catch (Exception e)
            {
                var badresultobject = new ResultObject();
                badresultobject.IsSuccesFull = false;
                badresultobject.ErrorMessage = "Something went wrong getting API SupplyChainRoles.";
                return badresultobject;
            }

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
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 200);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = client.GetAsync(new Uri(_apiconnectionstring + _getAllURL)).Result;

                    var dataResult = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode == false)
                    {
                        var badresultobject = new ResultObject();
                        badresultobject.IsSuccesFull = false;
                        badresultobject.ErrorMessage = "Something went wrong getting API companies.";
                        return badresultobject;
                    }

                    var result = JsonConvert.DeserializeObject<List<APICompany>>(dataResult);

                    var resultobject = new ResultObject();
                    resultobject.IsSuccesFull = true;
                    resultobject.Payload = result;
                    return resultobject;
                }                
            }
            catch (Exception e)
            {
                var badresultobject = new ResultObject();
                badresultobject.IsSuccesFull = false;
                badresultobject.ErrorMessage = "Something went wrong getting API companies.";
                return badresultobject;
            }
        }

        
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
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 200);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var payload = JsonConvert.SerializeObject(companies);

                    var response = client.PutAsync(_apiconnectionstring + _updateCompanieURL,
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
            }
            catch (Exception e)
            {
                var badresultobject = new ResultObject();
                badresultobject.IsSuccesFull = false;
                badresultobject.ErrorMessage = "Something went wrong.";               
                return badresultobject;
            }           
        }

        /// <summary>
        /// Sends Http request to Inn2POWER API 
        /// with a list of new companies to create.
        /// </summary>
        /// <param name="token">Bearer token.</param>
        /// <param name="companies">Companies to be created</param>
        /// <returns>ResultObject</returns>
        public async Task<ResultObject> CreateCompanies(string token, List<APICompany> companies)
        {
            var badresultobject = new ResultObject();
            var succesResult = new ResultObject();
            if (string.IsNullOrWhiteSpace(token))
            {
               
                badresultobject.IsSuccesFull = false;
                badresultobject.ErrorMessage = "Token was empty. Please restart applikation.";
                return badresultobject;
            }
            if (!companies.Any())
            {                
                succesResult.IsSuccesFull = true;
                succesResult.Payload = companies;
                return succesResult;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 200);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var payload = JsonConvert.SerializeObject(companies);

                    var response = client.PostAsync(_apiconnectionstring + _CreateCompanieURL,
                        new StringContent(payload, Encoding.UTF8, "application/json")).Result;


                    if (response.IsSuccessStatusCode)
                    {
                        succesResult.IsSuccesFull = true;
                        succesResult.Payload = response;
                        return succesResult;
                    }
                    else
                    {
                        badresultobject.IsSuccesFull = false;
                        badresultobject.ErrorMessage = "Something went wrong and not all comapnies got created.";
                        return badresultobject;
                    }
                }              
            }
            catch (Exception e)
            {                
                badresultobject.IsSuccesFull = false;
                badresultobject.ErrorMessage = "Something went wrong.";                
                return badresultobject;
            }            
        }
    }
}

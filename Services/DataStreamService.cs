using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Inn2PowerDataStreamUpdater.Misc;
using Newtonsoft.Json;

namespace Inn2PowerDataStreamUpdater
{
    public class DataStreamService
    {
        private readonly string _dataStreamUrl;
        private readonly HttpClient client;

        public DataStreamService(string datastreamUrl)
        {
            if (string.IsNullOrWhiteSpace(datastreamUrl))
                throw new ArgumentNullException(nameof(datastreamUrl));

            _dataStreamUrl = datastreamUrl;
            this.client = new HttpClient();
        }

        public async Task<ResultObject> GetDataFromStream()
        {
            try
            {
                //contacts the datastream to get data.
                HttpResponseMessage response = client.SendAsync
                    (new HttpRequestMessage(HttpMethod.Get, new Uri(_dataStreamUrl))).Result;

                if (response.IsSuccessStatusCode == false)
                {
                    var badResultObject = new ResultObject();
                    badResultObject.IsSuccesFull = false;
                    badResultObject.ErrorMessage = "Could not fetch data";
                    return badResultObject;
                }               

                var datastreamResult = response.Content.ReadAsStringAsync().Result;

                var result = JsonConvert.DeserializeObject<List<DataStreamCompany>>(datastreamResult);
                

                var resultObject = new ResultObject();
                resultObject.IsSuccesFull = true;
                resultObject.Payload = result;
                return resultObject;
            }
            catch (Exception e)
            {
                var badResultObject = new ResultObject();
                badResultObject.IsSuccesFull = false;
                badResultObject.ErrorMessage = "Could not fetch data (Something went wrong.)";
                return badResultObject;
            }
        }
    }
}

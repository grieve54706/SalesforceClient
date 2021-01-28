using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace sf_demo.Salesforce
{
    public class SalesforceClient
    {
        public const string ProdLoginEndpoint = "https://login.salesforce.com/services/oauth2/token";
        public const string TestLoginEndpoint = "https://test.salesforce.com/services/oauth2/token";
        public static string ApiEndpoint = "/services/data/v{0}/";
        public static string LoginEndpoint { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static string ClientId { get; set; }
        public static string ClientSecret { get; set; }
        public static string AccessToken { get; set; }
        public static string ServiceUrl { get; set; }
        private static HttpClient Client;
        private const string ApplicationType = "application/json";

        public void connect()
        {
            Client = CreateClient();

            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"grant_type", "password"},
                {"username", Username},
                {"password", Password},
                {"client_id", ClientId},
                {"client_secret", ClientSecret}
            });

            HttpResponseMessage message = Client.PostAsync(LoginEndpoint, content).Result;

            string response = message.Content.ReadAsStringAsync().Result;
            JObject obj = JObject.Parse(response);

            AccessToken = (string)obj["access_token"];
            ServiceUrl = (string)obj["instance_url"];

            Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
        }

        /**
         * Related to https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/resources_composite_sobjects_collections_create.htm
         */
        public async Task<List<MultiResult>> InsertAsync<T>(List<T> objs)
        {
            var objName = typeof(T).Name;

            var records = BuildRecords(objName, objs);

            var reqBodyDict = new Dictionary<string, Object>();
            reqBodyDict["allOrNone"] = false;
            reqBodyDict["records"] = records;

            var message = JsonConvert.SerializeObject(reqBodyDict);

            string uri = $"{ServiceUrl}{ApiEndpoint}composite/sobjects";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(message, Encoding.UTF8, ApplicationType);

            using (HttpResponseMessage response = await Client.SendAsync(request))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return DeserializeObjectWithCathJsonException<List<MultiResult>>(responseString);
            }
        }

        /**
         * Related to https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/resources_query.htm
         */
        public async Task<List<T>> QueryAsync<T>(string command)
        {
            var selectResult = new SelectResult<T>();
            var records = new List<T>();

            command = HttpUtility.UrlEncode(command);

            string uri = $"{ServiceUrl}{ApiEndpoint}query?q={command}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            do
            {
                using (HttpResponseMessage response = await Client.SendAsync(request))
                {
                    var responseString = response.Content.ReadAsStringAsync().Result;

                    selectResult = DeserializeObjectWithCathJsonException<SelectResult<T>>(responseString);

                    if (selectResult?.Records != null)
                    {
                        records.AddRange(selectResult.Records);
                    }
                }
            } while (!selectResult.Done);

            return records;
        }

        /**
         * Related to https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/resources_composite_sobjects_collections_update.htm
         */
        public async Task<List<MultiResult>> UpdateAsync<T>(List<T> objs)
        {
            var objName = typeof(T).Name;

            var records = BuildRecords(objName, objs);

            var reqBodyDict = new Dictionary<string, Object>();
            reqBodyDict["allOrNone"] = false;
            reqBodyDict["records"] = records;

            var message = JsonConvert.SerializeObject(reqBodyDict);

            string uri = $"{ServiceUrl}{ApiEndpoint}composite/sobjects";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Content = new StringContent(message, Encoding.UTF8, ApplicationType);

            using (HttpResponseMessage response = await Client.SendAsync(request))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return DeserializeObjectWithCathJsonException<List<MultiResult>>(responseString);
            }
        }

        /**
         * Related to https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/resources_composite_sobjects_collections_delete.htm
         */
        public async Task<List<MultiResult>> DeleteAsync<T>(List<T> objs)
        {
            var ids = GetIds(objs);

            var joinedIds = String.Join(",", ids);

            string uri = $"{ServiceUrl}{ApiEndpoint}composite/sobjects?ids={joinedIds}&allOrNone=false";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);

            using (HttpResponseMessage response = await Client.SendAsync(request))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return DeserializeObjectWithCathJsonException<List<MultiResult>>(responseString);
            }
        }

        private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", ApplicationType);
            return client;
        }

        private object BuildRecords<T>(string objName, List<T> objs)
        {
            var objDicts = new List<OrderedDictionary>();

            objs.ForEach(obj => {

                var objDict = ToDictionary(obj);

                var typeDict = new Dictionary<string, string>();
                typeDict["type"] = objName;

                // attributes must be at first.
                objDict.Insert(0, "attributes", typeDict);

                objDicts.Add(objDict);
            });

            return objDicts;
        }

        private OrderedDictionary ToDictionary(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<OrderedDictionary>(json);
        }

        private List<string> GetIds<T>(List<T> objs)
        {
            return objs.Select(obj => GetAttribute<T, string>(obj, "Id")).ToList();
        }

        private TFieldType GetAttribute<TObjectType, TFieldType>(TObjectType obj, string fieldName)
        {
            return (TFieldType) typeof(TObjectType).GetProperty(fieldName).GetValue(obj);
        }

        private T DeserializeObjectWithCathJsonException<T>(String responseString)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(responseString);
            }
            catch (JsonException exception)
            {
                throw new SalesforceException(
                    SalesforceError.JsonDeserializationError,
                    $"Response: {responseString}",
                    exception
                    );
            }
        }
    }
}
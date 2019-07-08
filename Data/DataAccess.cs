using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HeyDo.Controllers;

namespace HeyDo.Data
{
    public class DataAccess
    {
        private static HttpClient _client = new HttpClient();
        private static string baseUrl = AppSettings.AppSetting["FirebaseBaseUrl"];
        
        //TODO clean up and test
        public static async Task<JObject> ApiGoogle(string method, string json, string sub, Dictionary<string,string> auth)
        {
            var authCheck = await AuthController.Google(auth["token"]);
            //Make sure user is authorized
            if (authCheck == auth["uid"])
            {
                var url = baseUrl + sub + ".json?auth="+auth["token"];
                var res = new HttpResponseMessage();
                switch (method)
                {
                    case "PUT":
                        HttpContent newContent = new StringContent(json, Encoding.UTF8, "application/json");
                        res = await _client.PutAsync(url, newContent);
                        break;
                    case "GET":
                        res = await _client.GetAsync(url);
                        break;
                    case "PATCH":
                        HttpContent updateContent = new StringContent(json, Encoding.UTF8, "application/json");
                        res = await _client.PatchAsync(url, updateContent);
                        break;
                    case "DELETE":
                        res = await _client.DeleteAsync(url);
                        break;
                    default:
                            break;
                }

                if (res.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new JObject() {
                        { "Error", res.ReasonPhrase }
                    };
                }
                else
                {
                    var interim = await res.Content.ReadAsStringAsync();

                    return interim == null || interim == "null" ? new JObject() : JsonConvert.DeserializeObject<JObject>(interim);
                }
            }
            else
            {
                return new JObject() {
                    {"Error",authCheck }
                };
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeyDo.Data
{
    public class DataAccess
    {
        private static HttpClient _client = new HttpClient();
        private static string baseUrl = "";
        
        //TODO clean up and test
        public static async Task<JObject> ApiGoogle(string method, string json, string sub)
        {
            var url = baseUrl + sub + ".json";
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

            var interim = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<JObject>(interim);
        }

    }
}

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
        public static async Task<JObject> ApiGoogle(string method, string json, string sub,
            Dictionary<string, string> auth, bool admin = false)
        {
            var token = admin ? AppSettings.AppSetting["HangFireAccess"] : auth["token"];

            var authCheck = await AuthController.Google(token);
            //Make sure user is authorized or system user
            if (authCheck == auth["uid"] || admin)
            {
                
                var url = baseUrl + sub + ".json?auth=" + token;
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
                }

                if (res.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new JObject()
                    {
                        {"Error", res.ReasonPhrase}
                    };
                }

                var interim = await res.Content.ReadAsStringAsync();

                return interim == null || interim == "null"
                    ? new JObject()
                    : JsonConvert.DeserializeObject<JObject>(interim);

            }

            return new JObject()
            {
                {"Error", authCheck}
            };

        }
    }
}

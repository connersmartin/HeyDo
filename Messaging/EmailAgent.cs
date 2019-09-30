using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HeyDo.Models;
using Newtonsoft.Json;

namespace HeyDo.Messaging
{
    public class EmailAgent
    {
        /// <summary>
        /// Sends out an email via SendInBlue
        /// </summary>
        /// <param name="emailData">Email data object, sender,subject, to , etc</param>
        public static async Task<string> SendMail(MessageData emailData)
        {
            //RESTSharp
            //var client = new RestClient("https://api.sendinblue.com/v3/smtp/email");
            //var request = new RestRequest(Method.POST);          
            //request.AddHeader("api-key", AppSettings.AppSetting["EmailAPIKey"]);
            //request.AddHeader("Content-Type", "application/json");
            //request.AddParameter("undefined", JsonConvert.SerializeObject(emailData), ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            //return response.StatusCode.ToString();

            //Httpclient
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.sendinblue.com");
                client.DefaultRequestHeaders.Accept.Add
                    (new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("api-key", AppSettings.AppSetting["EmailAPIKey"]);
                var request = new HttpRequestMessage(HttpMethod.Post, "/v3/smtp/email");
                request.Content = new StringContent(JsonConvert.SerializeObject(emailData),
                                                    Encoding.UTF8,
                                                    "application/json");
                var res = await client.SendAsync(request);
                return await res.Content.ReadAsStringAsync();
            }
        }

    }
}

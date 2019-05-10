using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HeyDo.Models;
using Newtonsoft.Json;
using RestSharp;

namespace HeyDo.Messaging
{
    public class EmailAgent
    {
        /// <summary>
        /// Sends out an email via SednInBlue
        /// </summary>
        /// <param name="emailData">Email data object, sender,subject, to , etc</param>
        public static string SendMail(MessageData emailData)
        {
            var client = new RestClient("https://api.sendinblue.com/v3/smtp/email");
            var request = new RestRequest(Method.POST);          
            request.AddHeader("api-key", "");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined", JsonConvert.SerializeObject(emailData), ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            return response.StatusCode.ToString();

        }

    }
}

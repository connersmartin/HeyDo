using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using HeyDo.Data;
using HeyDo.Models;
using Microsoft.Extensions.Configuration;
using Twilio.Rest.Api.V2010.Account;

namespace HeyDo.Messaging
{
    public class SmsAgent
    {
        /// <summary>
        /// Sends an SMS via twilio to a given number
        /// </summary>
        /// <param name="messageData">Should only be sending to number and subject + text from email</param>
        public static void TwiSend(MessageData messageData)
        {
            var ownPhone = ""; 
            const string accountSid = "";
            const string authToken = "";

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: messageData.textContent,
                from: new Twilio.Types.PhoneNumber(ownPhone),
                to: new Twilio.Types.PhoneNumber("+1"+messageData.to.First().email)
            );

        }
    }
        
}

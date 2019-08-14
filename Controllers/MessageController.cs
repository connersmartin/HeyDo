using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeyDo.Data;
using HeyDo.Messaging;
using HeyDo.Models;
using HeyDo.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace HeyDo.Controllers
{
    public class MessageController : Controller
    {

        /// <summary>
        /// Sends a message to a user
        /// </summary>
        /// <param name="msg">Message information</param>
        /// <param name="cType">Contact type, Email or Phone</param>
        public void SendMessage(MessageData msg, Enums.ContactType cType)
        {
            //TODO figure out how to run OnScheduledEvent to schedule the next notification instead of CRON strings
            //don't need to send a message while testing
            if (true)
            {
                Console.WriteLine("Done");
            }
            else
            {
                switch (cType)
                {
                    case Enums.ContactType.Email:
                        //Test data
                        //var success = EmailAgent.SendMail(TestData.TestSms);
                        //Real life
                        var emailSuccess = EmailAgent.SendMail(msg);
                        break;
                    case Enums.ContactType.Phone:
                        //Test data
                        //SmsAgent.TwiSend(TestData.TestSms);
                        //Real life
                        var smsSuccess = SmsAgent.TwiSend(msg);
                        break;
                    default:
                        break;
                }       
                
            }
        }
        public void OnScheduledEvent()
        {
            //This could be used to have scheduled events get scheduled one by one
            //need to figure out how this could/should be done when thinking about authentication.
            //master user?
        }
    }

}
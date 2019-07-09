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
            switch (cType)
            {
                case Enums.ContactType.Email:
                    //Test data
                    //var success = EmailAgent.SendMail(TestData.TestSms);
                    //Real life
                    var success = EmailAgent.SendMail(msg);
                    break;
                case Enums.ContactType.Phone:
                    //Test data
                    //SmsAgent.TwiSend(TestData.TestSms);
                    //Real life
                    SmsAgent.TwiSend(msg);
                    break;
                default:
                    break;
            }            
        }
    }
}
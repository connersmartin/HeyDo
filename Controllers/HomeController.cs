using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HeyDo.Models;
using HeyDo.Data;
using HeyDo.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace HeyDo.Controllers
{
    public class HomeController : Controller
    {
        public const string SessionKeyUid = "_Uid";
        public const string SessionKeyAuth = "_Auth";
        public void SetSession(string auth, string uid)
        {
            //TODO figure out how to freaking set session variables in dot net core
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyUid)))
            {
                HttpContext.Session.SetString(SessionKeyUid, uid);
                HttpContext.Session.SetString(SessionKeyAuth, auth);
            }
            
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        //Sends a text message

        public void SendText()
        {
            SmsAgent.TwiSend(TestData.TestSms);
        }

        //Sends an email
        public void SendEmail()
        {
            var success = EmailAgent.SendMail(TestData.TestSms);
        }

        public async void AddData(string uid, string auth, Enums.DataType dataType)
        {
            var sUid = HttpContext.Session.GetString(SessionKeyUid);
            var sAuth = HttpContext.Session.GetString(SessionKeyAuth);

            //test data
            var data = TestData.Contests;

            var json = JsonConvert.SerializeObject(data);

            var obData = JsonConvert.DeserializeObject<JObject>(json);

            //var url = dataType + "/" + uid + "/" + obData["Id"];

            var url = dataType+"/" + HttpContext.Session.GetString(SessionKeyUid) + "/" + data.Id;

            await DataAccess.ApiGoogle("PUT", json, url, HttpContext.Session.GetString(SessionKeyAuth));
        }

        public async void GetData(string uid, string auth, string dataType)
        {
            var url = dataType + "/" + uid;

            var data = await DataAccess.ApiGoogle("GET", "", url, auth);

            var dataList = JsonConvert.DeserializeObject<List<User>>(data.ToString());

        }

        //Add a new user
        public IActionResult NewUser()
        {
            return View();
        }

        //Add a new task
        public IActionResult NewTask()
        {
            return View();
        }

        //View all users
        public IActionResult ViewUsers()
        {
            return View(TestData.TestUsers);
        }

        //View all tasks
        public IActionResult ViewTasks()
        {
            return View(TestData.TestTasks);
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

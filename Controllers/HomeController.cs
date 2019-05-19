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
using Newtonsoft.Json;


namespace HeyDo.Controllers
{
    public class HomeController : Controller
    {
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

        public async void AddData(string uid, string auth, string dataType)
        {
            //test data
            var data = TestData.Contests;

            var json = JsonConvert.SerializeObject(data);

            var url = dataType+"/" + uid + "/" + data.Id;

            await DataAccess.ApiGoogle("PUT", json, url, auth);
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

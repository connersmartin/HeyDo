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
using System.Net.Http;

namespace HeyDo.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
       
        // This should give an at a glance thing for an admin
        //Current users/tasks, links to add/edit/remove things
        //view past tasks etc
        public IActionResult Dashboard()
        {
             return View();
        }

        //Add a new user
        public async Task<IActionResult> NewUser(string uid, string auth, User user)
        {
            var jData = JsonConvert.SerializeObject(user);
            var data = await DataController.AddData(uid, auth, Enums.DataType.Users, jData);

            return View();
        }

        //Add a new task
        public async Task<IActionResult> NewTask(string uid, string auth, TaskItem task)
        {
            var jData = JsonConvert.SerializeObject(task);
            var data = await DataController.AddData(uid, auth, Enums.DataType.Tasks, jData);

            return View();
        }

        //View all users
        public async Task<IActionResult> ViewUsers(string uid, string auth)
        {
            //Real life
            /*
            var data = await DataController.GetData(uid, auth, Enums.DataType.Users);

            var userList = new List<User>();
            foreach (var user in data)
            {
                userList.Add(user.ToObject<User>());
            }

            return View(userList);
            */
            //Test data
            return View(TestData.TestUsers);
        }

        //View all tasks
        public async Task<IActionResult> ViewTasks(string uid, string auth)
        {
            //Real life
            /*
            var data = await DataController.GetData(uid, auth, Enums.DataType.Tasks);

            var taskList = new List<TaskItem>();
            foreach (var task in data)
            {
                taskList.Add(task.ToObject<TaskItem>());
            }
            return View(taskList);
            */
            //Test data
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

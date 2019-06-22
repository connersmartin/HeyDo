﻿using System;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }



        [HttpGet]
        public IActionResult Index()
        {
            var dict = GetCookies();

            if (dict["uid"] != null && dict["token"] != null)
            {
                return View("Dashboard");
            }
            else
            {
                    
                return View();
            }

        }


        public IActionResult Dashboard()
        {
            var dict = GetCookies();

            if (dict["uid"] == null && dict["token"] == null)
            {
                return View("Index");
            }
            else
            {

                return View();
            }
        }

        #region User Management

        [HttpGet]
        public IActionResult NewUser()
        {
            var dict = GetCookies();

            return View();
        }

        //Add a new user
        [HttpPost]
        public async Task<IActionResult> NewUser(User user)
        {

            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(user);
            var data = await DataController.AddData(dict, Enums.DataType.Users, jData);

            return RedirectToAction("ViewUsers");
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string Id)
        {
            var dict = GetCookies();

            var data = await DataController.GetData(dict, Enums.DataType.Users, "/"+Id);

            if (data.FirstOrDefault().ContainsKey("Error"))
            {
                return RedirectToAction("Logout");
            }
            else
            {
               var user = data.FirstOrDefault().ToObject<User>();


                return View(user);
            }           
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(User user)
        {
            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(user);
            var data = await DataController.AddData(dict, Enums.DataType.Users, jData, true);
            return RedirectToAction("ViewUsers");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            var dict = GetCookies();
            var data = await DataController.DeleteData(dict, Enums.DataType.Users, "/"+Id);
            return RedirectToAction("ViewUsers");
        }

        //View all users
        [HttpGet]
        public async Task<IActionResult> ViewUsers()
        {
            var dict = GetCookies();
            //Real life
            var userList = new List<User>();
            var data = await DataController.GetData(dict, Enums.DataType.Users);

            if (data.Count > 0)
            {
                if (data.FirstOrDefault().ContainsKey("Error"))
                {
                    return RedirectToAction("Logout");
                }
                else
                {
                
                    foreach (var user in data)
                    {
                        userList.Add(user.ToObject<User>());
                    }

                    return View(userList);
                }

            }
            else
            {
                return View(userList);
            }


            //Test data
            //return View(TestData.TestUsers);
        }
        #endregion

        #region Task Management
        [HttpGet]
        public IActionResult NewTask()
        {

            return View();
        }

        //Add a new task
        [HttpPost]
        public async Task<IActionResult> NewTask(TaskItem task)
        {
            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(task);
            var data = await DataController.AddData(dict, Enums.DataType.Tasks, jData);

            return RedirectToAction("ViewTasks");
        }

        [HttpGet]
        public async Task<IActionResult> EditTask(string Id)
        {
            var dict = GetCookies();

            var data = await DataController.GetData(dict, Enums.DataType.Tasks, "/" + Id);

            if (data.FirstOrDefault().ContainsKey("Error"))
            {
                return RedirectToAction("Logout");
            }
            else
            {
                var task = data.FirstOrDefault().ToObject<TaskItem>();


                return View(task);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditTask(TaskItem task)
        {
            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(task);
            var data = await DataController.AddData(dict, Enums.DataType.Tasks, jData, true);

            return RedirectToAction("ViewTasks");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteTask(string Id)
        {
            var dict = GetCookies();
            var data = await DataController.DeleteData(dict, Enums.DataType.Tasks,Id);

            return RedirectToAction("ViewTasks");
        }

        //View all tasks
        public async Task<IActionResult> ViewTasks()
        {
            var dict = GetCookies();
            //Real life

            var data = await DataController.GetData(dict, Enums.DataType.Tasks);

            var taskList = new List<TaskItem>();

            if (data.Count > 0)
            {
                if (data.FirstOrDefault().ContainsKey("Error"))
                {
                    return RedirectToAction("Logout");
                }
                else
                {

                    foreach (var task in data)
                    {
                        taskList.Add(task.ToObject<TaskItem>());
                    }

                    return View(taskList);
                }

            }
            else
            {
                return View(taskList);
            }

            //Test data
            //return View(TestData.TestTasks);
        }

        #endregion

        #region Assignments
        public async Task<IActionResult> ViewHistory()
        {
            var dict = GetCookies();

            var data = await DataController.GetData(dict, Enums.DataType.UserTasks);
            var taskList = new List<Usertask>();
            if (data.Count > 0)
            {
                if (data.FirstOrDefault().ContainsKey("Error"))
                {
                    return RedirectToAction("Logout");
                }
                else
                {

                    foreach (var task in data)
                    {
                        taskList.Add(task.ToObject<Usertask>());
                    }

                    return View(taskList);
                }

            }
            else
            {
                return View(taskList);
            }
        }
        [HttpGet]
        public async Task<IActionResult> AssignTask()
        {
            //Get Users
            
            //Get Tasks
            
            //Get Times

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AssignTask(Usertask usertask)
        {
            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(usertask);
            var data = await DataController.AddData(dict, Enums.DataType.UserTasks, jData);

            return RedirectToAction("ViewHistory");
        }

        #endregion
        public IActionResult Logout()
        {
            Remove("uid");
            Remove("token");
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Cookie Management

        public async Task<bool> SetAuth(string auth, string uid)
        {
            var dict = GetCookies();

            var rq = HttpContext.Request.Headers;

            var a = rq["uid"];
            var b = rq["token"];
            var x = HttpContext.User;

            if (a.ToString() == null || b.ToString() == null)
            {
                a = dict["uid"];
                b = dict["token"];
            }

            if (a == await AuthController.Google(b))
            {
                if (dict["uid"] == null && dict["token"] == null)
                {
                    Set("token", b, 10000);
                    Set("uid", a, 10000);

                }
                return true;

            }
            else
            {
                return false;
            }
        }

        public Dictionary<string, string> GetCookies()
        {
            return new Dictionary<string, string>()
            {
                {"uid", Get("uid")},
                {"token", Get("token")}
            };
        }

        /// <summary>  
        /// Get the cookie  
        /// </summary>  
        /// <param name="key">Key </param>  
        /// <returns>string value</returns>  
        public string Get(string key)
        {
            return Request.Cookies[key];
        }
        /// <summary>  
        /// set the cookie  
        /// </summary>  
        /// <param name="key">key (unique indentifier)</param>  
        /// <param name="value">value to store in cookie object</param>  
        /// <param name="expireTime">expiration time</param>  
        public void Set(string key, string value, int? expireTime)
        {
            CookieOptions option = new CookieOptions();
            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(100000);
            Response.Cookies.Append(key, value, option);
        }
        /// <summary>  
        /// Delete the key  
        /// </summary>  
        /// <param name="key">Key</param>  
        public void Remove(string key)
        {
            Response.Cookies.Delete(key);
        }
        #endregion
    }
}

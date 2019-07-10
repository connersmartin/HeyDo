﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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
using Microsoft.AspNetCore.Mvc.Rendering;
using Hangfire;

namespace HeyDo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }
        public MessageController mc = new MessageController();

        #region Default Views
        [HttpGet]
        public IActionResult Index()
        {
            var dict = GetCookies();
            //Check to see if logged in
            if (dict["uid"] != null && dict["token"] != null)
            {
                return View("Dashboard");
            }
            else
            {
                ViewBag.Title = AppSettings.AppSetting["Thisisdumb"];
                return View();
            }
        }

        public IActionResult Dashboard()
        {
            var dict = GetCookies();
            //Check to see if logged in
            if (dict["uid"] == null && dict["token"] == null)
            {
                return View("Index");
            }
            else
            {
                return View();
            }
        }
        #endregion

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
            user.Id = Guid.NewGuid().ToString();
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

        [HttpGet]
        public async Task<IActionResult> ViewUsers()
        {
            var dict = GetCookies();
            //Real life
            var userList = await GetUsers(dict);

            return View(userList);

            //Test data
            //return View(TestData.TestUsers);
        }

        /// <summary>
        /// Gets all users, reusable
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        public async Task<List<User>> GetUsers(Dictionary<string, string> auth, string uid=null)
        {
            var data = new List<JObject>();
            if (uid == null)
            {
                data = await DataController.GetData(auth, Enums.DataType.Users);
            }
            else
            {
                data = await DataController.GetData(auth, Enums.DataType.Users,"/"+uid);
            }
           
            var userList = new List<User>();
            if (data.Count > 0)
            {
                if (data.FirstOrDefault().ContainsKey("Error"))
                {
                    Logout();

                    return userList;
                }
                else
                {

                    foreach (var user in data)
                    {
                        userList.Add(user.ToObject<User>());
                    }

                    return userList;
                }

            }
            else
            {
                return userList;
            }
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
            task.Id = Guid.NewGuid().ToString();
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
            var taskList = await GetTasks(dict);

            return View(taskList);

            //Test data
            //return View(TestData.TestTasks);
        }

        public async Task<List<TaskItem>> GetTasks(Dictionary<string, string> auth, string id=null)
        {
            var data = new List<JObject>();
            if (id == null)
            {
                data = await DataController.GetData(auth, Enums.DataType.Tasks);
            }
            else
            {
                data = await DataController.GetData(auth, Enums.DataType.Tasks,"/"+id);
            }
            
            var taskList = new List<TaskItem>();

            if (data.Count > 0)
            {
                if (data.FirstOrDefault().ContainsKey("Error"))
                {
                    Logout();
                    return taskList;
                }
                    foreach (var task in data)
                    {
                        taskList.Add(task.ToObject<TaskItem>());
                    }
                return taskList;
            }

            return taskList;

        }

        #endregion

        #region Assignments
        /// <summary>
        /// View all Assignments made
        /// </summary>
        /// <returns></returns>
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
            var dict = GetCookies();
            var utl = new UserTaskList();
            //Get Users
            var userList = await GetUsers(dict);
            var userSl = new List<SelectListItem>();
            foreach (var user in userList)
            {
                userSl.Add(new SelectListItem(user.name,user.Id));
            }

            //Get Tasks
            var taskList = await GetTasks(dict);
            var taskSl = new List<SelectListItem>();
            foreach (var task in taskList)
            {
                taskSl.Add(new SelectListItem(task.Title, task.Id));
            }
            //Get Times
            var timeList = GetTimes();

            return View("AssignTask", new UserTaskList(){Tasks = taskSl,Users = userSl, Times = timeList} );
        }
        /// <summary>
        /// Assigns a task to a user and sends out a notification
        /// </summary>
        /// <param name="userTaskList"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AssignTask(UserTaskList userTaskList)
        {
            var something = userTaskList;
            userTaskList.UserTask.AssignedDateTime = DateTime.Now;
            userTaskList.UserTask.Complete = false;
            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(userTaskList.UserTask);
            var data = await DataController.AddData(dict, Enums.DataType.UserTasks, jData);

            //Send out notification
            await SendNotification(userTaskList, dict);

            return RedirectToAction("ViewHistory");
        }
        /// <summary>
        /// Resends a given task
        /// </summary>
        /// <param name="id">the specific usertask id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ResendTask(string id)
        {
            //auth
            var dict = GetCookies();

            //get the usertask data
            var userTask = await DataController.GetData(dict, Enums.DataType.UserTasks, "/" + id);
            var userTaskList = new UserTaskList()
            {
                UserTask = userTask.FirstOrDefault().ToObject<Usertask>()
            };
            userTaskList.UserTask.SendTime = DateTime.Now;
            var jData = JsonConvert.SerializeObject(userTaskList.UserTask);
            //update the sent time
            await DataController.AddData(dict, Enums.DataType.UserTasks, jData, true);
            //send the notification
            await SendNotification(userTaskList, dict);
            
            return RedirectToAction("ViewHistory");
        }

        #endregion

        #region Helper functions
        public IActionResult Issue()
        {
            //Should be a generic-ish error page.
            //Admin user exists
            //You have been logged out
            //connection timeout
            return View();
        }

        /// <summary>
        /// Sends the notification
        /// </summary>
        /// <param name="userTaskList"></param>
        /// <param name="dict"></param>
        /// <returns>nothing</returns>
        public async Task SendNotification(UserTaskList userTaskList, Dictionary<string,string> dict)
        {
            //Test data, to be replaced by owner/admin data
            var tester = new SimpleUser() { name = "testing", email = AppSettings.AppSetting["testEmail"]};

            //TODO get admin user
            //var adminUser = await DataController.GetData(dict, Enums.DataType.AdminUser, "/" + dict["uid"]);
            //var adminUserObj = adminUser.First().ToObject<User>();
            //var adminContact = new SimpleUser() { name = adminUserObj.name, email = adminUserObj.email };

            //get contact info
            var user = await DataController.GetData(dict, Enums.DataType.Users, "/" + userTaskList.UserTask.UserIdAssigned);
            var userObj = user.First().ToObject<User>();
            //get task
            var task = await DataController.GetData(dict, Enums.DataType.Tasks, "/" + userTaskList.UserTask.TaskId);
            var taskObj = task.First().ToObject<TaskItem>();
            //Make message
            //TODO create a template for htmlcontent
            var msg = new MessageData()
            {
                MessageId = Guid.NewGuid().ToString(),
                tags = new string[] { taskObj.Title },
                sender = tester,
                to = new SimpleUser[] { new SimpleUser() { name = userObj.name, email = userObj.email } },
                htmlContent = taskObj.TaskDetails,
                textContent = taskObj.TaskDetails,
                subject = taskObj.Title,
                replyTo = tester,
                SendTime = DateTime.Now
            };
            
            //Immediately send message
            BackgroundJob.Enqueue(() => mc.SendMessage(msg, userTaskList.UserTask.ContactMethod));

            //use encryption?
        }

        /// <summary>
        /// Gets a list of hours 0-23 from drop down menu
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetTimes()
        {
            var times = new List<SelectListItem>();
            for (int i = 0; i < 24; i++)
            {
                times.Add(new SelectListItem(
                    i+":00",
                    new DateTime(2000, 1, 1, i, 0, 0).ToShortTimeString()
                    ));
            }

            return times;
        }

        /// <summary>
        /// Logs out the users
        /// </summary>
        /// <returns></returns>
        public IActionResult Logout()
        {
            Remove("uid");
            Remove("token");
            return View("Index");
        }

        //Not used right now, implement?
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion

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

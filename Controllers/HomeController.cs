using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
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
using Cronos;
using Microsoft.Extensions.Caching.Memory;

namespace HeyDo.Controllers
{
    public class HomeController : Controller
    {
        private IMemoryCache _cache;

        public HomeController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }
        private readonly MessageController mc = new MessageController();

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

        [HttpGet]
        public async Task<IActionResult> HistoryDashboard()
        {
            var dict = GetCookies();
            //Get Users
            var userList = await GetUsers(dict);
            var userSl = UserIdToSelectList(userList);



            //Get Tasks
            var taskList = await GetTasks(dict);
            var taskSl = TaskIdToSelectList(taskList);

            return View(new UserTaskList { Tasks = taskSl, Users = userSl });
        }


        #endregion

        #region User Management

        [HttpGet]
        public IActionResult NewUser()
        {
            var dict = GetCookies();

            ViewData["ContactPreference"] = ContactEnumToList();

            return View();
        }

        //Add a new user
        [HttpPost]
        public async Task<IActionResult> NewUser(User user)
        {
            user.Id = Guid.NewGuid().ToString();
            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(user);
            await UpdateAndClearCache(dict, Enums.DataType.Users, Enums.UpdateType.Add, jData);

            return RedirectToAction("ViewUsers");
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string Id)
        {
            var dict = GetCookies();
            var data = await GetUsers(dict, Id);
            var selectList = ContactEnumToList();

            foreach (var item in selectList)
            {
                if (item.Value == data.FirstOrDefault().ContactPreference.ToString())
                {
                    item.Selected = true;
                    break;
                }
            }

            ViewData["ContactPreference"] = selectList;

            return View(data.FirstOrDefault());

        }

        [HttpPost]
        public async Task<IActionResult> EditUser(User user)
        {
            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(user);
            await UpdateAndClearCache(dict, Enums.DataType.Users, Enums.UpdateType.Edit, jData);
            return RedirectToAction("ViewUsers");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            var dict = GetCookies();
            await UpdateAndClearCache(dict, Enums.DataType.Users, Enums.UpdateType.Delete, "/" + Id);
            return RedirectToAction("ViewUsers");
        }

        [HttpGet]
        public async Task<IActionResult> ViewUsers()
        {
            var dict = GetCookies();
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
        public async Task<List<User>> GetUsers(Dictionary<string, string> auth, string uid = null)
        {
            var data = await GetOrSetCachedData(auth, Enums.DataType.Users, uid);

            var userList = new List<User>();
            if (data?.Count > 0 && data != null)
            {
                if (data.FirstOrDefault().ContainsKey("Error"))
                {
                    Logout();

                    return userList;
                }


                foreach (var user in data)
                {
                    userList.Add(user.ToObject<User>());
                }

                return userList;


            }

            return userList;
        }
        #endregion

        #region Admin User
        [HttpGet]
        public async Task<IActionResult> AddAdmin()
        {
            var dict = GetCookies();

            if (await GetAdmin(dict) == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Dashboard");
            }
        }

        //Add a new user
        [HttpPost]
        public async Task<IActionResult> AddAdmin(AdminUser user)
        {
            user.Id = Guid.NewGuid().ToString();
            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(user);
            await UpdateAndClearCache(dict, Enums.DataType.AdminUser, Enums.UpdateType.Add, jData);

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> EditAdmin()
        {
            var dict = GetCookies();
            
            var data = await GetAdmin(dict);
            //nothing returned when attempting to edit. Can only mean that admin user can not be found
            if (data == null)
            {
                return RedirectToAction("AddAdmin");
            }

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> EditAdmin(AdminUser user)
        {
            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(user);
            await UpdateAndClearCache(dict, Enums.DataType.AdminUser, Enums.UpdateType.Edit, jData);
            return RedirectToAction("Dashboard");
        }

        public async Task<AdminUser> GetAdmin(Dictionary<string, string> auth, string uid = null)
        {
            var data = await GetOrSetCachedData(auth, Enums.DataType.AdminUser);
            if (data == null)
            {
                Logout();
            }
            if (data.Count == 0)
            {
                return null;
            }
            var admin = data.FirstOrDefault().ToObject<AdminUser>();

            return admin;
        }

        #endregion

        #region Task Management
        [HttpGet]
        public async Task<IActionResult> NewTask()
        {
            var dict = GetCookies();

            ViewData["UserIdList"] = UserIdToSelectList(await GetUsers(dict));

            return View();
        }

        //Add a new task
        [HttpPost]
        public async Task<IActionResult> NewTask(TaskItem task)
        {
            task.Id = Guid.NewGuid().ToString();
            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(task);
            await UpdateAndClearCache(dict, Enums.DataType.Tasks, Enums.UpdateType.Add, jData);

            return RedirectToAction("ViewTasks");
        }

        [HttpGet]
        public async Task<IActionResult> EditTask(string Id)
        {
            var dict = GetCookies();

            var task = await GetTasks(dict, Id);

            var selectList = UserIdToSelectList(await GetUsers(dict));

            foreach (var item in selectList)
            {
                if (item.Value == task.FirstOrDefault().UserId.ToString())
                {
                    item.Selected = true;
                    break;
                }
            }

            ViewData["UserIdList"] = selectList;

            return View(task.FirstOrDefault());

        }

        [HttpPost]
        public async Task<IActionResult> EditTask(TaskItem task)
        {
            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(task);
            await UpdateAndClearCache(dict, Enums.DataType.Tasks, Enums.UpdateType.Edit, jData);

            return RedirectToAction("ViewTasks");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteTask(string Id)
        {
            var dict = GetCookies();
            await UpdateAndClearCache(dict, Enums.DataType.Tasks, Enums.UpdateType.Delete, Id);

            return RedirectToAction("ViewTasks");
        }

        //View all tasks
        public async Task<IActionResult> ViewTasks()
        {
            var dict = GetCookies();
            //Real life
            var taskList = await GetTasks(dict);

            var users = await GetUsers(dict);

            foreach (var t in taskList)
            {
                if (t.UserId != null)
                {
                    var user = users.Find(u => u.Id == t.UserId);
                    if (user != null)
                    {
                        t.UserId = user.name;
                    }
                    else
                    {
                        t.UserId = "";
                    }
                }
            }

            return View(taskList);

            //Test data
            //return View(TestData.TestTasks);
        }

        public async Task<List<TaskItem>> GetTasks(Dictionary<string, string> auth, string id = null)
        {

            var data = await GetOrSetCachedData(auth, Enums.DataType.Tasks, id);

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
        public async Task<IActionResult> ViewHistory(UserTaskList userTaskList = null)
        {
            var dict = GetCookies();

            var data = await GetOrSetCachedData(dict, Enums.DataType.UserTasks);
            var users = await GetUsers(dict);
            var tasks = await GetTasks(dict);

            var taskList = new List<Usertask>();
            if (data.Count > 0)
            {
                if (data.FirstOrDefault().ContainsKey("Error"))
                {
                    return RedirectToAction("Logout");
                }

                if (userTaskList.UserTask != null)
                {
                    foreach (var task in data)
                    {
                        if (task["UserIdAssigned"].ToString() == userTaskList.UserTask.UserIdAssigned ||
                            task["TaskId"].ToString() == userTaskList.UserTask.TaskId)
                        {
                            taskList.Add(task.ToObject<Usertask>());
                        }

                    }
                }
                else
                {
                    foreach (var task in data)
                    {
                        taskList.Add(task.ToObject<Usertask>());
                    }
                }

                foreach (var t in taskList)
                {
                    var user = users.Find(u => u.Id == t.UserIdAssigned);
                    var taskName = tasks.Find(tn => tn.Id == t.TaskId);
                    if (user != null)
                    {
                        t.UserIdAssigned = user.name;
                    }
                    if (taskName != null)
                    {
                        t.TaskId = taskName.Title;
                    }
                }
                return View(taskList);

            }

            return View(taskList);

        }

        [HttpGet]
        public async Task<IActionResult> AssignTask()
        {
            var dict = GetCookies();
            //Get Users
            var userList = await GetUsers(dict);
            var userSl = UserIdToSelectList(userList);

            //Get Tasks
            var taskList = await GetTasks(dict);
            var taskSl = TaskIdToSelectList(taskList);

            //Get Times
            var timeList = GetTimes();

            ViewData["ContactPreference"] = ContactEnumToList();

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
            userTaskList.UserTask.Id = Guid.NewGuid().ToString();
            userTaskList.UserTask.AssignedDateTime = DateTime.Now;
            userTaskList.UserTask.Complete = false;
            if (userTaskList.UserTask.SendNow)
            {
                userTaskList.UserTask.SendTime = DateTime.Now;
            }

            if (userTaskList.UserTask.SendTime < DateTime.Now && !userTaskList.UserTask.SendNow)
            {
                userTaskList.UserTask.SendTime = userTaskList.UserTask.SendTime.AddDays(1);
            }

            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(userTaskList.UserTask);

            await UpdateAndClearCache(dict, Enums.DataType.UserTasks,Enums.UpdateType.Add, jData);

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
            var userTask = await GetOrSetCachedData(dict, Enums.DataType.UserTasks, id);
            var userTaskList = new UserTaskList()
            {
                UserTask = userTask.FirstOrDefault().ToObject<Usertask>()
            };
            userTaskList.UserTask.SendTime = DateTime.Now;
            var jData = JsonConvert.SerializeObject(userTaskList.UserTask);
            //update the sent time
            await UpdateAndClearCache(dict, Enums.DataType.UserTasks,Enums.UpdateType.Edit, jData);
            
            //send the notification now, but not updating the task
            userTaskList.UserTask.SendNow = true;

            await SendNotification(userTaskList, dict);
            
            return RedirectToAction("ViewHistory");
        }

        [HttpGet]
        public async Task<IActionResult> SchedTask()
        {
            var dict = GetCookies();
            //Get Users
            var userList = await GetUsers(dict);
            var userSl = UserIdToSelectList(userList);

            //Get Tasks
            var taskList = await GetTasks(dict);
            var taskSl = TaskIdToSelectList(taskList);

            ViewData["ContactPreference"] = ContactEnumToList();
            ViewData["Frequency"] = FrequencyEnumToList();
            ViewData["DayFrequency"] = DayFrequencyEnumToList();
            ViewData["Days"] = GetDays();

            //TODO SEND THIS DATA TO THE VIEW
            return View();
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

        private async Task<List<JObject>> GetOrSetCachedData(Dictionary<string, string> auth, Enums.DataType dataType, string id = null)
        {
            var authed = auth["uid"] == await AuthController.Google(auth["token"]);
            if (!authed)
            {
                RedirectToAction("Logout");
            }
            var data = new List<JObject>();
            var uData = new List<JObject>();

            var isIt = _cache.TryGetValue(auth["uid"] + dataType, out data);

            if (!isIt && authed)
            {
                // Key not in cache, so get data.
                data = await DataController.GetData(auth, dataType);
                if (data.Count>0)
                { 
                    // Save data in cache if no error
                    _cache.Set(auth["uid"] + dataType, data);
                }
            }

            if (id != null && data.Count>0)
            {
                var task = data.Find(u => u["Id"].ToString() == id);
                uData.Add(task);
                return uData;
            }

            return data;
        }

        private async Task UpdateAndClearCache(Dictionary<string, string> auth, Enums.DataType dataType, Enums.UpdateType updateType, string jData=null)
        {
            _cache.Remove(auth["uid"] + dataType);

            switch (updateType)
            {
                case Enums.UpdateType.Add:
                    await DataController.AddData(auth, dataType, jData, false);                    
                    break;
                case Enums.UpdateType.Edit:
                    await DataController.AddData(auth, dataType, jData, true);
                    break;
                case Enums.UpdateType.Delete:
                    await DataController.DeleteData(auth, dataType, "/" + jData);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Sends the notification
        /// </summary>
        /// <param name="userTaskList"></param>
        /// <param name="dict"></param>
        /// <returns>nothing</returns>
        public async Task SendNotification(UserTaskList userTaskList, Dictionary<string,string> dict, TaskSchedule taskSchedule = null)
        {
            //get admin info
            var adminUserObj = await GetAdmin(dict);
            var adminContact = new SimpleUser() { name = adminUserObj.name, email = adminUserObj.ReplyToEmail };

            //get contact info
            var user = await GetOrSetCachedData(dict, Enums.DataType.Users, userTaskList.UserTask.UserIdAssigned);
            var userObj = user.First().ToObject<User>();
            //get task
            var task = await GetOrSetCachedData(dict, Enums.DataType.Tasks, userTaskList.UserTask.TaskId);
            var taskObj = task.First().ToObject<TaskItem>();

            //Make message
            //TODO create a template for htmlcontent
            var msg = new MessageData()
            {
                MessageId = Guid.NewGuid().ToString(),
                tags = new string[] { taskObj.Title },
                sender = adminContact,
                to = new SimpleUser[] { new SimpleUser() { name = userObj.name, email = userTaskList.UserTask.ContactMethod==Enums.ContactType.Email ? userObj.email : userObj.Phone} },
                htmlContent = taskObj.TaskDetails,
                textContent = taskObj.TaskDetails,
                subject = taskObj.Title,
                replyTo = adminContact,
                SendTime = taskSchedule == null ? userTaskList.UserTask.SendTime : taskSchedule.Time
            };
            if (taskSchedule == null)
            {
                //Immediately send message
                if (userTaskList.UserTask.SendNow)
                {
                    var single = BackgroundJob.Enqueue(() => mc.SendMessage(msg, userTaskList.UserTask.ContactMethod));
                }
                //wait until you say so
                else
                {
                    var future = BackgroundJob.Schedule(() => mc.SendMessage(msg, userTaskList.UserTask.ContactMethod), msg.SendTime);
                }
           
            }
            else
            {
                //TODO finish this, figure out logic
                switch (taskSchedule.Frequency)
                {
                    case Enums.Frequency.Daily:
                        RecurringJob.AddOrUpdate(() => mc.SendMessage(msg, userTaskList.UserTask.ContactMethod), Cron.Daily);
                        break;
                    case Enums.Frequency.Weekly:
                        break;
                    case Enums.Frequency.BiWeekly:
                        break;
                    case Enums.Frequency.Monthly:
                        break;
                    case Enums.Frequency.BiMonthly:
                        break;
                    default:
                        break;
                }
                
            }

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

        public List<SelectListItem> GetDays()
        {
            var times = new List<SelectListItem>();
            for (int i = 1; i < 32; i++)
            {
                times.Add(new SelectListItem(
                    i.ToString(),
                    i.ToString()
                ));
            }

            return times;
        }
        public List<SelectListItem> TaskIdToSelectList(List<TaskItem> tasks)
        {
            var taskList = new List<SelectListItem>();
            taskList.Add(new SelectListItem("Please select a Task", ""));

            foreach (var t in tasks)
            {
                taskList.Add(new SelectListItem(t.Title, t.Id));
            }

            return taskList;
        }

        public List<SelectListItem> UserIdToSelectList(List<User> users)
        {
            var userIdList = new List<SelectListItem>();
            userIdList.Add(new SelectListItem("Please select a User", ""));
            foreach (var u in users )
            {
                userIdList.Add(new SelectListItem(u.name,u.Id));
            }

            return userIdList;
        }

        public List<SelectListItem> ContactEnumToList()
        {
            var contactList = new List<SelectListItem>();

            foreach (var ct in Enum.GetValues(typeof(Enums.ContactType)))
            {
                contactList.Add(new SelectListItem(ct.ToString(), ct.ToString()));
            }

            return contactList;
        }

        public List<SelectListItem> FrequencyEnumToList()
        {
            var frequencyList = new List<SelectListItem>();

            foreach (var ct in Enum.GetValues(typeof(Enums.Frequency)))
            {
                frequencyList.Add(new SelectListItem(ct.ToString(), ct.ToString()));
            }

            return frequencyList;
        }

        public List<SelectListItem> DayFrequencyEnumToList()
        {
            var dayFrequencyList = new List<SelectListItem>();

            foreach (var ct in Enum.GetValues(typeof(Enums.DayFrequency)))
            {
                dayFrequencyList.Add(new SelectListItem(ct.ToString(), ct.ToString()));
            }

            return dayFrequencyList;
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
                if (dict["uid"] == null && dict["token"] == null )
                {
                    Set("token", b, 10);
                    Set("uid", a, 10);
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
                {"token", Get("token") }                
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

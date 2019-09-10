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
using Microsoft.Extensions.Localization.Internal;

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
            ViewData["jsSettings"] = AppSettings.AppSetting["jsSettings"];
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

        #region Upcoming Tasks
        //View upcoming usertasks
        public async Task<IActionResult> ViewUpcomingTasks()
        {
            var dict = GetCookies();
            var taskList = new List<Usertask>();
            //passing a usertasklist to filter if requested
            taskList = await GetUserTasks(dict);
            //taskList.Sort((x, y) => DateTime.Compare(y.SendTime, x.SendTime));
            //Only show future ones
            taskList = taskList.OrderBy(x => x.SendTime).Where(x => x.SendTime > DateTime.Now).ToList();
                       
            if (taskList.Count == 0)
            {
                return RedirectToAction("Dashboard");
            }
            //maybe go to own view, prob not necessary
            return View("ViewHistory",taskList);
        }
        //Be able to delete the hangfire job
        public async Task<IActionResult> DeleteUpcomingMessage(string id)
        {
            var dict = GetCookies();
            //Get the usertask data
            var ut = await GetUserTasks(dict, id);
            //Delete the usertask, bc it wouldn't be sent
            await UpdateAndClearCache(dict, Enums.DataType.UserTasks, Enums.UpdateType.Delete, id);
            //delete from hangfire scheduler
            MessageScheduler.DeleteMessage(ut.First().MessageId);

            return RedirectToAction("ViewUpcomingTasks");
        }
        #endregion

        #region Schedule Management
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
            ViewData["DayOfWeek"] = DayOfWeekToList();

            return View(new UserTaskSchedule() { UserTaskList = new UserTaskList() { Tasks = taskSl, Times = GetTimes(), Users = userSl } });
        }

        [HttpPost]
        public async Task<IActionResult> SchedTask(UserTaskSchedule userTaskSchedule)
        {
            var dict = GetCookies();
            var ut = userTaskSchedule.UserTaskList.UserTask;
            var ts = userTaskSchedule.TaskSchedule;

            ut.AssignedDateTime = DateTime.Now;
            ut.Id = Guid.NewGuid().ToString();
            ut.Complete = false;

            ts.Time = ut.SendTime;

            var jData = JsonConvert.SerializeObject(ut);
            //add the usertask
            await UpdateAndClearCache(dict, Enums.DataType.UserTasks, Enums.UpdateType.Add, jData);

            ts.Id = Guid.NewGuid().ToString();
            ts.UserTaskId = ut.Id;

            var messageId = await ScheduleNotification(ut, dict, ts);
            //TODO update usertask message id this is the recurring job, not the individual one
            ut.MessageId = messageId;
            var j2Data = JsonConvert.SerializeObject(ut);
            await UpdateAndClearCache(dict, Enums.DataType.UserTasks, Enums.UpdateType.Edit, j2Data);
            
            //add the task schedule
            jData = JsonConvert.SerializeObject(ts);
            await UpdateAndClearCache(dict, Enums.DataType.TaskSchedule, Enums.UpdateType.Add, jData);


            return RedirectToAction("ViewSched");
        }

        [HttpGet]
        public async Task<IActionResult> EditSched(string id)
        {
            var dict = GetCookies();
           
            var uts = await GetOrSetCachedData(dict, Enums.DataType.TaskSchedule, id);
            //could do empty check, but this is editing one, so we know it exists
            var taskSchedule = uts.FirstOrDefault().ToObject<TaskSchedule>();

            var ut = await GetUserTasks(dict, taskSchedule.UserTaskId);
            var usrtask = ut.FirstOrDefault();
            var userTaskSchedule = new UserTaskSchedule()
            {
                TaskSchedule = taskSchedule,
                UserTaskList = new UserTaskList() {UserTask = usrtask}
            };

            string[] days = Enum.GetNames(typeof(DayOfWeek));


            ViewData["ContactPreference"] = ContactEnumToList(usrtask.ContactMethod.ToString());
            ViewData["Frequency"] = FrequencyEnumToList(taskSchedule.Frequency.ToString());
            ViewData["DayFrequency"] = DayFrequencyEnumToList(taskSchedule.DayFrequency.ToString());
            ViewData["Days"] = GetDays(taskSchedule.DayOfMonth.ToString());
            ViewData["DayOfWeek"] = DayOfWeekToList(days);

            return View(userTaskSchedule);
        }

        [HttpPost]
        public async Task<IActionResult> EditSched(UserTaskSchedule userTaskSchedule)
        {
            var dict = GetCookies();
            var jData = JsonConvert.SerializeObject(userTaskSchedule.TaskSchedule);
            //I think we need to delete it then add a new one

            await UpdateAndClearCache(dict, Enums.DataType.TaskSchedule, Enums.UpdateType.Edit, jData);
            //schedule new one
            return RedirectToAction("ViewSched");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteSched(string id)
        {
            var dict = GetCookies();
            RecurringJob.RemoveIfExists(id);
            //delete from db
            await UpdateAndClearCache(dict, Enums.DataType.TaskSchedule, Enums.UpdateType.Delete, id);
            //Remove from hangfire
            return RedirectToAction("ViewSched");
        }
        public async Task<IActionResult> ViewSched(UserTaskSchedule userTaskSchedule = null)
        {
            var dict = GetCookies();
            var utsList = new List<UserTaskSchedule>();
            //Get Task Schedules
            var taskSchedules = await GetTaskSched(dict);
            //Get Usertasks
            var userTasks = await GetUserTasks(dict);
            //Populate list
            foreach (var ts in taskSchedules)
            {
                var ut = userTasks.Find(u => u.Id == ts.UserTaskId);
                utsList.Add(new UserTaskSchedule()
                {
                    TaskSchedule = ts,
                    UserTaskList = new UserTaskList() { UserTask = ut }
                });
            }
            //TODO Show all currently scheduled tasks, maybe have next scheduled date listed too and skip/send now

            return View(utsList);
        }

        [HttpGet]
        public async Task<IActionResult> GroupScheduleTask()
        {
            var dict = GetCookies();
            var usr = await GetUsers(dict);
            ViewData["Users"] = UserIdToSelectList(usr);
            var tsk = await GetTasks(dict);
            ViewData["Tasks"] = TaskIdToSelectList(tsk);

            return View("GroupScheduleTask");
        }

        [HttpPost]
        public async Task<IActionResult> GroupScheduleTask(GroupTaskSchedule groupTaskSchedule)
        {
            var dict = GetCookies();
            groupTaskSchedule.Id = Guid.NewGuid().ToString();
            groupTaskSchedule.GroupTaskRun = 0;
            //get users
            var userList = await GetUsers(dict);
            //get tasks
            var taskList = await GetTasks(dict);
            //create the grouptaskschedule
            var jData = JsonConvert.SerializeObject(groupTaskSchedule);
            await UpdateAndClearCache(dict, Enums.DataType.GroupSchedule, Enums.UpdateType.Add,jData);
            //create the usergroupschedule
            var kData = JsonConvert.SerializeObject(new { Id = groupTaskSchedule.Id, u = dict["uid"] });
            //use dataaccess instead of controller
            await DataAccess.ApiGoogle("PUT",kData,"/UserGroupSchedule/"+ groupTaskSchedule.Id,dict,true);
            //magic
            //would need to figure out how to randomly schedule a task
            //something like OnScheduledTask but NextRandomTask
            await MessageScheduler.OnScheduledEvent(groupTaskSchedule.Id);
            return RedirectToAction("ViewGroupScheduleTasks");
        }

        [HttpGet]
        public async Task<IActionResult> ViewGroupScheduleTasks()
        {
            var dict = GetCookies();
            var gts = await GetOrSetCachedData(dict, Enums.DataType.GroupSchedule);
            var groupTaskSchedules = gts.Select(g => g.ToObject<GroupTaskSchedule>()).ToList();
            
            return View(groupTaskSchedules);
        }

        public async Task<IActionResult> DeleteGroupSchedule(string id)
        {
            var dict = GetCookies();
            await UpdateAndClearCache(dict, Enums.DataType.GroupSchedule, Enums.UpdateType.Delete, id);

            return RedirectToAction("ViewGroupScheduleTasks");
        }

        public async Task<List<TaskSchedule>> GetTaskSched(Dictionary<string, string> auth, string uid = null)
        {
            var data = await GetOrSetCachedData(auth, Enums.DataType.TaskSchedule, uid);

            var tsList = new List<TaskSchedule>();
            if (data?.Count > 0 && data != null)
            {
                if (data.FirstOrDefault().ContainsKey("Error"))
                {
                    Logout();

                    return tsList;
                }

                foreach (var user in data)
                {
                    tsList.Add(user.ToObject<TaskSchedule>());
                }

                return tsList;
            }

            return tsList;
        }
        #endregion

        #region Assignments
        /// <summary>
        /// View all Assignments made
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ViewHistory(UserTaskList userTaskList)
        {
            var dict = GetCookies();
            var taskList = new List<Usertask>();
            //passing a usertasklist to filter if requested
            taskList = await GetUserTasks(dict);
            //taskList.Sort((x, y) => DateTime.Compare(y.SendTime, x.SendTime));
            //Only show historic ones
            taskList = taskList.OrderByDescending(x => x.SendTime).Where(x=>x.SendTime<=DateTime.Now).ToList();



            if (taskList.Count == 0)
            {
                return View("Dashboard");
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
            var messageId = await ScheduleNotification(userTaskList.UserTask, dict);
            // update usertask message id
            userTaskList.UserTask.MessageId = messageId;
            var j2Data = JsonConvert.SerializeObject(userTaskList.UserTask);
            await UpdateAndClearCache(dict, Enums.DataType.UserTasks, Enums.UpdateType.Edit, j2Data);

            if (userTaskList.UserTask.SendNow)
            {
                return RedirectToAction("ViewHistory");
            }
            return RedirectToAction("ViewUpcomingTasks");
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
            var userTask = await GetOrSetCachedData(dict,Enums.DataType.UserTasks, id);
         
            var ut =  userTask.FirstOrDefault().ToObject<Usertask>();

            //delete the already scheduled one
            if (ut.MessageId != null)
            {
                MessageScheduler.DeleteMessage(ut.MessageId);
                ut.MessageId = null;
            }             
            
            ut.SendTime = DateTime.Now;
            var jData = JsonConvert.SerializeObject(ut);
            //update the sent time
            await UpdateAndClearCache(dict, Enums.DataType.UserTasks,Enums.UpdateType.Edit, jData);
            //send the notification now, but not updating the task
            ut.SendNow = true;

            var messageId = await ScheduleNotification(ut, dict);
            //not necessary to update usertask message id since no action can be taken with it
            
            return RedirectToAction("ViewHistory");
        }    
        /// <summary>
        /// Gets
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<Usertask>> GetUserTasks(Dictionary<string, string> dict,  string id = null)
        {

            var data = await GetOrSetCachedData(dict, Enums.DataType.UserTasks,id);
            var users = await GetUsers(dict);
            var tasks = await GetTasks(dict);

            var taskList = new List<Usertask>();
            if (data.Count > 0)
            {
                if (data.FirstOrDefault().ContainsKey("Error"))
                {
                    return taskList;
                }

                foreach (var task in data)
                {
                    taskList.Add(task.ToObject<Usertask>());
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

                return taskList;

            }

            return taskList;
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
                Logout();
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
        //TODO put this in the message scheduler
        /// <summary>
        /// Schedules the notification to be sent 
        /// </summary>
        /// <param name="userTaskList"></param>
        /// <param name="dict"></param>
        /// <returns>nothing</returns>
        public async Task<string> ScheduleNotification(Usertask userTask, Dictionary<string,string> dict, TaskSchedule taskSchedule = null)
        {
            //get admin info
            var adminUserObj = await GetAdmin(dict);
            var adminContact = new SimpleUser() { name = adminUserObj.name, email = adminUserObj.ReplyToEmail };

            //get contact info
            var user = await GetOrSetCachedData(dict, Enums.DataType.Users, userTask.UserIdAssigned);
            var userObj = user.First().ToObject<User>();
            //get task
            var task = await GetOrSetCachedData(dict, Enums.DataType.Tasks, userTask.TaskId);
            var taskObj = task.First().ToObject<TaskItem>();

            //Make message
            return MessageScheduler.ScheduleMessage(adminContact, userObj, taskObj, userTask, taskSchedule);
            
        }


        #region SelectListFunctions
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

        public List<SelectListItem> GetDays(string match = null)
        {
            var times = new List<SelectListItem>();
            times.Add(new SelectListItem("n/a", "n/a"));
            for (int i = 1; i < 32; i++)
            {
                var m = i.ToString() == match;
                    times.Add(new SelectListItem(
                        i.ToString(),
                        i.ToString(), m
                    ));
                
            }

            return times;
        }
        public List<SelectListItem> TaskIdToSelectList(List<TaskItem> tasks, string match = null)
        {
            var taskList = new List<SelectListItem>();
            taskList.Add(new SelectListItem("Please select a Task", ""));

            foreach (var t in tasks)    
            {
                var m = t.Title == match;
                taskList.Add(new SelectListItem(t.Title, t.Id,m));
            }

            return taskList;
        }

        public List<SelectListItem> UserIdToSelectList(List<User> users, string match = null)
        {
            var userIdList = new List<SelectListItem>();
            userIdList.Add(new SelectListItem("Please select a User", ""));
            foreach (var u in users )
            {
                var m = u.name == match;
                userIdList.Add(new SelectListItem(u.name,u.Id,m));
            }

            return userIdList;
        }

        public List<SelectListItem> ContactEnumToList(string match=null)
        {
            var contactList = new List<SelectListItem>();

            foreach (var ct in Enum.GetValues(typeof(Enums.ContactType)))
            {
                var m = ct.ToString() == match;
                contactList.Add(new SelectListItem(ct.ToString(), ct.ToString(),m));
            }

            return contactList;
        }

        public List<SelectListItem> FrequencyEnumToList(string match = null)
        {
            var frequencyList = new List<SelectListItem>();

            foreach (var ct in Enum.GetValues(typeof(Enums.Frequency)))
            {
                var m = ct.ToString() == match;
                frequencyList.Add(new SelectListItem(ct.ToString(), ct.ToString(),m));
            }

            return frequencyList;
        }

        public List<SelectListItem> DayFrequencyEnumToList(string match = null)
        {
            var dayFrequencyList = new List<SelectListItem>(){new SelectListItem("n/a", "n/a")};
            foreach (var ct in Enum.GetValues(typeof(Enums.DayFrequency)))
            {
                var m = ct.ToString() == match;
                dayFrequencyList.Add(new SelectListItem(ct.ToString(), ct.ToString()));
            }

            return dayFrequencyList;
        }

        public List<SelectListItem> DayOfWeekToList(string[] match = null)
        {
            bool m = false;
            var dowList = new List<SelectListItem>();
            foreach (var ct in Enum.GetValues(typeof(DayOfWeek)))
            {
                if (match !=null)
                {
                    foreach (var d in match)
                    {
                        m = ct.ToString() == d;
                    }
                }
                   dowList.Add(new SelectListItem(ct.ToString(), ct.ToString(),m));
            }

            return dowList;
        }
        #endregion
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

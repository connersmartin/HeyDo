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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
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

        [HttpGet]
        public IActionResult Index()
        {
            var dict = new Dictionary<string, string>()
            {
                {"uid", Get("uid")},
                {"token", Get("token")}
            };

            if (Get("uid") != null && Get("token") != null)
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
            var dict = new Dictionary<string, string>()
            {
                {"uid", Get("uid")},
                {"token", Get("token")}
            };

            if (Get("uid") == null && Get("token") == null)
            {
                return View("Index");
            }
            else
            {

                return View();
            }
        }

        // This should give an at a glance thing for an admin
        //Current users/tasks, links to add/edit/remove things
        //view past tasks etc
        public async Task<bool> SetAuth(string auth, string uid)
        {
            var dict = new Dictionary<string, string>()
            {
                {"uid", Get("uid")},
                {"token", Get("token")}
            };

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
                if (Get("uid")==null && Get("token") == null)
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
        [HttpGet]
        public async Task<IActionResult> NewUser()
        {
            var dict = new Dictionary<string, string>()
            {
                {"uid", Get("uid")},
                {"token", Get("token")}
            };

            return View();
        }

        //Add a new user
        [HttpPost]
        public async Task<IActionResult> NewUser(User user)
        {

            var dict = new Dictionary<string, string>()
            {
                {"uid", Get("uid")},
                {"token", Get("token")}
            };
            var jData = JsonConvert.SerializeObject(user);
            var data = await DataController.AddData(dict, Enums.DataType.Users, jData);

            return RedirectToAction("ViewUsers");
        }
        [HttpGet]
        public async Task<IActionResult> NewTask()
        {
            var dict = new Dictionary<string, string>()
            {
                {"uid", Get("uid")},
                {"token", Get("token")}
            };
            return View();
        }

        //Add a new task
        [HttpPost]
        public async Task<IActionResult> NewTask(TaskItem task)
        {
            var dict = new Dictionary<string, string>()
            {
                {"uid", Get("uid")},
                {"token", Get("token")}
            };
            var jData = JsonConvert.SerializeObject(task);
            var data = await DataController.AddData(dict, Enums.DataType.Tasks, jData);

            return View("ViewTasks");
        }

        //View all users
        [HttpGet]
        public async Task<IActionResult> ViewUsers()
        {
            var dict = new Dictionary<string, string>()
            {
                {"uid", Get("uid")},
                {"token", Get("token")}
            };
            //Real life

            var data = await DataController.GetData(dict, Enums.DataType.Users);

            var userList = new List<User>();
            foreach (var user in data)
            {
                userList.Add(user.ToObject<User>());
            }

            return View(userList);
            
            //Test data
            //return View(TestData.TestUsers);
        }

        //View all tasks
        public async Task<IActionResult> ViewTasks()
        {
            var dict = new Dictionary<string, string>()
            {
                {"uid", Get("uid")},
                {"token", Get("token")}
            };
            //Real life

            var data = await DataController.GetData(dict, Enums.DataType.Tasks);

            var taskList = new List<TaskItem>();
            foreach (var task in data)
            {
                taskList.Add(task.ToObject<TaskItem>());
            }
            return View(taskList);
            
            //Test data
            //return View(TestData.TestTasks);
        }

        public static Dictionary<string,string> GetCookies(HttpContext http)
        {
            var rq = http.Request.Headers;

            var a = rq["uid"];
            var b = rq["token"];

            return new Dictionary<string, string>()
            {
                {"uid",a },
                {"token",b }
            };
        }

        public IActionResult Logout()
        {
            Remove("uid");
            Remove("token");
            return View("Index");
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

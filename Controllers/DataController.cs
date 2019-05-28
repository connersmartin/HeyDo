﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeyDo.Data;
using HeyDo.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeyDo.Controllers
{
    public class DataController : Controller
    {
        /// <summary>
        /// Adds data to Google
        /// </summary>
        /// <param name="uid">Google uid</param>
        /// <param name="auth">Google auth token</param>
        /// <param name="dataType">User, Task, Usertask</param>
        /// <returns></returns>
        public static async Task<string> AddData(string uid, string auth, Enums.DataType dataType, string jData)
        {
            //test data
            var data = TestData.Contests;

            var json = JsonConvert.SerializeObject(data);

            var obData = JsonConvert.DeserializeObject<JObject>(json);

            //var url = dataType + "/" + uid + "/" + obData["Id"];

            var url = dataType + "/" + uid + "/" + data.Id;

            var res = await DataAccess.ApiGoogle("PUT", json, url, auth);

            return res.ToString();
            
        }
        /// <summary>
        /// Returns lists of data
        /// </summary>
        /// <param name="uid">Google uid</param>
        /// <param name="auth">Google atuh token</param>
        /// <param name="dataType">User, Task, Usertask</param>
        /// <returns></returns>
        public static async Task<List<JObject>> GetData(string uid, string auth, Enums.DataType dataType)
        {
            var url = dataType + "/" + uid;

            var data = await DataAccess.ApiGoogle("GET", "", url, auth);

            return JsonConvert.DeserializeObject<List<JObject>>(data.ToString());

        }
    }
}
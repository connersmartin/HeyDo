using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeyDo.Data;
using HeyDo.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeyDo.Data
{
    public class DataService
    {
        /// <summary>
        /// Adds data to Google
        /// </summary>
        /// <param name="uid">Google uid</param>
        /// <param name="auth">Google auth token</param>
        /// <param name="dataType">User, Task, Usertask</param>
        /// <returns></returns>
        internal static async Task<string> AddData(Dictionary<string, string> auth, Enums.DataType dataType, string jData, bool update = false, bool admin = false)
        {
            var obData = JsonConvert.DeserializeObject<JObject>(jData);
            var url = dataType + "/" + auth["uid"] + "/" + obData["Id"];

            var action = update ? "PATCH" : "PUT";

            var res = await DataAccess.ApiGoogle(action, jData, url, auth,admin);

            return res.ToString();            
        }
        /// <summary>
        /// Returns lists of data
        /// </summary>
        /// <param name="uid">Google uid</param>
        /// <param name="auth">Google atuh token</param>
        /// <param name="dataType">User, Task, Usertask</param>
        /// <returns></returns>
        internal static async Task<List<JObject>> GetData(Dictionary<string, string> auth, Enums.DataType dataType, bool admin = false,
            string id = null)
        {
            var list = new List<JObject>();
            var url = dataType + "/" + auth["uid"] + id;

            var data = await DataAccess.ApiGoogle("GET", "", url, auth,admin);

            if (data != null)
            {
                if (data["Error"] != null)
                {
                    list.Add(data);
                    return list;
                }

                if (id != null)
                {
                    list.Add(data.ToObject<JObject>());
                }
                else
                {
                    foreach (var o in data)
                    {
                        list.Add(o.Value.ToObject<JObject>());
                    }
                }
                return list;
            }
            return list;
        }

        /// <summary>
        /// Deletes given data
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="auth"></param>
        /// <param name="dataType"></param>
        /// <returns>something maybe</returns>
        internal static async Task<string> DeleteData(Dictionary<string, string> auth, Enums.DataType dataType, string id, bool admin = false)
        {
            var url = dataType + "/" + auth["uid"] + "/"+id;
            var data = await DataAccess.ApiGoogle("DELETE", "", url, auth,admin);

            return data.ToString();
        }

    }
}
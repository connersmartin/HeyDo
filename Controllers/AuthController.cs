using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using HeyDo.Data;

namespace HeyDo.Controllers
{
    public class AuthController : Controller
    {
        private static string jsonCred = AppSettings.AppSetting["AuthJsonCred"];
        public static async Task<string> Google(string idToken)
        {
            //need to figure out how to log out of this though if even necessary
            try
            {
                var defaultApp = FirebaseApp.DefaultInstance;  

                if (defaultApp == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromJson(jsonCred),
                    });
                }

                var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                var uid = decoded.Uid;

                return uid;
            }
            catch (Exception e)
            {
                return e.Message;
            }       
        }

        public static void Clear()
        {
            //Unsure if this would ever need to be used
            FirebaseApp.DefaultInstance.Delete();
        }   
        
        public static async Task<Dictionary<string,string>> GetAdminAuth(string id)
        {
            var dict = new Dictionary<string, string>()
            {
                { "uid",null },
                {"token",null }
            };

            //Get the necessary data
            //get usergroupschedule via dataaccess
            var ugs = await DataAccess.ApiGoogle("GET", null, "/UserGroupSchedule/" + id, dict, true);
            //uid to auth to find the data properly
            dict["uid"] = ugs["u"].ToString();

            return dict;
        }
    }
}
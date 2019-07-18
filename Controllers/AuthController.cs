using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

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
    }
}
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
        public static async Task<string> Google(string idToken)
        {
            //TODO can theoretically check id token here and return uid
            //need to figure out how to log out of this though if even necessary
            try
            {
                var defaultApp = FirebaseApp.DefaultInstance;

                if (defaultApp == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromJson(""),
                    });
                }

                var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                var uid = decoded.Uid;

                return uid;
            }
            catch (Exception e)
            {
                throw;
            }
            
        }

        public static void Clear()
        {
            //Unsure if this would ever need to be used
            try
            {
                FirebaseApp.DefaultInstance.Delete();
            }
            catch (Exception e)
            {
                throw;
            }

        }
    }
}
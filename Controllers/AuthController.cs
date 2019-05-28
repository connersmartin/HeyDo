using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin;
using FirebaseAdmin.Auth;

namespace HeyDo.Controllers
{
    public class AuthController : Controller
    {
        public string Google(string idToken)
        {
            //TODO can theoretically check id token here and return uid

            return null;
        }
    }
}
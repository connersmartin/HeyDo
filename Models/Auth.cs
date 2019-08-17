using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    /// <summary>
    /// Holds the authorization data passed from the front end
    /// </summary>
    public class Auth
    {
        public string Uid { get; set; }
        public string Token { get; set; }
    }
}

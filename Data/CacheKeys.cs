using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Data
{
    public static class CacheKeys
    {
        public static string Users
        {
            get { return "_Users"; }
        }

        public static string Tasks
        {
            get { return "_Tasks"; }
        }

        public static string Usertasks
        {
            get { return "_Usertasks"; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    /// <summary>
    /// A user will be assigned tasks using their preferred contact preference unless overridden
    /// Group tasks should respect their ContactTime unless overridden
    /// </summary>
    public class User:SimpleUser
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string ContactTime { get; set; }
        public Enums.ContactType ContactPreference { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    /// <summary>
    /// This user contains the default "From" info in the notifications
    /// </summary>
    public class AdminUser: SimpleUser
    {
        public string Id { get; set; }
        public string SendName { get; set; }
        public string OrganizationName { get; set; }
        public string ReplyToEmail { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    
    public class Usertask
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string MessageId { get; set; }
        public bool Complete { get; set; }
        public DateTime SendTime { get; set; }
        public bool Passed { get; set; }
        public string UserIdAssigned { get; set; }
        public Enums.ContactType ContactMethod { get; set; }
        public string ActionTaken { get; set; }
        public DateTime AssignedDateTime { get; set; }
        public DateTime CompletedDateTime { get; set; }
        public bool SendNow { get; set; }
    }
}

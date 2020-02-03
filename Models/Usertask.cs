using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace HeyDo.Models
{
    /// <summary>
    /// This is what links users and tasks together
    /// This user task, once assigned can immediately send a notification
    /// or have one sent at a future date
    /// or have a recurring notification (taskschedule)
    /// or be part of a rotating notification (grouptaskschedule)
    /// </summary>
    public class Usertask
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string MessageId { get; set; }
        public bool Complete { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime SendTime { get; set; }
        public bool Passed { get; set; }
        public string UserIdAssigned { get; set; }
        public Enums.ContactType ContactMethod { get; set; }
        public string ActionTaken { get; set; }
        public DateTime AssignedDateTime { get; set; }
        public DateTime CompletedDateTime { get; set; }
        public bool SendNow { get; set; }
        public bool MessageSent { get; set; }
        public string UidToken { get; set; }
        public string LastTaskId { get; set; }
        public string GroupTaskId { get; set; }
        public int GroupTaskRun { get; set; }
        public bool LastScheduled { get; set; }
    }
}

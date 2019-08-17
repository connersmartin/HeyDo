using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeyDo.Models;

namespace HeyDo.Models
{
    /// <summary>
    /// This schedule type is for a specific usertaskid
    /// So a given task that is assigned to a user can be given a schedule
    /// </summary>
    public class TaskSchedule:Schedule
    {
        public string UserTaskId { get; set; }
        
    }
}

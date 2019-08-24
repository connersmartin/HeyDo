using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    /// <summary>
    /// This schedule is used for group schedules
    /// Where a group of users are assigned a group of tasks
    /// And those are run on a particular schedule
    /// </summary>
    public class GroupTaskSchedule:Schedule
    {
        public string[] Users { get; set; }
        public string[] Tasks { get; set; }
        public int GroupTaskRun { get; set; }
    }
}

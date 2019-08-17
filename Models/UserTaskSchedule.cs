using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    /// <summary>
    /// This links usertasklists and their taskschedule
    /// </summary>
    public class UserTaskSchedule
    {
        public UserTaskList UserTaskList { get; set; }
        public TaskSchedule TaskSchedule { get; set; }
    }
}

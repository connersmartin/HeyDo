using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    public class UserTaskSchedule
    {
        public UserTaskList UserTaskList { get; set; }
        public TaskSchedule TaskSchedule { get; set; }
    }
}

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
        public string Name { get; set; }

        public GroupTaskSchedule()
        {
            StartDate = DateTime.Today;
            //Time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour+1, 0, 0);
        }
    }
}

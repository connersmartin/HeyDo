using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    /// <summary>
    /// These are values used throughout the site that may have other options added
    /// </summary>
    public class Enums
    {
        public enum ContactType
        {
            Phone,
            Email
        }

        public enum DataType
        {
            AdminUser,
            Users,
            Tasks,
            TaskSchedule,
            UserTasks,
            GroupSchedule,
            UserGroupSchedule
        }

        public enum UpdateType
        {
            Add,
            Edit,
            Delete
        }

        public enum Frequency
        {
            Daily,
            Weekly,
            //BiWeekly,
            Monthly
            //,BiMonthly
        }

        public enum DayFrequency
        {
            First,
            //Second,
            //Third,
            //Fourth,
            Last
        }

    }
}

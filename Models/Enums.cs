using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
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
            UserTasks
        }

        public enum UpdateType
        {
            Add,
            Edit,
            Delete
        }


    }
}

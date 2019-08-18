using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeyDo.Models;
using HeyDo.Data;
using HeyDo.Controllers;

namespace HeyDo.Messaging
{
    public static class MessageScheduler
    {
        public static async Task OnScheduledEvent(string id)
        {
            //need an empty auth
            var dict = new Dictionary<string, string>()
            {
                { "uid",null },
                {"token",null }
            };

            //This could be used to have scheduled events get scheduled one by one
            //need to figure out how this could/should be done when thinking about authentication.
            //master user?

            //Get the necessary data
            //get usergroupschedule via dataaccess
            var ugs = await DataAccess.ApiGoogle("GET", null, "/UserGroupSchedule/" + id, dict, true);
            //uid to auth to find the data properly
            dict["uid"] = ugs["u"].ToString();
            //get grouptaskschedule
            var gts = await DataController.GetData(dict, Enums.DataType.GroupSchedule, true, "/"+id);
            //get tasks and users associated with this thing
            var users = await DataController.GetData(dict, Enums.DataType.Users, true);
            var tasks = await DataController.GetData(dict, Enums.DataType.Tasks, true);

            //get usertasks associated with the run
            var userTasks = await DataController.GetData(dict, Enums.DataType.UserTasks, true);

            //if the run has been completed delete the old schedule and create a new one?
            //ie if all users have cycled through the tasks
            //or all tasks have been sent

            //if not, schedule the proper usertasks

            //case when users=tasks try to randomize order then assign - CAKE?
            //users<>tasks
            //1.find out how many runs it will take
            //2.have a random assortment of the users assign the task or vice versa
            //3.on subsequent runs, remove those users/tasks from the available list
          
            //schedule messages

            //populate next messages
        }

        public static void PopulateNextRun()
        {
            //this could be used to populate the next run of a grouptaskschedule

        }

    }
}

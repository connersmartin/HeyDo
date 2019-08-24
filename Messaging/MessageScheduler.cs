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
            var groupUserList = new List<User>();
            var groupTaskList = new List<TaskItem>();

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
            var groupSchedule = gts.FirstOrDefault().ToObject<GroupTaskSchedule>();

            //get tasks and users associated with this thing
            var users = await DataController.GetData(dict, Enums.DataType.Users, true);
            var tasks = await DataController.GetData(dict, Enums.DataType.Tasks, true);
            //populate users and tasks for this group schedule
            foreach (var u in groupSchedule.Users)
            {
                groupUserList.Add(users.Find(us => us["Id"].ToString() == u).ToObject<User>());
            }
            foreach (var t in groupSchedule.Tasks)
            {
                groupTaskList.Add(tasks.Find(tk => tk["Id"].ToString() == t).ToObject<TaskItem>());
            }

            //populate usertasks

            //get usertasks associated with the run
            var userTasks = await DataController.GetData(dict, Enums.DataType.UserTasks, true);

            //create new usertasks
            var groupUsertask = CreateGroupUserTaskLists(groupSchedule);

            //add the contact preference to the usertasks
            foreach (var gut in groupUsertask)
            {
                foreach (var u in groupUserList)
                {
                    if (gut.UserIdAssigned==u.Id)
                    {
                        gut.ContactMethod = u.ContactPreference;
                    }
                }
            }
            //for debugging purposes
            foreach (var g in groupUsertask)
            {
                Console.WriteLine("UserId: {0} is going to be sent a {1} to do {2} for Group Run {3}", g.UserIdAssigned, g.ContactMethod, g.TaskId, g.GroupTaskRun);
            }

            //if count(usertasks for this grouptaskschedule) == whichever is greater users/tasks
            //if the run has been completed delete the old schedule and create a new one?
            //ie if all users have cycled through the tasks
            //or all tasks have been sent

            //if not, schedule the proper usertasks
            //find which users/tasks have been done 

            //OR just make it easy and schedule all the things and kee track of the last scheduled one

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
        public static List<Usertask> CreateGroupUserTaskLists(GroupTaskSchedule groupTaskSchedule, List<Usertask> userTasks = null)
        {
            var groupUserTasks = new List<Usertask>();

            var taskDict = new Dictionary<string, string>();

            var userList = new List<string>(groupTaskSchedule.Users);
            var taskList = new List<string>(groupTaskSchedule.Tasks);

            var longList = new List<string>();
            var shortList = new List<string>();

            //this section is to figure out how many times the loop below has to run
            //and which list is the longest
            decimal d = (decimal)userList.Count / (decimal)taskList.Count;
            decimal runs = d < 1 ? Math.Ceiling(1 / d) : Math.Ceiling(d);
            var flipped = d < 1;

            if (flipped)
            {
                longList = taskList;
                shortList = userList;
            }
            else
            {
                longList = userList;
                shortList = taskList;
            }

            //random for the loop
            var rand = new Random();
            //making a new list since we're going to remove items from the long list
            var lng = new List<string>(longList);
            //loop x times over short list
            for (var i = 0; i < runs; i++)
            {
                //iterate over the short list    
                foreach (var shrt in shortList)
                {
                    //break if no more left in the long list
                    if (lng.Count == 0) { break; }
                    //get a random number
                    var r = int.Parse(rand.Next(lng.Count).ToString());
                    //add to dictionary
                    taskDict.Add(lng[r], shrt);
                    
                    //remove from longer list
                    lng.RemoveAt(r);
                }
                foreach (var t in taskDict)
                {
                    groupUserTasks.Add(new Usertask()
                    {
                        Id = Guid.NewGuid().ToString(),
                        TaskId = flipped ? t.Key : t.Value,
                        SendTime = groupTaskSchedule.Time,
                        UserIdAssigned = flipped ? t.Value : t.Key,
                        AssignedDateTime = DateTime.Now,
                        SendNow = false,
                        GroupTaskId = groupTaskSchedule.Id,
                        GroupTaskRun = groupTaskSchedule.GroupTaskRun + i
                    });
                }
                taskDict.Clear();
            }

            

            return groupUserTasks;
        }


    }
}

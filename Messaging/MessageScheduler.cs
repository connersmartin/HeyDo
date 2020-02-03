using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeyDo.Models;
using HeyDo.Data;
using HeyDo.Controllers;
using Newtonsoft.Json;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace HeyDo.Messaging
{
    public class MessageScheduler
    {
        private ILogger _logger;
        private DataService _ds;

        public MessageScheduler(ILogger<DataService> logger, DataService ds)
        {
            _logger = logger;
            _ds = ds;
        }
        public async Task OnScheduledEvent(string id)
        {
            var groupUserList = new List<User>();
            var groupTaskList = new List<TaskItem>();

            //This could be used to have scheduled events get scheduled one by one
            var dict = await AuthController.GetAdminAuth(id);
            //get grouptaskschedule
            var gts = await _ds.GetData(dict, Enums.DataType.GroupSchedule, true, "/"+id);
            var groupSchedule = gts.FirstOrDefault().ToObject<GroupTaskSchedule>();
            //get admin user for contact info
            var admin = await _ds.GetData(dict,Enums.DataType.AdminUser);
            var adminUserObj = admin.FirstOrDefault().ToObject<AdminUser>();
            var adminContact = new SimpleUser() { name = adminUserObj.name, email = adminUserObj.ReplyToEmail };

            //get tasks and users associated with this thing
            var users = await _ds.GetData(dict, Enums.DataType.Users, true);
            var tasks = await _ds.GetData(dict, Enums.DataType.Tasks, true);
            //populate users and tasks for this group schedule
            foreach (var u in groupSchedule.Users)
            {
                groupUserList.Add(users.Find(us => us["Id"].ToString() == u).ToObject<User>());
            }
            foreach (var t in groupSchedule.Tasks)
            {
                groupTaskList.Add(tasks.Find(tk => tk["Id"].ToString() == t).ToObject<TaskItem>());
            }

            //create new list of usertasks
            var groupUsertask = CreateGroupUserTaskLists(groupSchedule);

            //add the contact preference to the usertasks
            foreach (var gut in groupUsertask)
            {
                gut.ContactMethod = groupUserList.Find(u => u.Id == gut.UserIdAssigned).ContactPreference;

                var st = gut.SendTime;

                if (!groupSchedule.TimeOverride)
                {
                    st=groupUserList.Find(u => u.Id == gut.UserIdAssigned).ContactTime;                    
                }
                //have send time start on the 
                gut.SendTime = new DateTime(gut.SendTime.Year, gut.SendTime.Month, gut.SendTime.Day, st.Hour, st.Minute, st.Second);

                //create the userTask in db
                var utData = JsonConvert.SerializeObject(gut);
                await _ds.AddData(dict, Enums.DataType.UserTasks, utData, false, true);

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
            var lastSet = false;
            //schedule messages
            var offsetOffset = groupUsertask.Min(g => g.GroupTaskRun);
            foreach (var gu in groupUsertask)
            {
                var userObj = groupUserList.Find(u => u.Id == gu.UserIdAssigned);
                var taskObj = groupTaskList.Find(t => t.Id == gu.TaskId);


                ScheduleMessage(adminContact, userObj, taskObj, gu, null, gu.GroupTaskRun-offsetOffset);
                if (gu.LastScheduled && !lastSet)
                {
                    lastSet = true;
                    //update grouptask run 
                    groupSchedule.GroupTaskRun = gu.GroupTaskRun;
                    var jsData = JsonConvert.SerializeObject(groupSchedule);
                    await _ds.AddData(dict, Enums.DataType.GroupSchedule,jsData,true,true);
                }
            }
        }

        public List<Usertask> CreateGroupUserTaskLists(GroupTaskSchedule groupTaskSchedule, List<Usertask> userTasks = null)
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
            //have initial time set to startdate and time
            var startDateTime = new DateTime(groupTaskSchedule.StartDate.Year,
                groupTaskSchedule.StartDate.Month,
                groupTaskSchedule.StartDate.Day,
                groupTaskSchedule.Time.Hour,
                groupTaskSchedule.Time.Minute,
                groupTaskSchedule.Time.Second);

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
                        SendTime = startDateTime.AddDays(groupTaskSchedule.GroupTaskRun + i - 1),
                        UserIdAssigned = flipped ? t.Value : t.Key,
                        AssignedDateTime = DateTime.Now,
                        SendNow = false,
                        GroupTaskId = groupTaskSchedule.Id,
                        GroupTaskRun = groupTaskSchedule.GroupTaskRun + i,
                        LastScheduled = lng.Count == 0
                    });
                }
                taskDict.Clear();
            }           

            return groupUserTasks;
        }

        public string ScheduleMessage(SimpleUser adminContact, User userObj, TaskItem taskObj, Usertask userTask, TaskSchedule taskSchedule, int offset=0)
        {
            //TODO create a template for htmlcontent
            //This will create an html email that looks nice
            var email = new EmailData(taskObj, userObj);

            var msg = new MessageData()
            {
                MessageId = Guid.NewGuid().ToString(),
                tags = new[] { taskObj.Title },
                sender = adminContact,
                to = new[] { new SimpleUser() { name = userObj.name, email = userTask.ContactMethod == Enums.ContactType.Email ? userObj.email : userObj.Phone } },
                htmlContent = email.BodyTemplate,
                textContent = taskObj.TaskDetails,
                subject = taskObj.Title,
                replyTo = adminContact,
                SendTime = taskSchedule?.Time ?? userTask.SendTime
            };
            if (taskSchedule == null)
            {
                //Immediately send message
                if (userTask.SendNow)
                {
                    return BackgroundJob.Enqueue(() => SendMessage(msg, userTask.ContactMethod,userTask));
                }
                //wait until you say so
                else 
                {
                    var offSet = msg.SendTime - DateTime.Now;
                    return BackgroundJob.Schedule(() => SendMessage(msg, userTask.ContactMethod,userTask), offSet);
                }
            }
            else
            {
                var freq = GetCronString(taskSchedule);
                //TODO finish this, figure out logic
                RecurringJob.AddOrUpdate(taskSchedule.Id, () => SendMessage(msg, userTask.ContactMethod,userTask), freq, TimeZoneInfo.Local);
                return taskSchedule.Id;

                //or do we schedule one at a time?
                //need to figure out how this will work with send once things
                //return BackgroundJob.Schedule(() => SendMessage(msg, userTask.ContactMethod, userTask), NextRun(taskSchedule, userTask, userObj));
                //ScheduleNextMessage
            }
        }

        public static DateTime NextRun(TaskSchedule ts, Usertask ut, User u)
        {
            //this could be used to populate the next run of a grouptaskschedule
            //Get last run time, will need to add to correct data later
            var lastRunTime = DateTime.Now;
            if (ut.SendNow)
            {
                lastRunTime =  ts.TimeOverride ? ts.Time : u.ContactTime;
            }
            else
            {
                lastRunTime = ut.SendTime;
            }

            //need to figure out the next x of when it should send based off of taskschedule or somehow from the last scheduled 

            switch (ts.Frequency)
            {
                case Enums.Frequency.Daily:
                    return lastRunTime.AddDays(1);                    
                case Enums.Frequency.Weekly:
                    return lastRunTime.AddDays(7);                    
                case Enums.Frequency.Monthly:
                    return lastRunTime.AddMonths(1);
                default:
                    break;
            }

            return DateTime.Now;
        }

        public static string GetCronString(TaskSchedule taskSchedule)
        {
            //Set Cron strings for common settings
            switch (taskSchedule.Frequency)
            {
                case Enums.Frequency.Daily:
                    return string.Format("{1} {0} * * 0-6", taskSchedule.Time.Hour.ToString(), taskSchedule.Time.Minute.ToString());
                case Enums.Frequency.Weekly:
                    var ds = taskSchedule.DayOfWeek.Select(s => s.ToString().ToUpper().Substring(0, 3));
                    return string.Format("{1} {0} * * {2}", taskSchedule.Time.Hour.ToString(), taskSchedule.Time.Minute.ToString(), string.Join(',', ds));
                case Enums.Frequency.Monthly:
                    return string.Format("{1} {0} {2} * *", taskSchedule.Time.Hour.ToString(), taskSchedule.Time.Minute.ToString(), taskSchedule.DayOfMonth);
                default:
                    return "";
            }
        }
        /// <summary>
        /// Sends a message to a user
        /// </summary>
        /// <param name="msg">Message information</param>
        /// <param name="cType">Contact type, Email or Phone</param>
        public async Task SendMessage(MessageData msg, Enums.ContactType cType, Usertask userTask)
        {
            //don't need to send a message while testing
            if (AppSettings.AppSetting["testmode"]=="true")
            {
                Console.WriteLine("Done");
            }
            else
            {
                if (userTask.SendTime < DateTime.Now.AddHours(23))
                {
                    switch (cType)
                    {
                        case Enums.ContactType.Email:
                            var emailSuccess = await EmailAgent.SendMail(msg);
                            break;
                        case Enums.ContactType.Phone:
                            var smsSuccess = SmsAgent.TwiSend(msg);
                            break;
                        default:
                            break;
                    }
                }
            }

            _logger.LogInformation("{0} sent for Usertask {1}", cType.ToString(), userTask.Id);
            //omitting this for now since it was causing issues
            /*
            if (userTask.LastScheduled)
            {
                //Clear all other lastScheduled flags in this group
                await CheckLastMessage(userTask);
                //increment the grouptask run
                _logger.LogInformation("Usertask {0} was run to repopulate GroupTaskId {1} for GroupTaskRun {2}",
                                        userTask.Id, userTask.GroupTaskId, userTask.GroupTaskRun);
                //Schedule the next event(s)
                await OnScheduledEvent(userTask.GroupTaskId);
                _logger.LogInformation("After repopulated");
            }
            */
        }

        public async Task CheckLastMessage(Usertask userTask)
        {
            //use admin auth since we've already gotten to this point
            var dict = await AuthController.GetAdminAuth(userTask.GroupTaskId);
            //get usertasks to update
            var uts = await _ds.GetData(dict, Enums.DataType.UserTasks, true);
            var userTasks = new List<Usertask>();
            foreach (var ut in uts)
            {
                userTasks.Add(ut.ToObject<Usertask>());
            }
            //iterate over userTasks to find the potentially matching ones
            foreach (var u in userTasks)
            {
                if (u.LastScheduled == true
                    && u.GroupTaskRun == userTask.GroupTaskRun
                    && u.GroupTaskId == userTask.GroupTaskId)

                {
                    //change lastscheduled to false and update it!
                    u.LastScheduled  = false;
                    var jData = JsonConvert.SerializeObject(u);
                    await _ds.AddData(dict, Enums.DataType.UserTasks, jData, true, true);
                }
            }
            //increment grouptaskrun
            var gts = await _ds.GetData(dict, Enums.DataType.GroupSchedule, true, "/" + userTask.GroupTaskId);
            var groupSchedule = gts.FirstOrDefault().ToObject<GroupTaskSchedule>();

            groupSchedule.GroupTaskRun = userTask.GroupTaskRun+1;
            var jsData = JsonConvert.SerializeObject(groupSchedule);
            await _ds.AddData(dict, Enums.DataType.GroupSchedule, jsData, true, true);
        }


        public void DeleteMessage(string id)
        {
            try
            {
                BackgroundJob.Delete(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


    }
}

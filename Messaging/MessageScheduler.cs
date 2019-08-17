using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Messaging
{
    public class MessageScheduler
    {
        public void OnScheduledEvent()
        {
            //This could be used to have scheduled events get scheduled one by one
            //need to figure out how this could/should be done when thinking about authentication.
            //master user?

            //schedule message
            //populate next messages
        }

        public void PopulateNextRun()
        {
            //this could be used to populate the next run of a grouptaskschedule

        }
    }
}

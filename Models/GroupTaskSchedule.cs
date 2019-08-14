using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    public class GroupTaskSchedule
    {
        public string Id { get; set; }
        public string[] Users { get; set; }
        public string[] Tasks { get; set; }
    }
}

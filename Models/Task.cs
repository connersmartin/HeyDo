﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    public class TaskItem
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string TaskDetails { get; set; }
    }
}

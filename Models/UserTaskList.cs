using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HeyDo.Models
{
    /// <summary>
    /// This may be obsolete/can be refactored out, 
    /// but this is used most places for creating usertasks
    /// </summary>
    public class UserTaskList
    {
        public Usertask UserTask { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
        public IEnumerable<SelectListItem> Tasks { get; set; }
        public IEnumerable<SelectListItem> Times { get; set; }        
    }

   
}

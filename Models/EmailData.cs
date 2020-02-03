using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    public class EmailData
    {
        public string BodyTemplate { get; set; }

        public EmailData(TaskItem taskObj, User userObj)
        {
            var body = new StringBuilder("<html><body>");
            body.Append("<h3>" + taskObj.Title + "</h3></br>");
            body.Append("<p>Hey " + userObj.name + ", you're tasked with:");
            body.Append(taskObj.TaskDetails+"</p>");
            body.Append("</body></html>");
            BodyTemplate = body.ToString();
        }
    }
}

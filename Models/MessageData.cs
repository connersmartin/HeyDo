using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    public class MessageData
    {
        public string MessageId { get; set; }
        public string[] tags { get; set; }
        public SimpleUser sender { get; set; }
        public SimpleUser[] to { get; set; }
        public string htmlContent { get; set; }
        public string textContent { get; set; }
        public string subject { get; set; }
        public SimpleUser replyTo { get; set; }
        public DateTime SendTime { get; set; }
    }
}

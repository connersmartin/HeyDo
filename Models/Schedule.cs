using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Models
{
    /// <summary>
    /// Base schedule information
    /// </summary>
    public class Schedule
    {
        public string Id { get; set; }
        public DateTime Time { get; set; }
        public DayOfWeek[] DayOfWeek { get; set; }
        public int DayOfMonth { get; set; }
        public Enums.Frequency Frequency { get; set; }
        public Enums.DayFrequency DayFrequency { get; set; }
        public bool Active { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CancelToken { get; set; }
        public bool TimeOverride { get; set; }
    }
}

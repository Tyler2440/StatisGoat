using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Events
{
    public class EventsRecord
    {
        public int MID { get; set; }
        public int TID { get; set; }
        public int PID { get; set; }
        public int Minute { get; set; }
        public string? Type { get; set; }
        public string? Detail { get; set; }
        public int? Assist { get; set; }
        public string? Comment { get; set; }
    }
}

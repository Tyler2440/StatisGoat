using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Events
{
    public class EventsInfoRecord
    {
        public int MID { get; set; }
        public int Minute { get; set; }
        public string? Type { get; set; }
        public string? Detail { get; set; }
        public string? Comment { get; set; }
        public int TeamID { get; set; }
        public string? TeamName { get; set; }
        public string? Badge { get; set; }
        public int PlayerID { get; set; }
        public string? First { get; set; }
        public string? Last { get; set; }
        public string? Nickname { get; set; }
        public string? Headshot { get; set; }
        public int AssistID { get; set; }
        public string? AssistFirst { get; set; }
        public string? AssistLast { get; set; }
        public string? Assistnickname { get; set; }
        public string? AssistHeadshot { get; set; }

    }
}

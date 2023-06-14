using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Chats
{
    public class ChatRecord
    {
        public int ChatID { get; set; }
        public int MatchID { get; set; }
        public int AccountID { get; set; } 
        public string? Message { get; set; }
        public DateTime? Timestamp { get; set; }
        public int? ThreadID { get; set; }
    }
}

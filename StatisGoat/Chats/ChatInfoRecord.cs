using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Chats
{
    public class ChatInfoRecord : ChatRecord
    {
        public string? First { get; set; }
        public string? Last { get; set; }
        public string? Email { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public bool Liked { get; set; }
        public bool Disliked { get; set; }
    }
}

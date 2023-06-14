using System;

namespace StatisGoat.Players
{
    public class PlayersRecord
    {
        public int ApiID { get; set; }
        public int TID { get; set; }
        public string? First { get; set; }
        public string? Last { get; set; }
        public string? Nickname { get; set; }
        public DateTime? DOB { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string? Nationality { get; set; }
        public string? Headshot { get; set; }

    }
}

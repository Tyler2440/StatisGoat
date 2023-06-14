using System;

namespace StatisGoat.Matches
{
    public class MatchesRecord
    {
        public int ApiID { get; set; }
        public int Home { get; set; }
        public int Away { get; set; }
        public string? Competition { get; set; }
        public DateTime DateTime { get; set; }
        public string? Status { get; set; }
        public string? Result { get; set; }
        public int Elapsed { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Player_Statistics
{
    public class Player_StatisticsInfoRecord : Player_StatisticsRecord
    {
        public int PID { get; set; }
        public string? First { get; set; }
        public string? Last { get; set; }
        public string? Nickname { get; set; }
        public string? Headshot { get; set; }
        public int TID { get; set; }
        public string? Teamname { get; set; }
        public int MID { get; set; }
        public int Opponentid { get; set; }
        public string? Opponentname { get; set; }
        public string? Competition { get; set; }
        public DateTime Datetime { get; set; }
        public string? Status { get; set; }
        public int Teamscored { get; set; }
        public int Teamconceded { get; set; }
        public string? Result { get; set; }
        public string? Position { get; set; }
        public string? Grid { get; set; }
        public double Pass_pct { get; set; }
    }
}

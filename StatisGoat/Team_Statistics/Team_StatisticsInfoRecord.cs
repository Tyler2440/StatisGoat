using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Team_Statistics
{
    public class Team_StatisticsInfoRecord : Team_StatisticsRecord
    {
        public string? Teamname { get; set; }
        public string? Competition { get; set; }
        public string? Result { get; set; }
        public DateTime Datetime { get; set; }
        public int Opponentid { get; set; }
        public string? Opponentname { get; set; }
        public int Scored { get; set; }
        public int Conceded { get; set; }
        public double Pass_pct { get; set; }
    }
}

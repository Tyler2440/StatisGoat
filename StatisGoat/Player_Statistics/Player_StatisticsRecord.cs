using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Player_Statistics
{
    public class Player_StatisticsRecord
    {
        public int LID { get; set; }
        public int Minutes { get; set; }
        public double Rating { get; set; }
        public bool Substitute { get; set; }
        public int Shots { get; set; }
        public int Shots_on_goal { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int Saves { get; set; }
        public int Conceded { get; set; }
        public int Passes { get; set; }
        public int Key_passes { get; set; }
        public int Passes_accurate { get; set; }
        public int Tackles { get; set; }
        public int Blocks { get; set; }
        public int Interceptions { get; set; }
        public int Duels { get; set; }
        public int Duels_won { get; set; }
        public int Dribbles { get; set; }
        public int Dribbles_won { get; set; }
        public int Dribbles_past { get; set; }
        public int Fouls_drawn { get; set; }
        public int Fouls_committed { get; set; }
        public int Yellow { get; set; }
        public int Red { get; set; }
        public int Penalties_won { get; set; }
        public int Penalties_conceded { get; set; }
        public int Penalties_scored { get; set; }
        public int Penalties_missed { get; set; }
        public int Penalties_saved { get; set; }
    }
}

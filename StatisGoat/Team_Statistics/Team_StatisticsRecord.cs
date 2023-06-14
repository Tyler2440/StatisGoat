using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Team_Statistics
{
    public class Team_StatisticsRecord
    {
        public int MID { get; set; }
        public int TID { get; set; }
        public int Shots { get; set; }
        public int Shots_on_goal { get; set; }
        public int Shots_off_goal { get; set; }
        public int Shots_inside { get; set; }
        public int Shots_outside { get; set; }
        public int Blocked { get; set; }
        public int Fouls { get; set; }
        public int Corners { get; set; }
        public int Offsides { get; set; }
        public int Possession { get; set; }
        public int Yellows { get; set; }
        public int Reds { get; set; }
        public int Saves { get; set; }
        public int Passes { get; set; }
        public int Passes_accurate { get; set; }

        public void SwitchStat(string stat, string value)
        {
            if (value is null) { value = "0"; }
            switch (stat)
            {
                case "Shots on Goal": Shots_on_goal = int.Parse(value); return;
                case "Shots off Goal": Shots_off_goal = int.Parse(value); return;
                case "Total Shots": Shots = int.Parse(value); return;
                case "Blocked Shots": Blocked = int.Parse(value); return;
                case "Shots insidebox": Shots_inside = int.Parse(value); return;
                case "Shots outsidebox": Shots_outside = int.Parse(value); return;
                case "Fouls": Fouls = int.Parse(value); return;
                case "Corner Kicks": Corners = int.Parse(value); return;
                case "Offsides": Offsides = int.Parse(value); return;
                case "Ball Possession": 
                    if (value.Length < 2) { Possession = 0; }
                    else { Possession = int.Parse(value.Substring(0, 2)); }
                    return;
                case "Yellow Cards": Yellows = int.Parse(value); return;
                case "Red Cards": Reds = int.Parse(value); return;
                case "Goalkeeper Saves": Saves = int.Parse(value); return;  
                case "Total passes": Passes = int.Parse(value); return;
                case "Passes accurate": Passes_accurate = int.Parse(value); return;
            }
        }
    }
}

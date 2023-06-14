using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Lineups
{
    public class LineupsInfoRecord
    {
        public int LineupId { get; set; }
        public int MatchID { get; set; }
        public int TeamID { get; set; }
        public string? TeamName { get; set; }
        public int PlayerID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Nickname { get; set; }
        public int? Number { get; set; }
        public DateTime DOB { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string? Nationality { get; set; }
        public string? Headshot { get; set; }
        public string? Position { get; set; }
        public string? Grid { get; set; }
    }
}

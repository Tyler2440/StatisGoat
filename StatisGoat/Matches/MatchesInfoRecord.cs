using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Matches
{
    public class MatchesInfoRecord
    {
        public int MatchId { get; set; }
        public string? Competition { get; set; }
        public DateTime DateTime { get; set; }
        public string? Venue { get; set; }
        public int HomeId { get; set; }
        public string? HomeName { get; set; }
        public string? HomeNation { get; set; }
        public string? HomeFormation { get; set; }
        public double? Homexg { get; set; }
        public double? Homepct { get; set; }
        public double? Drawpct { get; set; }
        public double? Awaypct { get; set; }
        public double? Awayxg { get; set; }
        public int AwayId { get; set; }
        public string? AwayName { get; set; }
        public string? AwayNation { get; set; }
        public string? AwayFormation { get; set; }
        public string? Status { get; set; }
        public string? Result { get; set; }
        public int Elapsed { get; set; }
        public string? HomeBadge { get; set; }
        public string? AwayBadge { get; set; }
    }
}

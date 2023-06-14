using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Players
{
    public class PlayersInfoRecord : PlayersRecord
    {
        public string? TeamName { get; set; }
        public string? TeamNation { get; set; }
        public string? Badge { get; set; }
    }
}

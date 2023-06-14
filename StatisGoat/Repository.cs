using StatisGoat.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat
{
    public class Repository
    {
        internal readonly IPostgresConnection postgres;
        internal readonly string Events, Formations, Lineups, Matches,
            Player_Statistics, Players, Team_Statistics, Teams, 
            xPlayer_Statistics, xTeam_Statistics, Forums, Accounts, Likes, Dislikes;
        internal string BaseRead, BaseWrite, OrderBy;

        public Repository(IPostgresConnection ipc)
        {
            postgres = ipc;
            Events = "events";
            Formations = "formations";
            Lineups = "lineups";
            Matches = "matches";
            Player_Statistics = "player_statistics";
            Players = "players";
            Team_Statistics = "team_statistics";
            Teams = "teams";
            xPlayer_Statistics = "xplayer_statistics";
            xTeam_Statistics = "xteam_statistics";
            Forums = "forums";
            Accounts = "accounts";
            Likes = "likes";
            Dislikes = "dislikes";
            BaseRead = "";
            BaseWrite = "";
            OrderBy = "";
        }
    }
}

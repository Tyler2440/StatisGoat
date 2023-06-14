using StatisGoat.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.xTeam_Statistics
{
    public class xTeam_StatisticsRepository : Repository, IxTeam_StatisticsRepository
    {
        private readonly string GroupBy; 
        public xTeam_StatisticsRepository(IPostgresConnection ipc) : base(ipc)
        {
            BaseRead = $"select {Teams}.apiid as tid, {Teams}.name as teamname, {Matches}.apiid as mid, " +
                $"case " +
                $"when {Teams}.apiid = {Matches}.home then {Matches}.away " +
                $"else {Matches}.home " +
                $"end opponentid, " +
                $"case " +
                $"when {Teams}.apiid = {Matches}.home then (select {Teams}.name from {Teams} where {Teams}.apiid = {Matches}.away) " +
                $"else (select {Teams}.name from {Teams} where {Teams}.apiid = {Matches}.home) " +
                $"end opponentname, " +
                $"competition, datetime, status, " +
                $"case " +
                $"when {Teams}.apiid = {Matches}.home then left(result, 1)::int " +
                $"else right(result, 1)::int " +
                $"end teamscored, " +
                $"case when {Teams}.apiid = {Matches}.home then right(result, 1)::int " +
                $"else left(result, 1)::int " +
                $"end teamconceded, " +
                $"case " +
                $"when {Teams}.apiid = {Matches}.home and left(result, 1)::int > right(result, 1)::int then \'W\' " +
                $"when {Teams}.apiid = {Matches}.home and left(result, 1)::int < right(result, 1)::int then \'L\' " +
                $"when {Teams}.apiid = {Matches}.away and right(result, 1)::int > left(result, 1)::int then \'W\' " +
                $"when {Teams}.apiid = {Matches}.away and right(result, 1)::int < left(result, 1)::int then \'L\' " +
                $"else \'D\' " +
                $"end result, " +
                $"avg(xrating) as xrating, avg(rating - xrating) as rating_perf, " +
                $"sum(xshots) as xshots, sum(shots - xshots) as shots_perf, " +
                $"sum(xgoals) as xgoals, sum(goals - xgoals) as goals_perf, " +
                $"sum(xassists) as xassists, sum(assists - xassists) as assists_perf, " +
                $"sum(xsaves) as xsaves, sum(saves - xsaves) as saves_perf, " +
                $"sum(xpasses) as xpasses, sum(passes - xpasses) as passes_perf, " +
                $"sum(xtackles) as xtackles, sum(tackles - xtackles) as tackles_perf, " +
                $"sum(xinterceptions) as xinterceptions, sum(interceptions - xinterceptions) as interceptions_perf, " +
                $"sum(xdribbles) as xdribbles, sum(dribbles - xdribbles) as dribbles_perf, " +
                $"sum(xfouls) as xfouls, sum(fouls_committed - xfouls) as fouls_perf, " +
                $"sum(xyellow) as xyellow, sum(yellow - xyellow) as yellow_perf, " +
                $"sum(xred) as xred, sum(red - xred) as red_perf " +
                $"from {Formations} join {Lineups} on {Formations}.fid = {Lineups}.fid " +
                $"left join {Player_Statistics} on {Lineups}.lid = {Player_Statistics}.lid " +
                $"left join {xPlayer_Statistics} on {Lineups}.lid = {xPlayer_Statistics}.lid " +
                $"join {Teams} on {Formations}.tid = {Teams}.apiid " +
                $"join {Matches} on {Formations}.mid = {Matches}.apiid " +
                $"join {Players} on {Lineups}.pid = {Players}.apiid " +
                $"where minutes > 0";
            OrderBy = $" order by datetime desc";
            GroupBy = $" group by ({Teams}.apiid, teamname, {Matches}.apiid, opponentid, opponentname, " +
                $"teamscored, teamconceded, result)";
        }

        public async Task<xTeam_StatisticsInfoRecord> GetByMID(int mid, int tid)
        {
            var xteam_stats = await postgres.ReadDataAsync<xTeam_StatisticsInfoRecord>(BaseRead +
                $" and {Matches}.apiid = :mid and {Teams}.apiid = :tid" + GroupBy, new { mid, tid });
            if (xteam_stats.Any()) { return xteam_stats.First(); }
            else { return new xTeam_StatisticsInfoRecord(); }
        }

        public async Task<IEnumerable<xTeam_StatisticsInfoRecord>> GetByTID(int tid, string? competition, int? limit, string? date)
        {
            string compfilter = competition is null ? "" : $" and competition=\'{competition}\'";
            string limitfilter = limit is null ? "" : $" limit {limit}";
            string datefilter = date is null ? "" : $" and {Matches}.datetime::date < \'{date}\'::date ";
            return await postgres.ReadDataAsync<xTeam_StatisticsInfoRecord>(BaseRead +
                $" and {Teams}.apiid = :tid" + compfilter + datefilter +
                GroupBy + OrderBy + limitfilter, new { tid });
        }
    }
}

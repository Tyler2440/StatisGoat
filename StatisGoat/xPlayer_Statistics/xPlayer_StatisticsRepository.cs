using StatisGoat.Player_Statistics;
using StatisGoat.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.xPlayer_Statistics
{
    public class xPlayer_StatisticsRepository : Repository, IxPlayer_StatisticsRepository
    {
        public xPlayer_StatisticsRepository(IPostgresConnection ipc) : base(ipc)
        {
            BaseRead = $"select {Players}.apiid as pid, first, last, nickname, headshot, " +
                $"{Teams}.apiid as tid, {Teams}.name as teamname, ids.apiid as mid, ids.datetime, " +
                $"case " +
                $"when {Teams}.apiid = ids.home then ids.away " +
                $"else ids.home " +
                $"end opponentid, " +
                $"case " +
                $"when {Teams}.apiid = ids.home then (select {Teams}.name from {Teams} where {Teams}.apiid = ids.away) " +
                $"else (select {Teams}.name from {Teams} where {Teams}.apiid = ids.home) " +
                $"end opponentname, " +
                $"competition, datetime, status, " +
                $"case " +
                $"when {Teams}.apiid = ids.home then left(result, 1)::int " +
                $"else right(result, 1)::int " +
                $"end teamscored, " +
                $"case " +
                $"when {Teams}.apiid = ids.home then right(result, 1)::int " +
                $"else left(result, 1)::int " +
                $"end teamconceded, " +
                $"case " +
                $"when {Teams}.apiid = ids.home and left(result, 1)::int > right(result, 1)::int then 'W' " +
                $"when {Teams}.apiid = ids.home and left(result, 1)::int < right(result, 1)::int then 'L' " +
                $"when {Teams}.apiid = ids.away and right(result, 1)::int > left(result, 1)::int then 'W' " +
                $"when {Teams}.apiid = ids.away and right(result, 1)::int < left(result, 1)::int then 'L' " +
                $"else 'D' " +
                $"end result, " +
                $"position, grid, " +
                $"xrating, rating - xrating as rating_perf, xshots, shots - xshots as shots_perf, " +
                $"xgoals, goals - xgoals as goals_perf, xassists, assists - xassists as assists_perf, " +
                $"xsaves, saves - xsaves as saves_perf, xpasses, passes - xpasses as passes_perf, " +
                $"xtackles, tackles - xtackles as tackles_perf, xinterceptions, interceptions - xinterceptions as interceptions_perf, " +
                $"xdribbles, dribbles - xdribbles as dribbles_perf, xfouls, fouls_committed - xfouls as fouls_perf, " +
                $"xyellow, yellow - xyellow as yellow_perf, xred, red - xred as red_perf " +
                $"from ({{0}}) as ids " +
                $"join {Teams} on ids.tid = {Teams}.apiid " +
                $"join {Lineups} on ids.fid = {Lineups}.fid " +
                $"join {Players} on {Lineups}.pid = {Players}.apiid " +
                $"left join {Player_Statistics} on {Lineups}.lid = {Player_Statistics}.lid " +
                $"left join {xPlayer_Statistics} on {Lineups}.lid = {xPlayer_Statistics}.lid ";
            BaseWrite = $"insert into {xPlayer_Statistics} " +
                $"(lid, xrating, xshots, xgoals, xassists, xsaves, xpasses, " +
                $"xtackles, xinterceptions, xdribbles, xfouls, xyellow, xred) " +
                $"values (:lid, :xrating, :xshots, :xgoals, :xassists, :xsaves, :xpasses, " +
                $":xtackles, :xinterceptions, :xdribbles, :xfouls, :xyellow, :xred) " +
                $"on conflict (lid) do update set " +
                $"lid=excluded.lid, xrating=excluded.xrating, xshots=excluded.xshots, xgoals=excluded.xgoals, xassists=excluded.xassists, xsaves=excluded.xsaves, xpasses=excluded.xpasses, " +
                $"xtackles=excluded.xtackles, xinterceptions=excluded.xinterceptions, xdribbles=excluded.xdribbles, xfouls=excluded.xfouls, xyellow=excluded.xyellow, xred=excluded.xred";
            OrderBy = $" order by (datetime, {Formations}.fid, xrating) desc";
        }

        public async Task<AvgxPlayer_StatisticsInfoRecord> GetAvgByPID(int pid, string? competition, int? limit, string? date)
        {
            return (await postgres.ReadDataAsync<AvgxPlayer_StatisticsInfoRecord>(AggRead("avg", competition, limit, date, pid: pid))).First();
        }

        public async Task<IEnumerable<AvgxPlayer_StatisticsInfoRecord>> GetAvgByTID(int tid, string? competition, int? limit, string? date)
        {
            return (await postgres.ReadDataAsync<AvgxPlayer_StatisticsInfoRecord>(AggRead("avg", competition, limit, date, tid: tid))).ToList();
        }

        public async Task<IEnumerable<xPlayer_StatisticsInfoRecord>> GetByMID(int mid, int? pid = null)
        {
            string pidfilter = pid is null ? "" : $"where {Players}.apiid={pid} ";
            return (await postgres.ReadDataAsync<xPlayer_StatisticsInfoRecord>(string.Format(BaseRead,
                $"select * from {Matches} join {Formations} on {Matches}.apiid = {Formations}.mid " +
                $"where {Matches}.apiid = {mid} ") +
                pidfilter + $"order by {Teams}.apiid, grid")).ToList();
        }

        public async Task<IEnumerable<xPlayer_StatisticsInfoRecord>> GetByPID(int pid, string? competition, int? limit, string? date)
        {
            string compfilter = competition is null ? "" : $"and competition=\'{competition}\' ";
            string limitfilter = limit is null ? "" : $"limit {limit}";
            string datefilter = date is null ? "" : $"and {Matches}.datetime::date < \'{date}\'::date ";
            return (await postgres.ReadDataAsync<xPlayer_StatisticsInfoRecord>(string.Format(BaseRead,
                $"select * from {Matches} join {Formations} on {Matches}.apiid = {Formations}.mid " +
                $"where true " + compfilter + datefilter) +
                $"where {Players}.apiid = {pid} " +
                $"order by datetime desc " +
                limitfilter)).ToList();
        }

        public async Task<SumxPlayer_StatisticsInfoRecord> GetSumByPID(int pid, string? competition, int? limit, string? date)
        {
            return (await postgres.ReadDataAsync<SumxPlayer_StatisticsInfoRecord>(AggRead("sum", competition, limit, date, pid: pid))).First();
        }

        public async Task<IEnumerable<SumxPlayer_StatisticsInfoRecord>> GetSumByTID(int tid, string? competition, int? limit, string? date)
        {
            return (await postgres.ReadDataAsync<SumxPlayer_StatisticsInfoRecord>(AggRead("sum", competition, limit, date, tid: tid))).ToList();
        }

        public async Task<xPlayer_StatisticsRecord> FindAsync(int lid)
        {
            var query = $"SELECT * FROM {xPlayer_Statistics} WHERE lid=:lid";

            return (await postgres.ReadDataAsync<xPlayer_StatisticsRecord>(query, new { lid })).ToList().FirstOrDefault();
        }

        public async Task SaveAsync(xPlayer_StatisticsRecord record) { await postgres.WriteDataAsync(BaseWrite, record); }


        private string AggRead(string agg, string? competition, int? limit, string? date,
            int? pid = null, int? tid = null)
        {
            string pidfilter = pid is null ? "" : $"where pid={pid} ";
            string compfilter = competition is null ? "" : $"and competition=\'{competition}\' ";
            string limitfilter = limit is not null ? $"limit {limit} " : "";
            string datefilter = date is null ? "" : $"and datetime < \'{date}\' ";

            string idsfilter = pid is null ?
                $"select * from " +
                $"{Matches} join {Formations} on {Matches}.apiid = {Formations}.mid " +
                $"where ({Matches}.home = {tid} or {Matches}.away = {tid}) and {Formations}.tid = {tid} " +
                compfilter + datefilter +
                $"order by datetime desc " +
                limitfilter
                :
                $"select * from " +
                $"{Matches} join {Formations} on {Matches}.apiid = {Formations}.mid " +
                compfilter + datefilter +
                "order by datetime desc ";

            limitfilter = pid is null ? "" : limitfilter;

            return $"select pid, first, last, nickname, headshot, count(*) as nummatches, " +
                $"{agg}(teamscored) as {agg}_teamscored, {agg}(teamconceded) as {agg}_teamconceded, " +
                $"{agg}(xrating) as {agg}_xrating, {agg}(rating_perf) as {agg}_rating_perf, " +
                $"{agg}(xshots) as {agg}_xshots, {agg}(shots_perf) as {agg}_shots_perf, " +
                $"{agg}(xgoals) as {agg}_xgoals, {agg}(goals_perf) as {agg}_goals_perf, " +
                $"{agg}(xassists) as {agg}_xassists, {agg}(assists_perf) as {agg}_assists_perf, " +
                $"{agg}(xsaves) as {agg}_xsaves, {agg}(saves_perf) as {agg}_saves_perf, " +
                $"{agg}(xpasses) as {agg}_xpasses, {agg}(passes_perf) as {agg}_passes_perf, " +
                $"{agg}(xtackles) as {agg}_xtackles, {agg}(tackles_perf) as {agg}_tackles_perf, " +
                $"{agg}(xinterceptions) as {agg}_xinterceptions, {agg}(interceptions_perf) as {agg}_interceptions_perf, " +
                $"{agg}(xdribbles) as {agg}_xdribbles, {agg}(dribbles_perf) as {agg}_dribbles_perf, " +
                $"{agg}(xfouls) as {agg}_xfouls, {agg}(fouls_perf) as {agg}_fouls_perf, " +
                $"{agg}(xyellow) as {agg}_xyellow, {agg}(yellow_perf) as {agg}_yellow_perf, " +
                $"{agg}(xred) as {agg}_xred, {agg}(red_perf) as {agg}_red_perf " +
                $"from (" +
                string.Format(BaseRead, idsfilter) +
                pidfilter + $"order by datetime desc " + limitfilter +
                $") as playerstats " +
                $"group by pid, first, last, nickname, headshot";
        }

    }
}

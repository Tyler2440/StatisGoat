using StatisGoat.Postgres;
using StatisGoat.Lineups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Team_Statistics
{
    public class Team_StatisticsRepository : Repository, ITeam_StatisticsRepository
    {
        public Team_StatisticsRepository(IPostgresConnection ipc) : base(ipc)
        {
            BaseWrite = $"insert into {Team_Statistics} " +
                $"(mid, tid, shots, shots_on_goal, shots_off_goal, shots_inside, shots_outside, " +
                $"blocked, fouls, corners, offsides, possession, yellows, reds, saves, passes, passes_accurate) " +
                $"values (:mid, :tid, :shots, :shots_on_goal, :shots_off_goal, :shots_inside, :shots_outside, " +
                $":blocked, :fouls, :corners, :offsides, :possession, :yellows, :reds, :saves, :passes, :passes_accurate) " +
                $"on conflict (mid, tid) do update set " +
                $"mid=excluded.mid, tid=excluded.tid, shots=excluded.shots, shots_on_goal=excluded.shots_on_goal, shots_off_goal=excluded.shots_off_goal, shots_inside=excluded.shots_inside, shots_outside=excluded.shots_outside, " +
                $"blocked=excluded.blocked, fouls=excluded.fouls, corners=excluded.corners, offsides=excluded.offsides, possession=excluded.possession, yellows=excluded.yellows, reds=excluded.reds, saves=excluded.saves, passes=excluded.passes, passes_accurate=excluded.passes_accurate;";
            BaseRead = $"select {Matches}.apiid as mid, {Teams}.apiid as tid, {Teams}.name as teamname, competition, datetime," +
                $"case when {Teams}.apiid = {Matches}.home then {Matches}.away " +
                $"else {Matches}.home " +
                $"end opponentid, " +
                $"case when {Teams}.apiid = {Matches}.home then (select name from {Teams} where apiid = {Matches}.away) " +
                $"else (select name from {Teams} where apiid = {Matches}.home) " +
                $"end opponentname, " +
                $"case when {Teams}.apiid = {Matches}.home then left(result, 1)::int " +
                $"else right(result, 1)::int " +
                $"end scored, " +
                $"case when {Teams}.apiid = {Matches}.home then right(result, 1)::int " +
                $"else left(result, 1)::int " +
                $"end conceded, " +
                $"case " +
                $"when teams.apiid = matches.home and left(result, 1)::int > right(result, 1)::int then \'W\' " +
                $"when teams.apiid = matches.home and left(result, 1)::int < right(result, 1)::int then \'L\' " +
                $"when teams.apiid = matches.away and left(result, 1)::int > right(result, 1)::int then \'L\' " +
                $"when teams.apiid = matches.away and left(result, 1)::int < right(result, 1)::int then \'W\'" +
                $"else \'D\' " +
                $"end result, " +
                $"status, elapsed, shots, shots_on_goal, shots_off_goal, shots_inside, shots_outside, " +
                $"blocked, fouls, corners, offsides, possession, yellows, reds, saves, passes, passes_accurate, " +
                $"case " +
                $"when passes > 0 then passes_accurate::real/passes::real " +
                $"else 0" +
                $"end pass_pct " +
                $"from {Teams} join {Matches} on {Teams}.apiid = {Matches}.home or {Teams}.apiid = {Matches}.away " +
                $"join {Formations} on {Matches}.apiid = {Formations}.mid and {Teams}.apiid = {Formations}.tid " +
                $"left join {Team_Statistics} on {Matches}.apiid = {Team_Statistics}.mid and {Teams}.apiid = {Team_Statistics}.tid ";
            /*BaseRead = $"select mid, tid, name as teamname, competition, datetime, " +
                $"case " +
                $"when {Matches}.home = tid then {Matches}.away " +
                $"else {Matches}.home " +
                $"end opponentid, " +
                $"case " +
                $"when {Matches}.home = tid then (select name from teams where apiid = {Matches}.away) " +
                $"else (select name from teams where apiid = {Matches}.home) " +
                $"end opponentname, " +
                $"case " +
                $"when {Matches}.home = tid then left(result, 1)::int " +
                $"else right(result, 1)::int " +
                $"end scored, " +
                $"case " +
                $"when {Matches}.home = tid then right(result, 1)::int " +
                $"else left(result, 1)::int " +
                $"end conceded, " +
                $"case " +
                $"when {Matches}.home = tid and left(result, 1)::int > right(result, 1)::int then 'W' " +
                $"when {Matches}.home = tid and left(result, 1)::int < right(result, 1)::int then 'L' " +
                $"when {Matches}.away = tid and left(result, 1)::int > right(result, 1)::int then 'L' " +
                $"when {Matches}.away = tid and left(result, 1)::int < right(result, 1)::int then 'W' " +
                $"else 'D' " +
                $"end result, " +
                $"status, elapsed, " +
                $"shots, shots_on_goal, shots_off_goal, shots_inside, shots_outside, " +
                $"blocked, fouls, corners, offsides, possession, yellows, reds, saves, " +
                $"passes, passes_accurate, " +
                $"case " +
                $"when passes > 0 then (passes_accurate::real/passes::real) " +
                $"else 0" +
                $"end pass_pct " +
                $"from {Matches} left join {Team_Statistics} on {Team_Statistics}.mid = {Matches}.apiid " +
                $"join {Teams} on tid = {Teams}.apiid";*/
            OrderBy = $"order by datetime desc";
        }

        public async Task<Avg_TeamStatisticsInfoRecord> FindAvgByTID(int tid, string? competition, int? limit, string? date)
        {
            var avg_stats = (await postgres.ReadDataAsync<Avg_TeamStatisticsInfoRecord>(AggRead("avg", tid, competition, limit, date))).ToList();
            if (avg_stats.Count > 0) { return avg_stats.First(); }
            else { return new Avg_TeamStatisticsInfoRecord(); }
        }

        public async Task<Team_StatisticsInfoRecord> FindByMIDTIDAsync(int mid, int tid)
        {
            return (await postgres.ReadDataAsync<Team_StatisticsInfoRecord>(BaseRead +
                $" where {Matches}.apiid=:mid and {Teams}.apiid=:tid " + 
                OrderBy, new { mid, tid })).ToList().First();
        }
        public async Task<List<Team_StatisticsInfoRecord>> FindByTID(int tid, string? competition, int? limit, string? date)
        {
            string compfilter = competition is null ? "" : $" and competition=\'{competition}\'";
            string limitfilter = limit is null ? "" : $" limit {limit}";
            string datefilter = date is null ? "" : $" and {Matches}.datetime::date < \'{date}\'::date ";
            return (await postgres.ReadDataAsync<Team_StatisticsInfoRecord>(BaseRead +
              $" where {Teams}.apiid=:tid " + compfilter + datefilter +
              OrderBy + limitfilter, new { tid })).ToList();
        }

        public async Task<Sum_TeamStatisticsInfoRecord> FindSumByTID(int tid, string? competition, int? limit, string? date)
        {
            var sum_stats = (await postgres.ReadDataAsync<Sum_TeamStatisticsInfoRecord>(AggRead("sum", tid, competition, limit, date))).ToList();
            if (sum_stats.Count > 0) { return sum_stats.First(); }
            else { return new Sum_TeamStatisticsInfoRecord(); }
        }

        public async Task SaveAsync(Team_StatisticsRecord record) { await postgres.WriteDataAsync(BaseWrite, record); }

        private string AggRead(string agg, int tid, string? competition, int? limit, string? date)
        {
            string compfilter = competition is null ? "" : $" and competition=\'{competition}\'";
            string limitfilter = limit is null ? "" : $" limit {limit}";
            string datefilter = date is null ? "" : $" and {Matches}.datetime::date < \'{date}\'::date ";
            return $"select tid, teamname, count(*) as nummatches, " +
                $"sum((result = 'W')::int) as wins, sum((result = 'L')::int) as losses, sum((result = 'D')::int) as draws, " +
                $"{agg}(scored) as {agg}scored, {agg}(conceded) as {agg}conceded, " +
                $"{agg}(shots) as {agg}shots, {agg}(shots_on_goal) as {agg}shots_on_goal, {agg}(shots_off_goal) as {agg}shots_off_goal, {agg}(shots_inside) as {agg}shots_inside, {agg}(shots_outside) as {agg}shots_outside, " +
                $"{agg}(blocked) as {agg}blocked, {agg}(fouls) as {agg}fouls, {agg}(corners) as {agg}corners, {agg}(offsides) as {agg}offsides, {agg}(possession) as {agg}possession, " +
                $"{agg}(yellows) as {agg}yellows, {agg}(reds) as {agg}reds, {agg}(saves) as {agg}saves, {agg}(passes) as {agg}passes, {agg}(passes_accurate) as {agg}passes_accurate, {agg}(pass_pct) as {agg}pass_pct " +
                "from (" + BaseRead + 
                $" where {Teams}.apiid={tid} " + compfilter + datefilter +
                OrderBy + limitfilter + ") as matches " +
                "group by tid, teamname";
        }
    }
}

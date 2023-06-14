using StatisGoat.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Player_Statistics
{
    public class Player_StatisticsRepository : Repository, IPlayer_StatisticsRepository
    {
        public Player_StatisticsRepository(IPostgresConnection ipc) : base(ipc)
        {
            BaseWrite = $"insert into {Player_Statistics} " +
                $"(lid, minutes, rating, substitute, shots, shots_on_goal, goals, assists, saves, conceded, " +
                $"passes, key_passes, passes_accurate, tackles, blocks, interceptions, duels, duels_won, " +
                $"dribbles, dribbles_won, dribbles_past, fouls_drawn, fouls_committed, yellow, red, " +
                $"penalties_won, penalties_conceded, penalties_scored, penalties_missed, penalties_saved) " +
                $"values (:lid, :minutes, :rating, :substitute, :shots, :shots_on_goal, :goals, :assists, :saves, :conceded, " +
                $":passes, :key_passes, :passes_accurate, :tackles, :blocks, :interceptions, :duels, :duels_won, " +
                $":dribbles, :dribbles_won, :dribbles_past, :fouls_drawn, :fouls_committed, :yellow, :red, " +
                $":penalties_won, :penalties_conceded, :penalties_scored, :penalties_missed, :penalties_saved) " +
                $"on conflict (lid) do update set " +
                $"lid=excluded.lid, minutes=excluded.minutes, rating=excluded.rating, substitute=excluded.substitute, shots=excluded.shots, shots_on_goal=excluded.shots_on_goal, goals=excluded.goals, assists=excluded.assists, saves=excluded.saves, conceded=excluded.conceded, " +
                $"passes=excluded.passes, key_passes=excluded.key_passes, passes_accurate=excluded.passes_accurate, tackles=excluded.tackles, blocks=excluded.blocks, interceptions=excluded.interceptions, duels=excluded.duels, duels_won=excluded.duels_won, " +
                $"dribbles=excluded.dribbles, dribbles_won=excluded.dribbles_won, dribbles_past=excluded.dribbles_past, fouls_drawn=excluded.fouls_drawn, fouls_committed=excluded.fouls_committed, yellow=excluded.yellow, red=excluded.red, " +
                $"penalties_won=excluded.penalties_won, penalties_conceded=excluded.penalties_conceded, penalties_scored=excluded.penalties_scored, penalties_missed=excluded.penalties_missed, penalties_saved=excluded.penalties_saved;";
            BaseRead = $"select {Players}.apiid as pid, first, last, nickname, headshot, {Lineups}.lid, " +
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
                $"minutes, rating, substitute, shots, shots_on_goal, goals, assists, (goals + assists) as goal_contributions, saves, conceded, " +
                $"passes, key_passes, passes_accurate, " +
                $"case " +
                $"when passes > 0 then passes_accurate::real/passes::real " +
                $"else 0 " +
                $"end pass_pct, " +
                $"tackles, blocks, interceptions, duels, duels_won, " +
                $"dribbles, dribbles_won, dribbles_past, fouls_drawn, fouls_committed, yellow, red, " +
                $"penalties_won, penalties_conceded, penalties_scored, penalties_missed, penalties_saved " +
                $"from ({{0}}) as ids " +
                $"join {Teams} on ids.tid = {Teams}.apiid " +
                $"join {Lineups} on ids.fid = {Lineups}.fid " +
                $"join {Players} on {Lineups}.pid = {Players}.apiid " +
                $"left join {Player_Statistics} on {Lineups}.lid = {Player_Statistics}.lid ";
        }

        public async Task<AvgPlayer_StatisticsInfoRecord> FindAvgByPID(int pid, string? competition, int? limit, string? date)
        {
            var avgstats = await postgres.ReadDataAsync<AvgPlayer_StatisticsInfoRecord>(AggRead("avg", competition, limit, date, pid: pid));
            if (avgstats.Any()) { return avgstats.First(); }
            else { return new AvgPlayer_StatisticsInfoRecord(); }
        }

        public async Task<List<AvgPlayer_StatisticsInfoRecord>> FindAvgByTID(int tid, string? competition, int? limit, string? date)
        {
            return (await postgres.ReadDataAsync<AvgPlayer_StatisticsInfoRecord>(AggRead("avg", competition, limit, date, tid: tid))).ToList();
        }

        public async Task<List<Player_StatisticsInfoRecord>> FindByDay(string day, string? competition)
        {
            string compfilter = competition is null ? "" : $"and competition = \'{competition}\' ";
            return (await postgres.ReadDataAsync<Player_StatisticsInfoRecord>(string.Format(BaseRead,
                $"select * from {Matches} join {Formations} on {Matches}.apiid = {Formations}.mid " +
                $"where datetime::date = \'{day}\'::date " +
                compfilter) + $"order by {Teams}.apiid, grid")).ToList();
        }

        public async Task<List<Player_StatisticsInfoRecord>> FindByMIDAsync(int mid, int? pid)
        {
            string pidfilter = pid is null ? "" : $"where {Players}.apiid={pid} ";
            return (await postgres.ReadDataAsync<Player_StatisticsInfoRecord>(string.Format(BaseRead,
                $"select * from {Matches} join {Formations} on {Matches}.apiid = {Formations}.mid " +
                $"where {Matches}.apiid = {mid} ") + 
                pidfilter + $"order by {Teams}.apiid, grid")).ToList();
        }

        public async Task<List<Player_StatisticsInfoRecord>> FindByPIDAsync(int pid, string? competition, int? limit, string? date)
        {
            string compfilter = competition is null ? "" : $"and competition=\'{competition}\' ";
            string limitfilter = limit is null ? "" : $"limit {limit}";
            string datefilter = date is null ? "" : $"and {Matches}.datetime::date < \'{date}\'::date ";
            return (await postgres.ReadDataAsync<Player_StatisticsInfoRecord>(string.Format(BaseRead,
                $"select * from {Matches} join {Formations} on {Matches}.apiid = {Formations}.mid " +
                $"where true " + compfilter + datefilter) +
                $"where {Players}.apiid = {pid} " + 
                $"order by datetime desc " + 
                limitfilter)).ToList();
        }

        public async Task<SumPlayer_StatisticsInfoRecord> FindSumByPID(int pid, string? competition, int? limit, string? date)
        {
            return (await postgres.ReadDataAsync<SumPlayer_StatisticsInfoRecord>(AggRead("sum", competition, limit, date, pid: pid))).First();
        }

        public async Task<List<SumPlayer_StatisticsInfoRecord>> FindSumByTID(int tid, string? competition, int? limit, string? date)
        {
            return (await postgres.ReadDataAsync<SumPlayer_StatisticsInfoRecord>(AggRead("sum", competition, limit, date, tid: tid))).ToList();
        }

        public async Task SaveAsync(Player_StatisticsRecord record) { await postgres.WriteDataAsync(BaseWrite, record); }

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

            return $"select pid, first, last, nickname, headshot, count(*) as nummatches, {agg}(teamscored) as {agg}teamscored, {agg}(teamconceded) as {agg}teamconceded, " +
                $"{agg}(minutes) as {agg}minutes, {agg}(rating) as {agg}rating, " +
                $"{agg}(shots) as {agg}shots, {agg}(shots_on_goal) as {agg}shots_on_goal, {agg}(goals) as {agg}goals, " +
                $"{agg}(assists) as {agg}assists, {agg}(goal_contributions) as {agg}goal_contributions, " +
                $"{agg}(saves) as {agg}saves, {agg}(conceded) as {agg}conceded, {agg}(passes) as {agg}passes, " +
                $"{agg}(key_passes) as {agg}key_passes, {agg}(passes_accurate) as {agg}passes_accurate, " +
                $"{agg}(pass_pct) as {agg}pass_pct, {agg}(tackles) as {agg}tackles, " +
                $"{agg}(blocks) as {agg}blocks, {agg}(interceptions) as {agg}interceptions, " +
                $"{agg}(duels) as {agg}duels, {agg}(duels_won) as {agg}duels_won, " +
                $"{agg}(dribbles) as {agg}dribbles, {agg}(dribbles_won) as {agg}dribbles_won, " +
                $"{agg}(dribbles_past) as {agg}dribbles_past, {agg}(fouls_drawn) as {agg}fouls_drawn, " +
                $"{agg}(fouls_committed) as {agg}fouls_committed, {agg}(yellow) as {agg}yellow, {agg}(red) as {agg}red, " +
                $"{agg}(penalties_won) as {agg}penalties_won, {agg}(penalties_conceded) as {agg}penalties_conceded, " +
                $"{agg}(penalties_scored) as {agg}penalties_scored, {agg}(penalties_missed) as {agg}penalties_missed, " +
                $"{agg}(penalties_saved) as {agg}penalties_saved, " +
                $"case " +
                $"when sum(teamscored) > 0 then sum(goals)::real/sum(teamscored)::real " +
                $"else 0 " +
                $"end percent_scored, " +
                $"case " +
                $"when sum(teamscored) > 0  then sum(assists)::real/sum(teamscored)::real " +
                $"else 0 " +
                $"end percent_assisted, " +
                $"case when sum(teamscored) > 0 then sum(goal_contributions)::real/sum(teamscored)::real " +
                $"else 0 " +
                $"end percent_contributed " +
                $"from (" +
                string.Format(BaseRead, idsfilter) +
                pidfilter +  $"order by datetime desc " + limitfilter + 
                $") as playerstats " +        
                $"group by pid, first, last, nickname, headshot";
        }
    }
}

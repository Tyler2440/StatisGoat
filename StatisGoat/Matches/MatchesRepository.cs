using Newtonsoft.Json;
using StatisGoat.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StatisGoat.Matches
{
    public class MatchesRepository : Repository, IMatchesRepository
    {
        public MatchesRepository(IPostgresConnection ipc) : base(ipc)
        {
            BaseWrite = $"insert into {Matches} (apiid, home, away, competition, datetime, status, result, elapsed) " +
                $"values (:apiid, :home, :away, :competition, :datetime, :status, :result, :elapsed) " +
                $"on conflict (apiid)" +
                $"do update set home=excluded.home, away=excluded.away, competition=excluded.competition, " +
                $"datetime=excluded.datetime, status=excluded.status, result=excluded.result, elapsed=excluded.elapsed";
            BaseRead = $"select matchid, competition, datetime, venue, " +
                $"homeid, homenation, homename, homeformation, homebadge, homexg, " +
                $"case " +
                $"when homexg > awayxg then 1 - (1/(sigma*sqrt(2*pi())))*2.71828^(-1*abs(homexg - awayxg)^2/(2*sigma^2)) - (1/(sigma*sqrt(2*pi())))*2.71828^(-1*(abs(homexg - awayxg) + 0.15)^2/(2*sigma^2)) " +
                $"else (1/(sigma*sqrt(2*pi())))*2.71828^(-1*(abs(homexg - awayxg) + 0.15)^2/(2*sigma^2)) " +
                $"end homepct, " +
                $"(1/(sigma*sqrt(2*pi())))*2.71828^(-1*(homexg - awayxg)^2/(2*sigma^2)) as drawpct, " +
                $"case " +
                $"when awayxg > homexg then 1 - (1/(sigma*sqrt(2*pi())))*2.71828^(-1*abs(homexg - awayxg)^2/(2*sigma^2)) - (1/(sigma*sqrt(2*pi())))*2.71828^(-1*(abs(homexg - awayxg) + 0.15)^2/(2*sigma^2)) " +
                $"else (1/(sigma*sqrt(2*pi())))*2.71828^(-1*(abs(homexg - awayxg) + 0.15)^2/(2*sigma^2)) " +
                $"end awaypct, " +
                $"awayxg, awayid, awaynation, awayname, awayformation, awaybadge, status, result, elapsed " +
                $"from " +
                $"(select {Matches}.apiid as matchid, competition, datetime, thome.venue as venue, " +
                $"thome.apiid as homeid, thome.nation as homenation, thome.name as homename, fhome.formation as homeformation, thome.badge as homebadge, " +
                $"(select sum(xgoals) " +
                $"from {xPlayer_Statistics} join {Lineups} on {xPlayer_Statistics}.lid = {Lineups}.lid " +
                $"join {Formations} on {Lineups}.fid = {Formations}.fid " +
                $"join {Player_Statistics} on {Lineups}.lid = {Player_Statistics}.lid " +
                $"where minutes > 0 and {Formations}.fid = fhome.fid " +
                $"group by {Formations}.fid) as homexg, " +
                $"(select sum(xgoals) " +
                $"from {xPlayer_Statistics} join {Lineups} on {xPlayer_Statistics}.lid = {Lineups}.lid " +
                $"join {Formations} on {Lineups}.fid = {Formations}.fid  " +
                $"join {Player_Statistics} on {Lineups}.lid = {Player_Statistics}.lid  " +
                $"where minutes > 0 and {Formations}.fid = faway.fid " +
                $"group by {Formations}.fid) as awayxg, " +
                $"taway.apiid as awayid, taway.nation as awaynation, taway.name as awayname, faway.formation as awayformation, taway.badge as awaybadge, " +
                $"status, result, elapsed, " +
                $"(select stddev(avgerr)" +
                $"from (select sum(goals - xgoals) as avgerr " +
                $"from {xPlayer_Statistics} join {Lineups} on {xPlayer_Statistics}.lid = {Lineups}.lid " +
                $"join {Formations} on {Lineups}.fid = {Formations}.fid " +
                $"join {Player_Statistics} on {Lineups}.lid = {Player_Statistics}.lid " +
                $"where minutes > 0 " +
                $"group by {Formations}.fid) as t) as sigma " +
                $"from " +
                $"{Matches} join {Teams} as thome on {Matches}.home = thome.apiid " +
                $"join {Teams} as taway on {Matches}.away = taway.apiid " +
                $"left join {Formations} as fhome on fhome.mid = {Matches}.apiid and fhome.tid = {Matches}.home " +
                $"left join {Formations} as faway on faway.mid = {Matches}.apiid and faway.tid = {Matches}.away ";
            OrderBy = $"order by competition, datetime) as matchesinfo";
        }

        public async Task<List<MatchesInfoRecord>> FindAllAsync() 
        { 
            return (await postgres.ReadDataAsync<MatchesInfoRecord>(BaseRead + " " + OrderBy)).ToList(); 
        }

        public async Task<List<MatchesInfoRecord>> FindByTidAsync(int id, int? limit, string? date)
        {
            string datefilter = date is null ? "" : $" and datetime::date < \'{date}\'::date ";
            string limitfilter = limit is null ? "" : $" limit {limit}";
            return (await postgres.ReadDataAsync<MatchesInfoRecord>(BaseRead + 
                $" where {Matches}.status=\'FT\' and " +
                $"({Matches}.home=:id or {Matches}.away=:id)" + datefilter +
                "order by competition, datetime desc) as matchesinfo" + limitfilter, new { id })).ToList();
        }

        public async Task<List<MatchesInfoRecord>> FindByTidTidAsync(int id1, int id2)
        {
            return (await postgres.ReadDataAsync<MatchesInfoRecord>(BaseRead + 
                $" where {Matches}.status=\'FT\' and " +
                $"{Matches}.home=:id1 and {Matches}.away=:id2 or " +
                $"{Matches}.away=:id1 and {Matches}.home=:id2 " + 
                OrderBy, new { id1, id2 })).ToList();
        }

        public async Task<MatchesInfoRecord> FindByMidAsync(int id)
        {
            return (await postgres.ReadDataAsync<MatchesInfoRecord>(BaseRead + 
                $" where {Matches}.apiid=:id " + OrderBy, new { id })).ToList().First();
        }

        public async Task<List<MatchesInfoRecord>> FindByDayRangeAsync(string start, string end)
        {
            return (await postgres.ReadDataAsync<MatchesInfoRecord>(BaseRead + 
                $" where {Matches}.datetime::date>=:start::date and {Matches}.datetime::date<=:end::date " + 
                OrderBy, new { start, end })).ToList();
        }

        public async Task SaveAsync(MatchesRecord record) { await postgres.WriteDataAsync(BaseWrite, record); }
    }
}

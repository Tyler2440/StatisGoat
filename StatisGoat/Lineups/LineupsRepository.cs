using StatisGoat.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Lineups
{
    public class LineupsRepository : Repository, ILineupsRepository
    {
        public LineupsRepository(IPostgresConnection ipc) : base(ipc)
        {
            BaseWrite = $"insert into {Lineups} (fid, pid, position, grid, number) " +
                $"values (:fid, :pid, :position, :grid, :number) " +
                $"on conflict (fid, pid) " +
                $"do update set fid=excluded.fid, pid=excluded.pid, grid=excluded.grid, number=excluded.number;";
            BaseRead = $"select lid as lineupid, mid as matchid, " +
                $"{Teams}.apiid as teamid, {Teams}.name as teamname, " +
                $"{Players}.apiid as playerid, {Players}.first as firstname, {Players}.last as lastname, {Players}.nickname as nickname, number, " +
                $"{Players}.dob as dob, {Players}.height as height, {Players}.weight as weight, {Players}.nationality as nationality, {Players}.headshot as headshot, " +
                $"{Lineups}.position as position, {Lineups}.grid as grid " +
                $"from {Lineups} join {Formations} " +
                $"on {Lineups}.fid = {Formations}.fid " +
                $"join {Matches} " +
                $"on {Formations}.mid = {Matches}.apiid " +
                $"join teams " +
                $"on {Formations}.tid = {Teams}.apiid " +
                $"join {Players} " +
                $"on {Lineups}.pid = {Players}.apiid";
            OrderBy = "order by teams.apiid, grid";
        }

        public async Task<List<LineupsInfoRecord>> FindByMIDAsync(int mid)
        {
            return (await postgres.ReadDataAsync<LineupsInfoRecord>(BaseRead + 
                $" where {Matches}.apiid=:mid " + 
                OrderBy, new { mid })).ToList();
        }

        public async Task<List<LineupsInfoRecord>> FindByMIDTIDAsync(int mid, int tid)
        {
            return (await postgres.ReadDataAsync<LineupsInfoRecord>(BaseRead +
                $" where {Matches}.apiid=:mid and {Teams}.apiid=:tid " +
                OrderBy, new { mid, tid })).ToList();
        }

        public async Task<int> FindByPidFidAsync(int pid, int fid)
        {
            var query = $"select lid from {Lineups} where pid=:pid and fid=:fid;";
            return (await postgres.ReadDataAsync<int>(query, new { pid, fid })).ToList().First();
        }

        public async Task SaveAsync(LineupsRecord record) { await postgres.WriteDataAsync(BaseWrite, record); }

    }
}

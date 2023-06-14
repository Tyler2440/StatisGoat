using Npgsql.Replication.PgOutput.Messages;
using StatisGoat.Postgres;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Events
{
    public class EventsRepository : Repository, IEventsRepository
    {
        public EventsRepository(IPostgresConnection ipc) : base(ipc) 
        {
            BaseWrite = $"insert into {Events} " +
                $"(mid, tid, pid, minute, type, detail, assist, comment) " +
                $"values (:mid, :tid, :pid, :minute, :type, :detail, :assist, :comment) " +
                $"on conflict (mid, tid, pid, minute, type) " +
                $"do update set detail=excluded.detail, assist=excluded.assist, comment=excluded.comment;";
            BaseRead = $"select mid as matchid, " +
                $"minute, type, detail, comment, " +
                $"{Teams}.apiid as teamid, {Teams}.name as teamname, badge, " +
                $"{Players}.apiid as playerid, {Players}.first as first, {Players}.last as last, {Players}.headshot as headshot, {Players}.nickname as nickname, " +
                $"assist.apiid as assistid, assist.first as assistfirst, assist.last as assistlast, assist.headshot as assistheadshot, assist.nickname as assistnickname " +
                $"from {Events} join {Teams} " +
                $"on {Events}.tid = {Teams}.apiid " +
                $"join {Players} " +
                $"on {Events}.pid = {Players}.apiid " +
                $"left join {Players} as assist " +
                $"on {Events}.assist = assist.apiid";
            OrderBy = "order by minute";
        }

        public async Task<List<EventsInfoRecord>> FindByMIDAsync(int mid)
        {
            return (await postgres.ReadDataAsync<EventsInfoRecord>(BaseRead + 
                $" where mid=:mid " + 
                OrderBy, new { mid })).ToList();
        }

        public async Task SaveAsync(EventsRecord record) { await postgres.WriteDataAsync(BaseWrite, record); }
    }
}

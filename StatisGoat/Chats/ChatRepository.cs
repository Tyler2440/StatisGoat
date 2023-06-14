using StatisGoat.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Chats
{
    public class ChatRepository : Repository, IChatRepository
    {
        public ChatRepository(IPostgresConnection ipc) : base(ipc)
        {
            BaseWrite = $"insert into {Forums} " +
                $"(matchid, accountid, message, timestamp, threadid) " +
                $"values (:matchid, :accountid, :message, :timestamp, :threadid) " +
                $"on conflict (chatid) do update set " +
                $"matchid=excluded.matchid, accountid=excluded.accountid, message=excluded.message, timestamp=excluded.timestamp, threadid=excluded.threadid";
            BaseRead = $"select *," +
                $"case " +
                $"when exists (select likeid from {Likes} where gamechats.chatid = {Likes}.chatid and accountid = {{0}}) then true " +
                $"else false " +
                $"end liked, " +
                $"case " +
                $"when exists (select dislikeid from {Dislikes} where gamechats.chatid = {Dislikes}.chatid and accountid = {{0}}) then true " +
                $"else false " +
                $"end disliked " +
                $"from (select {Forums}.chatid, matchid, accountid, message, timestamp, threadid, email, first, last, " +
                $"likes, dislikes " +
                $"from {Forums} join {Accounts} on {Forums}.accountid = {Accounts}.id " +
                $"left join (select chatid, count(*) as likes from {Likes} group by chatid) as likes " +
                $"on {Forums}.chatid = {Likes}.chatid " +
                $"left join (select chatid, count(*) as dislikes from {Dislikes} group by chatid) as dislikes " +
                $"on {Forums}.chatid = {Dislikes}.chatid " +
                $"where matchid = {{1}}) as gamechats";
        }

        public async Task DeleteAsync(int id)
        {
            await postgres.ReadDataAsync<ChatInfoRecord>($"delete from {Forums} where chatid={id} or threadid={id}");
        }

        public async Task DislikeAsync(int accountid, int chatid)
        {
            await postgres.WriteDataAsync($"insert into {Dislikes} (accountid, chatid) values ({accountid}, {chatid}) on conflict (accountid, chatid) do nothing");
        }

        public async Task<List<ChatInfoRecord>> FindByMID(int mid, int? accountid)
        {
            var idString = "null";
            if (accountid != null)
                idString = accountid.ToString();

            return (await postgres.ReadDataAsync<ChatInfoRecord>(string.Format(BaseRead, idString, mid))).ToList();
        }

        public async Task LikeAsync(int accountid, int chatid)
        {
            await postgres.WriteDataAsync($"insert into {Likes} (accountid, chatid) values ({accountid}, {chatid}) on conflict (accountid, chatid) do nothing");
        }

        public async Task SaveAsync(ChatRecord record) { await postgres.WriteDataAsync(BaseWrite, record ); }

        public async Task UndislikeAsync(int accountid, int chatid)
        {
            await postgres.WriteDataAsync($"delete from {Dislikes} where accountid = {accountid} and chatid = {chatid}");

        }

        public async Task UnlikeAsync(int accountid, int chatid)
        {
            await postgres.WriteDataAsync($"delete from {Likes} where accountid = {accountid} and chatid = {chatid}");
        }
    }
}

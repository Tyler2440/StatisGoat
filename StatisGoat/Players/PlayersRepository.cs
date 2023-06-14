using Newtonsoft.Json;
using StatisGoat.ExternalApi;
using StatisGoat.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatisGoat.Players
{
    public class PlayersRepository : Repository, IPlayersRepository
    {
        private IFootballApi footballApi;

        public PlayersRepository(IPostgresConnection ipc, IFootballApi footballApi) : base(ipc)
        {
            this.footballApi = footballApi;

            BaseWrite = $"insert into {Players} (ApiId, tId, First, Last, Dob, Height, Weight, Nationality, Headshot) " +
                $"values (:apiid, :tid, :first, :last, :dob, :height, :weight, :nationality, :headshot) " +
                $"on conflict (apiid) " +
                $"do update set apiid=excluded.apiid, tid=excluded.tid, first=excluded.first, last=excluded.last, nickname=excluded.nickname, dob=excluded.dob, " +
                $"height=excluded.height, weight=excluded.weight, nationality=excluded.nationality, headshot=excluded.headshot";
            BaseRead = $"select {Players}.apiid as apiid, first, last, nickname, dob, height, weight, nationality, headshot, " +
                $"{Teams}.apiid as tid, {Teams}.name as teamname, {Teams}.nation as teamnation, badge " +
                $"from {Players} join {Teams} " +
                $"on {Players}.tid = {Teams}.apiid";
            OrderBy = "order by last";
        }

        public async Task<List<PlayersInfoRecord>> FindAllAsync()
        {
            return (await postgres.ReadDataAsync<PlayersInfoRecord>(BaseRead + " " + OrderBy)).ToList();
        }

        public async Task<PlayersInfoRecord?> FindByPIDAsync(int id)
        {
            var record = await postgres.ReadDataAsync<PlayersInfoRecord>(BaseRead +
                $" where {Players}.apiid=:id", new { id });
            return record.Any() ? record.First() : null;
        }

        public async Task<List<PlayersInfoRecord>> FindByNameAsync(string first, string last)
        {
            return (await postgres.ReadDataAsync<PlayersInfoRecord>(BaseRead +
                " where first=:first and last=:last", new { first, last })).ToList();
        }

        public async Task<List<PlayersInfoRecord>> FindByTeamAsync(int id)
        {
            return (await postgres.ReadDataAsync<PlayersInfoRecord>(BaseRead +
                $" where {Teams}.apiid=:id " +
                OrderBy, new { id })).ToList();
        }

        public async Task SaveAsync(PlayersRecord record) { await postgres.WriteDataAsync(BaseWrite, record); }

        public async Task BackfillPlayer(int id, int tid, int year)
        {
            using (var response = await footballApi.GetAsync($"players?id={id}&season={year}"))
            {
                response.EnsureSuccessStatusCode();
                var result = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
                {
                    response = new[]
                    {
                            new
                            {
                                player = new
                                {
                                    id = 0, firstname = "", lastname = "", nationality = "",
                                    birth = new { date = "" },
                                    height = "", weight = "", photo = ""
                                }
                            }
                        }
                });

                if (!result.response.Any()) { throw new ArgumentException($"Player does not exist with id: {id}"); }

                var player_info = result.response[0].player;
                await SaveAsync(new PlayersRecord
                {
                    ApiID = player_info.id,
                    TID = tid,
                    First = player_info.firstname,
                    Last = player_info.lastname,
                    DOB = player_info.birth.date == null ? DateTime.MinValue : DateTime.Parse(player_info.birth.date),
                    Height = player_info.height == null ? 0 : Int32.Parse(player_info.height.Substring(0, player_info.height.IndexOf(" "))),
                    Weight = player_info.weight == null ? 0 : Int32.Parse(player_info.weight.Substring(0, player_info.weight.IndexOf(" "))),
                    Nationality = player_info.nationality ?? "",
                    Headshot = player_info.photo ?? ""
                });
            }
        }
    }
}

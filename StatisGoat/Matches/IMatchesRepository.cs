using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisGoat.Matches
{
    public interface IMatchesRepository
    {
        Task<List<MatchesInfoRecord>> FindAllAsync();
        //List<MatchesRecord> FindTeamsMatches(int teamId);
        Task<List<MatchesInfoRecord>> FindByTidAsync(int teamId, int? limit, string? date);
        Task<List<MatchesInfoRecord>> FindByTidTidAsync(int team1Id, int team2Id);
        Task<List<MatchesInfoRecord>> FindByDayRangeAsync(string start, string end);
        //List<MatchesRecord> Find(int gameId);
        Task<MatchesInfoRecord> FindByMidAsync(int gameId);
        Task SaveAsync(MatchesRecord record);
    }
}

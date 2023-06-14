using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Chats
{
    public interface IChatRepository
    {
        Task SaveAsync(ChatRecord record);
        Task DeleteAsync(int id);
        Task<List<ChatInfoRecord>> FindByMID(int mid, int? accountid);
        Task LikeAsync(int accountid, int chatid);
        Task UnlikeAsync(int accountid, int chatid);
        Task DislikeAsync(int accountid, int chatid);
        Task UndislikeAsync(int accountid, int chatid);
    }
}

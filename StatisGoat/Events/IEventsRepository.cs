using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Events
{
    public interface IEventsRepository
    {
        Task SaveAsync(EventsRecord record);
        Task<List<EventsInfoRecord>> FindByMIDAsync(int mid);
    }
}

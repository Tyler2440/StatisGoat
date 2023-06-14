using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisGoat.Postgres
{
    public interface IPostgresConnection
    {
        IEnumerable<T> ReadData<T>(string statement, object parameters = null);

        Task<IEnumerable<T>> ReadDataAsync<T>(string statement, object parameters = null);

        void WriteData(string statement, object parameters = null);

        Task<dynamic> WriteDataAsync(string statement, object parameters = null);
    }
}

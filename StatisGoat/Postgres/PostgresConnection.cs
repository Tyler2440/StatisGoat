using System;
using System.Collections.Generic;
using StatisGoat.Postgres;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Dapper;
using System.Threading.Tasks;

namespace StatisGoat.Postgres
{
    public class PostgresConnection : IPostgresConnection
    {
        string connectionString;
        string postgresHost, postgresDatabase, postgresUsername, postgresPassword;

        public PostgresConnection(IConfiguration configuration)
        {
            postgresHost = configuration["PostgresHost"];
            postgresDatabase = configuration["PostgresDatabase"];
            postgresUsername = configuration["PostgresUsername"];
            postgresPassword = configuration["PostgresPassword"];

            connectionString = $"Host={postgresHost};Username={postgresUsername};Password={postgresPassword};Database={postgresDatabase};Pooling=true;Maximum Pool Size=100;Connection Idle Lifetime=300;Timeout=15;";
        }

        public NpgsqlConnection BuildConnection()
        {
            using var source = NpgsqlDataSource.Create(connectionString);
            return source.OpenConnection();
        }

        public IEnumerable<T> ReadData<T>(string statement, object parameters = null)
        {
            NpgsqlConnection connection = null;
            try
            {
                connection = BuildConnection();
                return connection.Query<T>(statement, parameters);
            }
            catch (Exception e)
            {
                throw new TimeoutException($"Read operation timed out with message {e.Message}");
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<IEnumerable<T>> ReadDataAsync<T>(string statement, object parameters = null)
        {
            NpgsqlConnection connection = null;
            try
            {
                connection = BuildConnection();
                return await connection.QueryAsync<T>(statement, parameters);
            }
            catch (Exception e)
            {
                throw new TimeoutException($"Read operation timed out with message {e.Message}");
            }
            finally
            {
                if (connection != null)
                {
                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                }
            }
        }

        public void WriteData(string statement, object parameters = null)
        {
            NpgsqlConnection connection = null;
            try
            {
                connection = BuildConnection();
                connection.Execute(statement, parameters);
            }
            catch (Exception e)
            {
                throw new TimeoutException($"Write operation timed out with message {e.Message}");
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<dynamic> WriteDataAsync(string statement, object parameters = null)
        {
            NpgsqlConnection connection = null;
            try
            {
                connection = BuildConnection();
                return await connection.ExecuteAsync(statement, parameters);
            }
            catch (Exception e)
            {
                throw new TimeoutException($"Write operation timed out with message {e.Message}");
            }
            finally
            {
                if (connection != null)
                {
                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                }
            }
        }
    }
}

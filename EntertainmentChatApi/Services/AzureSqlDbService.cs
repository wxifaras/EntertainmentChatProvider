using Dapper;
using EntertainmentChatApi.Interfaces;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;

namespace EntertainmentChatApi.Services
{
    public class AzureSqlDbService(string connectionString) : IAzureSqlDbService
    {
        public async Task<string> GetDbResults(string query)
        {
            using IDbConnection connection = new SqlConnection(connectionString);

            var dbResult = await connection.QueryAsync<dynamic>(query);
            var jsonString = JsonSerializer.Serialize(dbResult);

            return jsonString;
        }
    }
}
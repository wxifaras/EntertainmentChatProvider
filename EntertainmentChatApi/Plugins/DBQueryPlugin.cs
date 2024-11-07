using EntertainmentChatApi.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace EntertainmentChatApi.Plugins
{
    public class DBQueryPlugin(IAzureSqlDbService azureDbService)
    {
        [KernelFunction]
        [Description("Executes a SQL query to get information for movies, shows, or other entertainment data. Attempt to query name column if not enough information in prompt.")]
        public async Task<string> GetEntertainmentData(string query)
        {
            var dbResults = await azureDbService.GetDbResults(query);
            return dbResults;
        }
    }
}

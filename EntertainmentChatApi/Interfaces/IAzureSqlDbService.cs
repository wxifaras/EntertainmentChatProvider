namespace EntertainmentChatApi.Interfaces
{
    public interface IAzureSqlDbService
    {
        Task<string> GetDbResults(string query);
    }
}

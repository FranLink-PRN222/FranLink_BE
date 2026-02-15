using DataAccessLayer_FranLink.Models;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface ISystemConfigService
    {
        Task<List<SystemConfig>> GetAllConfigsAsync();
        Task<string> GetConfigValueAsync(string key);
        Task SetConfigAsync(string key, string value, string description);
        Task DeleteConfigAsync(int id);
    }
}

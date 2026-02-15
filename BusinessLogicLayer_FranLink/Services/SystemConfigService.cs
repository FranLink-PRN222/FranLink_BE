using DataAccessLayer_FranLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class SystemConfigService : ISystemConfigService
    {
        private readonly FranLinkContext _context;

        public SystemConfigService(FranLinkContext context)
        {
            _context = context;
        }

        public async Task<List<SystemConfig>> GetAllConfigsAsync()
        {
            return await _context.SystemConfigs
                .OrderBy(c => c.ConfigKey)
                .ToListAsync();
        }

        public async Task<string> GetConfigValueAsync(string key)
        {
            var config = await _context.SystemConfigs
                .FirstOrDefaultAsync(c => c.ConfigKey == key);
            return config?.ConfigValue;
        }

        public async Task SetConfigAsync(string key, string value, string description)
        {
            var config = await _context.SystemConfigs
                .FirstOrDefaultAsync(c => c.ConfigKey == key);

            if (config != null)
            {
                config.ConfigValue = value;
                config.Description = description;
                config.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.SystemConfigs.Add(new SystemConfig
                {
                    ConfigKey = key,
                    ConfigValue = value,
                    Description = description,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteConfigAsync(int id)
        {
            var config = await _context.SystemConfigs.FindAsync(id);
            if (config != null)
            {
                _context.SystemConfigs.Remove(config);
                await _context.SaveChangesAsync();
            }
        }
    }
}

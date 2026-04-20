using DirectoryMonitor.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DirectoryMonitor.Data.Repositories
{
    public class RuleRepository : IRuleRepository
    {
        private readonly AppDbContext _context;

        public RuleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Rule>> GetAllAsync()
        {
            return await _context.Rules.ToListAsync();
        }

        public async Task<List<Rule>> GetActiveAsync()
        {
            return await _context.Rules
                .Where(r => r.IsActive)
                .OrderBy(r => r.Priority)
                .ToListAsync();
        }

        public async Task<Rule?> GetByIdAsync(int id)
        {
            return await _context.Rules.FindAsync(id);
        }

        public async Task AddAsync(Rule rule)
        {
            rule.CreatedAt = DateTime.UtcNow;
            await _context.Rules.AddAsync(rule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Rule rule)
        {
            rule.UpdatedAt = DateTime.UtcNow;
            _context.Rules.Update(rule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rule = await GetByIdAsync(id);
            if (rule != null)
            {
                _context.Rules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }
    }
}

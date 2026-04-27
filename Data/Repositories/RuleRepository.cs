using DirectoryMonitor.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DirectoryMonitor.Data.Repositories
{
    public class RuleRepository : IRuleRepository
    {
        IDbContextFactory<AppDbContext> _contextFactory;

        public RuleRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Rule>> GetAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Rules.ToListAsync();
        }

        public async Task<List<Rule>> GetActiveAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Rules
                .Where(r => r.IsActive)
                .OrderBy(r => r.Priority)
                .ToListAsync();
        }

        public async Task<Rule?> GetByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Rules.FindAsync(id);
        }

        public async Task<Rule?> GetRuleWithActionsAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Rules
                .Include(r => r.RuleActions)
                    .ThenInclude(ra => ra.Action)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(Rule rule)
        {
            using var context = _contextFactory.CreateDbContext();
            rule.CreatedAt = DateTime.UtcNow;
            await context.Rules.AddAsync(rule);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Rule rule)
        {
            using var context = _contextFactory.CreateDbContext();
            rule.UpdatedAt = DateTime.UtcNow;
            context.Rules.Update(rule);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var rule = await GetByIdAsync(id);
            if (rule != null)
            {
                context.Rules.Remove(rule);
                await context.SaveChangesAsync();
            }
        }
    }
}

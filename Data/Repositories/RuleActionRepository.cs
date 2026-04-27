using DirectoryMonitor.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMonitor.Data.Repositories
{
    public class RuleActionRepository : IRuleActionRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public RuleActionRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddAsync(RuleAction ruleAction)
        {
            using var context = _contextFactory.CreateDbContext();
            await context.RuleActions.AddAsync(ruleAction);
            await context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<RuleAction> ruleActions)
        {
            using var context = _contextFactory.CreateDbContext();
            await context.RuleActions.AddRangeAsync(ruleActions);
            await context.SaveChangesAsync();
        }

        public async Task DeleteByRuleIdAsync(int ruleId)
        {
            using var context = _contextFactory.CreateDbContext();
            var ruleActions = await context.RuleActions
                .Where(ra => ra.RuleId == ruleId)
                .ToListAsync();

            context.RuleActions.RemoveRange(ruleActions);
            await context.SaveChangesAsync();
        }

        public async Task<List<RuleAction>> GetByRuleIdAsync(int ruleId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.RuleActions
                .Where(ra => ra.RuleId == ruleId)
                .OrderBy(ra => ra.Order)
                .ToListAsync();
        }

        public async Task<List<RuleAction>> GetByRuleIdWithActionsAsync(int ruleId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.RuleActions
                .Include(ra => ra.Action)
                .Where(ra => ra.RuleId == ruleId)
                .OrderBy(ra => ra.Order)
                .ToListAsync();
        }
    }
}

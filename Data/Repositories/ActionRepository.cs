using DirectoryMonitor.Helpers;
using DirectoryMonitor.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DirectoryMonitor.Data.Repositories
{
    public class ActionRepository : IActionRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ActionRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Rule?> GetRuleWithActionsAsync(int ruleId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Rules
                .Include(r => r.RuleActions)
                    .ThenInclude(ra => ra.Action)
                .FirstOrDefaultAsync(r => r.Id == ruleId);
        }
        public async Task<List<Models.Entities.Action>> GetAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Actions.ToListAsync();
        }

        public async Task<Models.Entities.Action?> GetByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Actions.FindAsync(id);
        }

        public async Task<Rule> AddRuleWithActionsAsync(Rule rule, List<RuleActionItem> actions)
        {
            using var context = _contextFactory.CreateDbContext();

            rule.CreatedAt = DateTime.UtcNow;
            await context.Rules.AddAsync(rule);
            await context.SaveChangesAsync();

            foreach (var actionItem in actions)
            {
                var ruleAction = new RuleAction
                {
                    RuleId = rule.Id,
                    ActionId = actionItem.Id,
                    Order = actionItem.Order
                };
                await context.RuleActions.AddAsync(ruleAction);
            }

            await context.SaveChangesAsync();
            return rule;
        }

        public async Task UpdateRuleWithActionsAsync(Rule rule, List<RuleActionItem> actions)
        {
            using var context = _contextFactory.CreateDbContext();

            rule.UpdatedAt = DateTime.UtcNow;
            context.Rules.Update(rule);

            // Delete old associations
            var oldActions = await context.RuleActions
                .Where(ra => ra.RuleId == rule.Id)
                .ToListAsync();
            context.RuleActions.RemoveRange(oldActions);

            // Add new associations
            foreach (var actionItem in actions)
            {
                var ruleAction = new RuleAction
                {
                    RuleId = rule.Id,
                    ActionId = actionItem.Id,
                    Order = actionItem.Order
                };
                await context.RuleActions.AddAsync(ruleAction);
            }

            await context.SaveChangesAsync();
        }

        public async Task AddAsync(Models.Entities.Action action)
        {
            using var context = _contextFactory.CreateDbContext();
            action.CreatedAt = DateTime.UtcNow;
            await context.Actions.AddAsync(action);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Models.Entities.Action action)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Actions.Update(action);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var action = await GetByIdAsync(id);
            if (action != null)
            {
                context.Actions.Remove(action);
                await context.SaveChangesAsync();
            }
        }
    }
}

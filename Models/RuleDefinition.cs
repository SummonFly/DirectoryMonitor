using DirectoryMonitor.Models.Actions;
using DirectoryMonitor.Models.Conditions;
using DirectoryMonitor.Models.Enums;

namespace DirectoryMonitor.Models
{
    public class RuleDefinition
    {
        public ConditionNode Condition { get; set; } = new ConditionGroup { Operator = LogicalOperator.And };
        public List<ActionBase> Actions { get; set; } = new();
    }
}

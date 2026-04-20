using DirectoryMonitor.Models.Enums;
using System.IO;



namespace DirectoryMonitor.Models.Conditions
{
    public class ConditionGroup : ConditionNode
    {
        public LogicalOperator Operator { get; set; } = LogicalOperator.And;
        public List<ConditionNode> Children { get; set; } = new();

        public override bool IsMet(FileSystemEventArgs args, FileInfo? fileInfo = null)
        {
            if (Children.Count == 0)
                return true;

            return Operator switch
            {
                LogicalOperator.And => Children.All(c => c.IsMet(args, fileInfo)),
                LogicalOperator.Or => Children.Any(c => c.IsMet(args, fileInfo)),
                LogicalOperator.Not => !Children.First().IsMet(args, fileInfo),
                _ => false
            };
        }
    }
}

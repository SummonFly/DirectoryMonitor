using DirectoryMonitor.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DirectoryMonitor.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public AppDbContext() { }

        public DbSet<WatchedPath> WatchedPaths { get; set; }
        public DbSet<EventLogEntry> EventLogEntries { get; set; }

        public DbSet<Models.Entities.Rule> Rules { get; set; }
        public DbSet<RuleExecutionLogEntry> RuleExecutionLogs { get; set; }

        public DbSet<Models.Entities.Action> Actions { get; set; }
        public DbSet<WatchedPathRule> WatchedPathRules { get; set; }
        public DbSet<RuleAction> RuleActions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventLogEntry>()
                .HasIndex(e => e.Timestamp);

            modelBuilder.Entity<EventLogEntry>()
                .HasIndex(e => e.Path);

            // Convert enums to string
            modelBuilder.Entity<EventLogEntry>()
                .Property(e => e.EventType)
                .HasConversion<string>();

            modelBuilder.Entity<Models.Entities.Rule>()
                .Property(r => r.EventType)
                .HasConversion<string>();

            modelBuilder.Entity<Models.Entities.Action>()
                .Property(a => a.ActionType)
                .HasConversion<string>();

            modelBuilder.Entity<RuleExecutionLogEntry>()
                .Property(r => r.EventType)
                .HasConversion<string>();

            // Indexes for RuleExecutionLog
            modelBuilder.Entity<RuleExecutionLogEntry>()
                .HasIndex(r => r.Timestamp);

            modelBuilder.Entity<RuleExecutionLogEntry>()
                .HasIndex(r => r.RuleId);

            // EventLogEntry -> WatchedPath (set NULL on delete)
            modelBuilder.Entity<EventLogEntry>()
                .HasOne(e => e.WatchedPath)
                .WithMany()
                .HasForeignKey(e => e.WatchedPathId)
                .OnDelete(DeleteBehavior.SetNull);

            // WatchedPathRule configuration
            modelBuilder.Entity<WatchedPathRule>()
                .HasOne(wpr => wpr.WatchedPath)
                .WithMany(wp => wp.WatchedPathRules)
                .HasForeignKey(wpr => wpr.WatchedPathId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WatchedPathRule>()
                .HasOne(wpr => wpr.Rule)
                .WithMany(r => r.WatchedPathRules)
                .HasForeignKey(wpr => wpr.RuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // RuleAction configuration
            modelBuilder.Entity<RuleAction>()
                .HasOne(ra => ra.Rule)
                .WithMany(r => r.RuleActions)
                .HasForeignKey(ra => ra.RuleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RuleAction>()
                .HasOne(ra => ra.Action)
                .WithMany(a => a.RuleActions)
                .HasForeignKey(ra => ra.ActionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RuleAction>()
                .HasIndex(ra => new { ra.RuleId, ra.ActionId })
                .IsUnique();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=monitor.db");
            }
        }

    }
}

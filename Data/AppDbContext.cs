using DirectoryMonitor.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Existing indexes...
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

            modelBuilder.Entity<RuleExecutionLogEntry>()
                .Property(r => r.EventType)
                .HasConversion<string>();

            // Indexes for RuleExecutionLog
            modelBuilder.Entity<RuleExecutionLogEntry>()
                .HasIndex(r => r.Timestamp);

            modelBuilder.Entity<RuleExecutionLogEntry>()
                .HasIndex(r => r.RuleId);
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

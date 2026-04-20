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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=monitor.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventLogEntry>()
                .HasIndex(e => e.Timestamp);

            modelBuilder.Entity<EventLogEntry>()
                .HasIndex(e => e.Path);

            modelBuilder.Entity<EventLogEntry>()
                .HasOne(e => e.WatchedPath)
                .WithMany()
                .HasForeignKey(e => e.WatchedPathId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

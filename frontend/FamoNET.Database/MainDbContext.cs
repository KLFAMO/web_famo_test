using FamoNET.Database.Model;
using FamoNET.Database.Model.Classes;
using Microsoft.EntityFrameworkCore;

namespace FamoNET.Database
{
    public class MainDbContext : DbContext
    {
        public DbSet<TerminalCommand> TerminalCommands { get; set; }
        public DbSet<DDSDevice> DDSDevices { get; set; }
        public DbSet<DDSChannel> DDSChannels { get; set; }
        public DbSet<RemoteChartFile> RemoteChartsFiles { get; set; }

        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
        {


        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TerminalCommand>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Request).IsRequired();
                entity.Property(e => e.IP).IsRequired();
                entity.Property(e => e.CreatedOn).IsRequired();
                entity.Property(e => e.ResponseType).IsRequired();
                entity.Property(e => e.State).HasDefaultValue((int)DbRecordState.Enabled);
            });

            modelBuilder.Entity<DDSChannel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IsLocked).IsRequired();                
                entity.Property(e => e.CreatedOn).IsRequired();
                entity.Property(e => e.State).HasDefaultValue((int)DbRecordState.Enabled);
                entity.HasOne(e => e.Device);
            });

            modelBuilder.Entity<DDSDevice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IsLocked).IsRequired();
                entity.Property(e => e.ApiDeviceId).IsRequired();
                entity.Property(e => e.CreatedOn).IsRequired();                
                entity.Property(e => e.State).HasDefaultValue((int)DbRecordState.Enabled);
                entity.HasMany(e => e.Channels);
            });

            modelBuilder.Entity<RemoteChartFile>(entity => 
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ApiDeviceId).IsRequired();
                entity.Property(e => e.CreatedOn).IsRequired();
                entity.Property(e => e.State).HasDefaultValue((int)DbRecordState.Enabled);
            });
        }
    }
}

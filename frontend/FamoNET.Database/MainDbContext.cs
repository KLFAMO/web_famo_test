using FamoNET.Database.Model;
using FamoNET.Database.Model.Classes;
using Microsoft.EntityFrameworkCore;

namespace FamoNET.Database
{
    public class MainDbContext : DbContext
    {
        public DbSet<TerminalCommand> TerminalCommands { get; set; }

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
        }
    }
}

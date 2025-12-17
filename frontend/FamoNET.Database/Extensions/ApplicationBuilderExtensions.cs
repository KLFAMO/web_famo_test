using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NLog;

namespace FamoNET.Database.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public static IHost CheckDatabase(this IHost host, IDbContextFactory<MainDbContext> dbContextFactory)
        {
            using var context = dbContextFactory.CreateDbContext();            

            try
            {
                context.Database.Migrate();
                _logger.Info("Database migrated");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to migrate databse");
            }

            return host;
        }         
    }
}

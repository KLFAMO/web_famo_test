namespace FamoNET.Database.Repositories.Implementations
{
    public abstract class RepositoryBase
    {
        protected MainDbContext Context;
        protected RepositoryBase(MainDbContext mainDbContext)
        {
            Context = mainDbContext;
        }
    }
}

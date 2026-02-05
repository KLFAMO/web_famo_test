using FamoNET.Database.Model.Classes;
using FamoNET.Database.Model.Interfaces;
using System.Threading.Channels;

namespace FamoNET.Database.Repositories.Implementations
{
    public class DDSChannelRepository : RepositoryBase, IDDSChannelsRepository
    {
        public DDSChannelRepository(MainDbContext mainDbContext) : base(mainDbContext)
        {
        }

        public Task<DDSChannel> GetSingleAsync(string name, int deviceId)
        {
            return Task.FromResult(Context.DDSChannels.SingleOrDefault(c => c.Device.Id == deviceId && c.Name == name));
        }

        public Task<List<DDSChannel>> GetByDeviceIdAsync(int deviceId)
        {
            return Task.FromResult(Context.DDSChannels.Where(c => c.Device.Id == deviceId).ToList());
        }

        public async Task<int> InsertAsync(DDSChannel channel)
        {
            channel.CreatedOn = DateTime.Now;

            Context.DDSChannels.Add(channel);
            await Context.SaveChangesAsync();

            return channel.Id;
        }

        public async Task LockChannel(int channelId)
        {
            var exisitngChannel = Context.DDSChannels.FirstOrDefault(d => d.Id == channelId);
            if (exisitngChannel == null)
                return;

            exisitngChannel.IsLocked = true;
            Context.Update(exisitngChannel);
            await Context.SaveChangesAsync();
        }

        public async Task UnlockChannel(int channelId)
        {
            var exisitngChannel = Context.DDSChannels.FirstOrDefault(d => d.Id == channelId);
            if (exisitngChannel == null)
                return;

            exisitngChannel.IsLocked = false;
            Context.Update(exisitngChannel);
            await Context.SaveChangesAsync();
        }
    }
}

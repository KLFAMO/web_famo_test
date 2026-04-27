using FamoNET.Database.Model.Classes;
using FamoNET.Database.Model.Interfaces;

namespace FamoNET.Database.Repositories.Implementations
{
    public class DDSDevicesRepository : RepositoryBase, IDDSDevicesRepository
    {
        public DDSDevicesRepository(MainDbContext mainDbContext) : base(mainDbContext)
        {
        }

        public Task<DDSDevice> GetByApiDeviceIdAsync(int deviceId)
        {
            //var channels = Context.DDSChannels.Where(c => c.Device.Id == deviceId);
            var device = Context.DDSDevices.Where(d => d.ApiDeviceId == deviceId).FirstOrDefault();
            //device.Channels = channels.ToList();
            
            return Task.FromResult(device);
        }

        public async Task<int> InsertAsync(DDSDevice device)
        {
            device.CreatedOn = DateTime.Now;

            Context.DDSDevices.Add(device);
            await Context.SaveChangesAsync();

            return device.Id;
        }

        public async Task LockDevice(int deviceId)
        {
            var device = Context.DDSDevices.Where(d => d.Id == deviceId).FirstOrDefault();

            if (device == null)
            {
                throw new InvalidDataException("Device not found");
            }

            device.IsLocked = true;
            Context.DDSDevices.Update(device);
            await Context.SaveChangesAsync();
        }

        public async Task UnlockDevice(int deviceId)
        {
            var device = Context.DDSDevices.Where(d => d.Id == deviceId).FirstOrDefault();

            if (device == null)
            {
                throw new InvalidDataException("Device not found");
            }

            device.IsLocked = false;
            Context.DDSDevices.Update(device);
            await Context.SaveChangesAsync();
        }
    }
}

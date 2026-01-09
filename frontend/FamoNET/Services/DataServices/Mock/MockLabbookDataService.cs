using FamoNET.Model;
using FamoNET.Model.Interfaces;

namespace FamoNET.Services.DataServices.Mock
{
    public class MockLabbookDataService : ILabbookDataService
    {
        private List<LabbookEntry> _entries = new List<LabbookEntry>()
        {
            new LabbookEntry()
                {
                    Id = 1,
                    Message = "Testowy message 1",
                    StartMjd = 10,
                    EndMjd = 12,
                    Tag = "tag1"
                },
                new LabbookEntry()
                {
                    Id = 2,
                    Message = "Testowy message 2",
                    StartMjd = 10,
                    EndMjd = 12,
                    Tag = "tag2"
                },
                new LabbookEntry()
                {
                    Id = 3,
                    Message = "Testowy message 3",
                    StartMjd = 10,
                    EndMjd = 12,
                    Tag = "tag3"
                },
                new LabbookEntry()
                {
                    Id = 4,
                    Message = "Child 1 wpisu 1",
                    StartMjd = 10,
                    EndMjd = 12,
                    ParentId = 1,
                    Tag = "tag3"
                },
                new LabbookEntry()
                {
                    Id = 5,
                    Message = "Child 1 childa 1 wpisu 1",
                    StartMjd = 10,
                    EndMjd = 12,
                    ParentId = 4,
                    Tag = "tag3"
                }
        };

        public Task<LabbookEntry> GetByIdAsync(int id)
        {
            return Task.FromResult(_entries.FirstOrDefault(x => x.Id == id));
        }

        public async Task<List<LabbookEntry>> GetEntriesAsync(double from, double to)
        {
            await Task.Delay(3000);
            if (from == 0 && to == 0)
            {
                return _entries;
            }

            //return _entries.Where(e => e.StartMjd >= from && e.EndMjd <= to).ToList();
            return _entries;
        }

        public Task<bool> PostAsync(LabbookEntry entry)
        {
            _entries.Add(entry);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateAsync(LabbookEntry entry)
        {
            var entryIndex = _entries.FindIndex(e => e.Id == entry.Id);
            if (entryIndex > -1)
            {
                _entries[entryIndex] = entry;
            }

            return Task.FromResult(true);
        }
    }
}

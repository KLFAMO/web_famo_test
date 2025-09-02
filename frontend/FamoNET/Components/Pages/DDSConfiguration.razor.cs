using FamoNET.Components.SubComponents;
using FamoNET.Model;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.Pages
{
    public partial class DDSConfiguration : ComponentBase
    {
        public void LoadDDS(DDS dds)
        {
            DDSConfigurationPanel.LoadModel(dds);
        }

        private DDSConfigurationPanel DDSConfigurationPanel { get; set; }
        public List<DDS> DDSs { get; set; } = new List<DDS>()
        {
            new DDS()
            {
                Id = 1,
                Description = "Jakiś DDS 1",
                Name = "DDS1",
                IP = "192.168.3.1",
                Channels = new List<DDSChannel>
                {
                    new DDSChannel()
                    {
                        Id = 1,
                        Name = "Kanał 1",
                        Amplitude = 1000,
                        Phase = 30
                    }
                }
            },
            new DDS()
            {
                Id = 1,
                Description = "Jakiś DDS 2",
                IP = "192.168.3.2",
                Name = "DDS2",
                Channels = new List<DDSChannel>
                {
                    new DDSChannel()
                    {
                        Id = 1,
                        Name = "Kanał 1",
                        Amplitude = 1000,
                        Phase = 30
                    }
                }
            },
            new DDS()
            {
                Id = 1,
                Description = "Jakiś DDS 3",
                Name = "DDS3",
                IP = "192.168.3.3",
                Channels = new List<DDSChannel>
                {
                    new DDSChannel()
                    {
                        Id = 1,
                        Name = "Kanał 1",
                        Amplitude = 1000,
                        Phase = 30
                    }
                }
            },
            new DDS()
            {
                Id = 1,
                Description = "Jakiś DDS 4",
                Name = "DDS4",
                IP = "192.168.3.4",
                Channels = new List<DDSChannel>
                {
                    new DDSChannel()
                    {
                        Id = 1,
                        Name = "Kanał 1",
                        Amplitude = 1000,
                        Phase = 30
                    }
                }
            },
            new DDS()
            {
                Id = 1,
                Description = "Jakiś DDS 5",
                Name = "DDS5",
                IP = "192.168.3.5",
                Channels = new List<DDSChannel>
                {
                    new DDSChannel()
                    {
                        Id = 1,
                        Name = "Kanał 1",
                        Amplitude = 1000,
                        Phase = 30
                    }
                }
            }

        };
    }
}

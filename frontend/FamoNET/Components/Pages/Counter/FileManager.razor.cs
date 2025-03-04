using FamoNET.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NLog;

namespace FamoNET.Components.Pages.Counter
{
    public partial class FileManager : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        [Inject]
        private IJSRuntime _jsRuntime { get; set; }

        public List<FileStatus> Files { get; } = new List<FileStatus>();

        public FileManager()
        {
            if (!Directory.Exists("Data"))
                return;

            foreach (var file in Directory.GetFiles("Data"))
            {
                try
                {
                    using (Stream stream = new FileStream(file, FileMode.Open))
                    {
                        //file was opened, so it's not busy
                    }

                    Files.Add(new FileStatus() { Name = file, Status = Model.Enums.FileStatus.Completed });
                }
                catch
                {
                    //file is busy
                    Files.Add(new FileStatus() { Name = file, Status = Model.Enums.FileStatus.InProgress });
                }
            }
        }

        public void AddFile(FileStatus fileStatus)
        {
            Files.Add(fileStatus);
            StateHasChanged();
        }

        public void CompleteFile()
        {
            var file = Files.FirstOrDefault(f => f.Status == Model.Enums.FileStatus.InProgress);
            if (file == null)
            {
                _logger.Error("Requested file could not be completed (not found)");
                return;
            }

            file.Status = Model.Enums.FileStatus.Completed;
            StateHasChanged();
        }

        public async Task DownloadFile(FileStatus file)
        {
            if (File.Exists(file.Name))
            {
                var fileStream = File.OpenRead(file.Name);
                using var streamRef = new DotNetStreamReference(stream: fileStream);
                await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", file.Name, streamRef);
            }
        }
    }
}

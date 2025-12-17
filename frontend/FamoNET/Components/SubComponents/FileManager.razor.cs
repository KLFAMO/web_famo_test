using FamoNET.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NLog;

namespace FamoNET.Components.SubComponents
{
    public partial class FileManager : ComponentBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        [Inject]
        private IJSRuntime _jsRuntime { get; set; }      

        public List<FileStatus> Files { get; private set; } = new List<FileStatus>();
        
        public string DataPath { get; private set; } 

        public async Task Initialize(string path)
        {
            DataPath = path;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);                
                return;
            }

            await Reload();
        }

        public async Task Reload()
        {
            Files.Clear();
            foreach (var file in Directory.GetFiles(DataPath))
            {
                try
                {
                    string description = String.Empty;
                    using (StreamReader stream = new StreamReader(file))
                    {
                        description = stream.ReadLine();
                    }

                    Files.Add(new FileStatus() { Name = file, Description = description, Status = ProgressStatus.Completed });
                }
                catch
                {
                    //file is busy
                    Files.Add(new FileStatus() { Name = file, Status = ProgressStatus.InProgress });
                }
            }
            Files = Files.OrderBy(f => f.Name).ToList();
            await InvokeAsync(StateHasChanged).ConfigureAwait(false);
        }
        
        public void AddFile(FileStatus fileStatus)
        {
            Files.Add(fileStatus);
            StateHasChanged();
        }

        public void CompleteFile()
        {
            var file = Files.FirstOrDefault((Func<FileStatus, bool>)(f => f.Status == ProgressStatus.InProgress));
            if (file == null)
            {
                _logger.Error("Requested file could not be completed (not found)");
                return;
            }

            file.Status = ProgressStatus.Completed;
            StateHasChanged();
        }

        public async Task DownloadFile(FileStatus file)
        {
            if (File.Exists(Path.Combine(DataPath, file.Name)))
            {
                var fileStream = File.OpenRead(file.Name);
                using var streamRef = new DotNetStreamReference(stream: fileStream);
                await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", Path.GetFileName(file.Name), streamRef);
            }
        }
    }
}

using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Services;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents.Wizards
{
    public partial class FileWizard : ComponentBase, IDisposable
    {
        [Inject]
        private RemoteChartsWriterService _writerService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _writerService.RequestWizard += WriterService_RequestWizard;
        }

        private void WriterService_RequestWizard(object sender, GenericEventArgs<(string dataFolder, List<DataPoint<double>> points)> e)
        {
            _dataPath = e.Value.dataFolder;
            _points = e.Value.points;
            IsVisible = true;
            StateHasChanged();
        }

        public FileStatus Model { get; set; } = new FileStatus();
        public bool IsVisible { get; set; } = false;
        public bool IncludeParameters { get; set; } = false;
        public bool IncludeUnits { get; set; } = false;        
        public bool IncludeDescription { get; set; } = false;        

        private string _dataPath;
        private List<DataPoint<double>> _points;
        private void CloseModal()
        {
            IsVisible = false;
            StateHasChanged();
        }        

        private void SaveFile()
        {            
            string filename = string.IsNullOrWhiteSpace(Model.Name) ? DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") : Model.Name;
            Model.Name = filename;
            _writerService.WriteFile(Model, _dataPath, _points);
            
            IsVisible = false;            
            StateHasChanged();
        }

        public void Dispose()
        {
            _writerService.RequestWizard -= WriterService_RequestWizard;
        }
    }
}

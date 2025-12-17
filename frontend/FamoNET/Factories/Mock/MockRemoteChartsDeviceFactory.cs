using FamoNET.Controllers.Mock;
using FamoNET.Model.Interfaces;

namespace FamoNET.Factories.Mock
{
    public class MockRemoteChartsDeviceFactory : IRemoteChartsDeviceFactory
    {
        public Task<ISpectrumAnalyzerController> Create(string ip, int port)
        {
            var controller = new MockSpectrumAnalyzerController();
            controller.Connect(ip, port);

            return Task.FromResult(controller as ISpectrumAnalyzerController);
        }

        public void Dispose()
        {
            
        }
    }
}

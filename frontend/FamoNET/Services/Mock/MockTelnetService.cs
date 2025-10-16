using FamoNET.Model.Interfaces;

namespace FamoNET.Services.Mock
{
    public class MockTelnetService : ITelnetService
    {
        public async Task Send(string ip, string message)
        {
            await Task.Delay(1000);            
        }                    
    }
}

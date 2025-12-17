using FamoNET.Model;
using FamoNET.Model.Args;
using FamoNET.Model.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FamoNET.Components.SubComponents
{
    public partial class SystemMessageComponent : ComponentBase, IDisposable
    {
        private Dictionary<System.Timers.Timer, SystemMessage> _timers = new Dictionary<System.Timers.Timer, SystemMessage>();
        [Inject]
        private ISystemNotificationService _notificationService { get; set; }
        protected override Task OnParametersSetAsync()
        {
            _notificationService.SystemMessageReceived += OnSystemMessageReceived;
            return base.OnParametersSetAsync();
        }

        private void OnSystemMessageReceived(object sender, GenericEventArgs<SystemMessage> e)
        {
            if (e.Value == null)
                return;

            Messages.Add(e.Value);
            InvokeAsync(() => StateHasChanged());

            var timer = new System.Timers.Timer(3000);
            timer.Elapsed += (sender, e) =>
            {
                ((System.Timers.Timer)sender).Dispose();
                if (!_timers.TryGetValue((System.Timers.Timer)sender, out var message))
                    return;

                if (Messages.Contains(message))
                {
                    Messages.Remove(message);
                    InvokeAsync(() => StateHasChanged());
                }
                _timers.Remove((System.Timers.Timer)sender);               
            };
            timer.AutoReset = false;
            timer.Start();
            _timers.Add(timer, e.Value);
        }

        public void Dispose()
        {
            foreach (var pair in _timers)
                pair.Key?.Dispose();
        }

        private List<SystemMessage> _messages = new List<SystemMessage>();

        public List<SystemMessage> Messages
        {
            get => _messages;
            set
            {
                if (_messages == value)
                    return;

                _messages = value;
                InvokeAsync(() => StateHasChanged());
            }
        }
    }
}

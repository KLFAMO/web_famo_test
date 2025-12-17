using FamoNET.Model;
using FamoNET.Model.Interfaces;
using NLog;
using System.Collections.Concurrent;

namespace FamoNET.Services
{
    public class RemoteDevicesDataService : IDisposable
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private ConcurrentDictionary<string, ISpectrumAnalyzerController> _controllers = new();
        private ConcurrentDictionary<Guid, RemoteChartsSubscriptionInfo> _subscriptions = new();
        private SemaphoreSlim _subscriptionSemaphore = new SemaphoreSlim(1);

        private Task GuardTask;

        private readonly IRemoteChartsDeviceFactory _remoteChartsDeviceFactory;
        public RemoteDevicesDataService(IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                _remoteChartsDeviceFactory = scope.ServiceProvider.GetRequiredService<IRemoteChartsDeviceFactory>();
            }

            GuardTask = Task.Run(SubscriptionGuard);
        }

        public async Task<bool> Subscribe(RemoteChartsSubscriptionInfo remoteChartsSubscriptionInfo)
        {
            try
            {                            
                await _subscriptionSemaphore.WaitAsync().ConfigureAwait(false);                
                remoteChartsSubscriptionInfo.LastFetch = DateTime.UtcNow;

                if (!_subscriptions.TryAdd(remoteChartsSubscriptionInfo.SessionId, remoteChartsSubscriptionInfo))
                {
                    _logger.Warn($"Session already exists in subscriptions");

                }
                _logger.Debug($"Registered subsription {remoteChartsSubscriptionInfo.SessionId}");
                if (!_controllers.TryGetValue(remoteChartsSubscriptionInfo.IP, out var controller))
                {                    
                    controller = await _remoteChartsDeviceFactory.Create(remoteChartsSubscriptionInfo.IP, remoteChartsSubscriptionInfo.Port);
                    if (!_controllers.TryAdd(remoteChartsSubscriptionInfo.IP, controller))
                    {
                        _logger.Warn($"Failed to add controller {remoteChartsSubscriptionInfo.IP}");
                    }
                    
                }
                
                AdjustRate(remoteChartsSubscriptionInfo.IP);                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
            finally
            {                
                _subscriptionSemaphore.Release();
            }
        }

        public async Task SetRefreshRate(RemoteChartsSubscriptionInfo remoteChartsSubscriptionInfo)
        {
            try
            {
                await _subscriptionSemaphore.WaitAsync().ConfigureAwait(false);
                if (_controllers.TryGetValue(remoteChartsSubscriptionInfo.IP, out var controller))
                {
                    if (controller.RefreshRate > remoteChartsSubscriptionInfo.Rate)
                    {
                        controller.SetRefreshRate(remoteChartsSubscriptionInfo.Rate);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                _subscriptionSemaphore.Release();
            }
        }

        public async Task Unsubscribe(RemoteChartsSubscriptionInfo remoteChartsSubscriptionInfo)
        {
            try
            {
                await _subscriptionSemaphore.WaitAsync().ConfigureAwait(false);

                if (!_subscriptions.Remove(remoteChartsSubscriptionInfo.SessionId, out var subscription))
                {
                    _logger.Warn($"Can't delete session - not found {remoteChartsSubscriptionInfo.SessionId}");
                }

                OnSubscriptionRemove(remoteChartsSubscriptionInfo);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                _subscriptionSemaphore.Release();
                _logger.Debug("Unsubscribed");
            }
        }

        public Task<SpectrumAnalyzerParameters> GetData(RemoteChartsSubscriptionInfo remoteChartsSubscriptionInfo)
        {
            try
            {                
                if (!_controllers.TryGetValue(remoteChartsSubscriptionInfo.IP, out var controller))
                    throw new KeyNotFoundException("Controller not registered");

                return Task.FromResult(new SpectrumAnalyzerParameters(controller.Data));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }
        
        public async Task SetParameters(RemoteChartsSubscriptionInfo remoteChartsSubscriptionInfo, SpectrumAnalyzerParameters parameters)
        {
            try
            {                                
                if (!_controllers.TryGetValue(remoteChartsSubscriptionInfo.IP, out var controller))
                    throw new KeyNotFoundException("Controller not registered");

                await controller.SetParameters(parameters);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }

        public void SetRate(RemoteChartsSubscriptionInfo remoteChartsSubscriptionInfo, double rate)
        {
            if (!_controllers.TryGetValue(remoteChartsSubscriptionInfo.IP, out var controller))
                throw new KeyNotFoundException("Controller not registered");

            if (rate > controller.RefreshRate)
                controller.SetRefreshRate(rate);
        }

        private void AdjustRate(string IP)
        {
            if (string.IsNullOrWhiteSpace(IP))
            {
                _logger.Warn("Passed empty IP");
                return;
            }

            var maxRate = _subscriptions.Values
                       .Where(k => k.IP == IP)
                       .Select(k => k.Rate).Min();

            if (_controllers.TryGetValue(IP, out var controller))
            {
                if (controller.RefreshRate != maxRate)
                {
                    controller.SetRefreshRate(maxRate);
                }
            }            
        }

        public void Ping(RemoteChartsSubscriptionInfo remoteChartsSubscriptionInfo)
        {
            if (_subscriptions.TryGetValue(remoteChartsSubscriptionInfo.SessionId, out var subscription))
            {
                subscription.LastFetch = DateTime.UtcNow;
            }
            else
            {
                _logger.Warn($"Failed to retrieve subscription. {remoteChartsSubscriptionInfo.SessionId}");
                throw new Exception($"Subscription {remoteChartsSubscriptionInfo.SessionId} not found. Subscribe first");
            }
        }

        private async Task SubscriptionGuard()
        {
            _logger.Debug("Subscription guard fired");
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        foreach (var subscriptionPair in _subscriptions)
                        {
                            var now = DateTime.UtcNow;
                            TimeSpan diff = now - subscriptionPair.Value.LastFetch;
                            if (diff.TotalSeconds > 10)
                            {
                                try
                                {
                                    await _subscriptionSemaphore.WaitAsync().ConfigureAwait(false);
                                    _subscriptions.Remove(subscriptionPair.Key, out _);
                                    OnSubscriptionRemove(subscriptionPair.Value);
                                    _logger.Debug($"Removed by guard {subscriptionPair.Value.SessionId}");
                                }
                                finally
                                {
                                    _subscriptionSemaphore.Release();
                                }                                
                            }                            
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.Error(ex);
                    }
                    finally
                    {
                        await Task.Delay(10000).ConfigureAwait(false);
                    }
                }
            }
            catch(Exception ex)
            {
                if (ex is TaskCanceledException)
                    return;

                _logger.Error(ex);
            }
        }

        private void OnSubscriptionRemove(RemoteChartsSubscriptionInfo remoteChartsSubscriptionInfo)
        {
            if (_subscriptions.Values.Where(s => s.IP == remoteChartsSubscriptionInfo.IP).FirstOrDefault() == null) //check if there are another subscriptions that use this IP
            {
                if (_controllers.Remove(remoteChartsSubscriptionInfo.IP, out var controller)) //remove controller if that was the last subscription
                {
                    controller.Dispose();
                }
            }
            else //check read rate
            {
                AdjustRate(remoteChartsSubscriptionInfo.IP);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _subscriptionSemaphore.Dispose();
        }
    }
}

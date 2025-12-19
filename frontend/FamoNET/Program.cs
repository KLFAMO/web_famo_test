using FamoNET.Components;
using FamoNET.Controllers.Mock;
using FamoNET.Database;
using FamoNET.Database.Extensions;
using FamoNET.Database.Model.Interfaces;
using FamoNET.Database.Repositories.Implementations;
using FamoNET.DataProviders;
using FamoNET.DataProviders.Mock;
using FamoNET.Factories;
using FamoNET.Model;
using FamoNET.Model.Interfaces;
using FamoNET.Services;
using FamoNET.Services.DataServices;
using FamoNET.Services.DataServices.Mock;
using FamoNET.Services.Mock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace FamoNET
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.Configure<EndpointsOptions>(builder.Configuration.GetSection(EndpointsOptions.SectionName));
            
            builder.Services.AddScoped<ISystemNotificationService>((s) => new SystemNotificationService());
            builder.Services.AddScoped((s) => new TimeService());
            builder.Services.AddScoped<RemoteChartsWriterService>();
            builder.Services.AddScoped((s) => new ChartManagerService(s.GetRequiredService<IJSRuntime>()));
            builder.Services.AddSingleton((s) => new CounterWriterService());            
            builder.Services.AddSingleton((s) => new CounterDataService(s.GetService<IOptions<EndpointsOptions>>().Value.FXMCounterUri));            
            builder.Services.AddScoped<ICSVDataProvider>((s) => new MockAndaDataProvider(@"TestData\data_export(5).csv"));
            builder.Services.AddScoped<ITerminalCommandsRepository, TerminalCommandsRepository>();

#if (!DEBUG)
            builder.Services.AddScoped<IAndaDataProvider>((s) => new AndaDataProvider(s.GetService<IOptions<EndpointsOptions>>().Value.AndaUri));
            builder.Services.AddSingleton<IFreqMonitorDataService>((s) => new FreqMonitorDataService(s.GetService<IOptions<EndpointsOptions>>().Value.FreqMonitorUri));            
            builder.Services.AddScoped<IDevicesDataService>((s) => new DevicesDataService(s.GetService<IOptions<EndpointsOptions>>().Value.DevicesUri));
            builder.Services.AddScoped<ILabbookDataService>((s) => new LabbookDataService(s.GetService<IOptions<EndpointsOptions>>().Value.LabbookUri));
            builder.Services.AddScoped<ITelnetService>((s) => new TelnetService(s.GetService<IOptions<EndpointsOptions>>().Value.TelnetUri, s.GetService<ITerminalCommandsRepository>()));            
            builder.Services.AddScoped<IRemoteChartsDeviceFactory>((s) => new RemoteChartsDeviceFactory());
            builder.Services.AddScoped<IDDSDataService>((s) => new DDSDataService(s.GetService<IOptions<EndpointsOptions>>().Value.ElementsUri, s.GetRequiredService<IDevicesDataService>()));
#else
            builder.Services.AddScoped<IAndaDataProvider>((s) => new MockAndaDataProvider(@"TestData\data_export(5).csv"));
            builder.Services.AddSingleton<IFreqMonitorDataService>((s) => new MockFreqMonitorDataService());
            builder.Services.AddTransient<ISpectrumAnalyzerController>((s) => new MockSpectrumAnalyzerController());
            builder.Services.AddScoped<ILabbookDataService>((s) => new MockLabbookDataService());
            builder.Services.AddScoped<IDevicesDataService>((s) => new MockDevicesDataService());
            builder.Services.AddScoped<ITelnetService, MockTelnetService>();
            builder.Services.AddScoped<IRemoteChartsDeviceFactory>((s) => new RemoteChartsDeviceFactory());
            builder.Services.AddScoped<IDDSDataService>((s) => new MockDDSDataService(s.GetRequiredService<IDevicesDataService>()));
            //builder.Services.AddScoped<IRemoteChartsDeviceFactory>((s) => new MockRemoteChartsDeviceFactory());
#endif

            builder.Services.AddScoped((s) => new AndaDataService(s.GetService<IAndaDataProvider>()));            
            builder.Services.AddSingleton((s) => new RemoteDevicesDataService(s));

            var connectionString = builder.Configuration.GetConnectionString("MysqlConnection");
            builder.Services.AddDbContextFactory<MainDbContext>(options => {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            //builder.Logging.ClearProviders();
            //builder.Logging.SetMinimumLevel(LogLevel.Trace);
            //builder.Host.UseNLog();
            
            var app = builder.Build();
            app.CheckDatabase(app.Services.GetRequiredService<IDbContextFactory<MainDbContext>>());

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            var counterDataService = app.Services.GetService<CounterDataService>();
            var writerService = app.Services.GetService<CounterWriterService>();
            app.Run();
        }
    }
}

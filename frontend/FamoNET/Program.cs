using FamoNET.Components;
using FamoNET.Controllers;
using FamoNET.Controllers.Mock;
using FamoNET.DataProviders;
using FamoNET.DataProviders.Mock;
using FamoNET.Model;
using FamoNET.Model.Interfaces;
using FamoNET.Services;
using FamoNET.Services.DataServices;
using FamoNET.Services.DataServices.Mock;
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
            builder.Services.AddScoped((s) => new ChartManagerService(s.GetRequiredService<IJSRuntime>()));
            builder.Services.AddSingleton((s) => new CounterWriterService());
            builder.Services.AddSingleton((s) => new CounterDataService(s.GetService<IOptions<EndpointsOptions>>().Value.FXMCounterUri));            
            builder.Services.AddScoped<ICSVDataProvider>((s) => new MockAndaDataProvider(@"TestData\data_export(5).csv"));

#if (!DEBUG)
            builder.Services.AddScoped<IAndaDataProvider>((s) => new AndaDataProvider(s.GetService<IOptions<EndpointsOptions>>().Value.AndaUri));
            builder.Services.AddSingleton<IFreqMonitorDataService>((s) => new FreqMonitorDataService(s.GetService<IOptions<EndpointsOptions>>().Value.FreqMonitorUri));
            builder.Services.AddTransient<IFC1000Controller>((s) => new FC1000Controller());
            builder.Services.AddScoped<IDevicesDataService>((s) => new DevicesDataService(s.GetService<IOptions<EndpointsOptions>>().Value.DevicesUri));
#else
            builder.Services.AddScoped<IAndaDataProvider>((s) => new MockAndaDataProvider(@"TestData\data_export(5).csv"));
            builder.Services.AddSingleton<IFreqMonitorDataService>((s) => new MockFreqMonitorDataService());
            builder.Services.AddTransient<IFC1000Controller>((s) => new MockFC1000Controller());
            builder.Services.AddScoped<IDevicesDataService>((s) => new MockDevicesDataService());
#endif

            builder.Services.AddScoped((s) => new AndaDataService(s.GetService<IAndaDataProvider>()));

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            //builder.Logging.ClearProviders();
            //builder.Logging.SetMinimumLevel(LogLevel.Trace);
            //builder.Host.UseNLog();
            
            var app = builder.Build();

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

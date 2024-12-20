using FamoNET.Components;
using FamoNET.DataProviders;
using FamoNET.Mock;
using FamoNET.Model;
using FamoNET.Model.Interfaces;
using FamoNET.Services;
using FamoNET.Services.DataServices;
using Microsoft.Extensions.Options;

namespace FamoNET
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.Configure<EndpointsOptions>(builder.Configuration.GetSection(EndpointsOptions.SectionName));

            builder.Services.AddScoped((s) => new TimeService());
            builder.Services.AddSingleton((s) => new CounterWriterService());
            builder.Services.AddSingleton((s) => new CounterDataService(s.GetService<IOptions<EndpointsOptions>>().Value.FXMCounterUri));            
            builder.Services.AddScoped<ICSVDataProvider>((s) => new MockDataProvider(@"TestData\data_export(5).csv"));

#if (!DEBUG)
            builder.Services.AddScoped<IAndaDataProvider>((s) => new AndaDataProvider(s.GetService<IOptions<EndpointsOptions>>().Value.AndaUri));
            builder.Services.AddSingleton<IFreqMonitorDataService>((s) => new FreqMonitorDataService(s.GetService<IOptions<EndpointsOptions>>().Value.FreqMonitorUri));
#else
            builder.Services.AddScoped<IAndaDataProvider>((s) => new MockDataProvider(@"TestData\data_export(5).csv"));
            builder.Services.AddSingleton<IFreqMonitorDataService>((s) => new MockFreqMonitorDataService());
#endif

            builder.Services.AddScoped((s) => new AndaDataService(s.GetService<IAndaDataProvider>()));

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

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

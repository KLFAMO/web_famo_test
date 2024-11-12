using FamoNET.Components;
using FamoNET.DataProviders;
using FamoNET.Model.Interfaces;
using FamoNET.Services.DataServices;

namespace FamoNET
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<MockDataProvider>((s) => new MockDataProvider(@"TestData\data_export(5).csv"));
            builder.Services.AddScoped<ICounterDataProvider>((s) => s.GetService<MockDataProvider>());
            builder.Services.AddScoped<ICSVDataProvider>((s) => s.GetService<MockDataProvider>());
            builder.Services.AddScoped<CounterDataService>((s) => new CounterDataService(s.GetService<ICounterDataProvider>()));

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

            app.Run();
        }
    }
}

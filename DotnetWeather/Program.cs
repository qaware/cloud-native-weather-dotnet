using DotnetWeather.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetWeather;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<DotnetWeatherContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"), 
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 15,
                    maxRetryDelay: TimeSpan.FromSeconds(30), 
                    errorNumbersToAdd: new List<int>{}
                )
            )
        );

        builder.Services.AddScoped<WeatherService>();

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<DotnetWeatherContext>();
            context.Database.EnsureCreated();
        }

        WeatherService.OpenWeatherConnector = new OpenWeatherConnector(app.Configuration);
        WeatherService.WeatherContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<DotnetWeatherContext>();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
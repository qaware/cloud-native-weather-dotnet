using DotnetWeather.Controllers;
using DotnetWeather.Data;
using DotnetWeather.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetWeatherTest;

public static class Utils
{
    public static WeatherController GetWeatherController()
    {
        var context = GetSeededDatabaseContext();
        var connector = new MockOpenWeather();
        var logger = new Logger<WeatherService>(LoggerFactory.Create(builder =>
            builder.AddSystemdConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "hh:mm:ss ";
            })));
        var service = new WeatherService(logger, context, connector);
        var weatherController = new WeatherController(service);
        return weatherController;
    }
    
    public static DotnetWeatherContext GetEmptyDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<DotnetWeatherContext>()
            .UseInMemoryDatabase(databaseName: "MockDatabase")
            .Options;

        var context = new DotnetWeatherContext(options);
        context.Database.EnsureCreated();

        return context;
    }
    
    public static DotnetWeatherContext GetSeededDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<DotnetWeatherContext>()
            .UseInMemoryDatabase(databaseName: "MockDatabase")
            .Options;

        var context = new DotnetWeatherContext(options);
        SeedDatabase(context);

        return context;
    }

    private static void SeedDatabase(DotnetWeatherContext context)
    {
        
        context.Database.EnsureCreated();

        if (context.Weather.Any())
        {
            return;
        }

        var cities = new City[]
        {
            new City {Lat = 48.137154, Lon = 11.576124, Name = "Munich"},
            new City {Lat = 50.073658, Lon = 14.418540, Name = "Prague"},
            new City {Lat = 51.507359, Lon = -0.136439, Name = "London"},
            new City {Lat = 52.531677, Lon = 13.381777, Name = "Berlin"},
            new City {Lat = -33.447487, Lon = -70.673676, Name = "Santiago"},
        };

        foreach (City c in cities)
        {
            context.City.Add(c);
        }
        context.SaveChanges();

        Random random = new Random();
        List<WeatherType> weatherTypes = WeatherType.GetAllWeatherTypes();
        for (int i = 1; i <= cities.Length; i++)
        {
            List<DateTime> dates = Enumerable.Range(1, DateTime.DaysInMonth(2022, 7))
                .Select(day => new DateTime(2022, 7, day)).ToList();
            foreach (var date in dates)
            {
                Weather w = new Weather
                {
                    CityId = i, Date = date, Temperature = random.Next(10, 30),
                    WeatherType = weatherTypes[random.Next(weatherTypes.Count)]
                };
                context.Weather.Add(w);
            }
        }

        context.SaveChanges();
    }
}
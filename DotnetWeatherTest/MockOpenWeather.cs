using DotnetWeather.Data;
using DotnetWeather.Models;

namespace DotnetWeatherTest;

public class MockOpenWeather : IOpenWeatherConnector
{
    public Task<List<Weather>> GetWeather(string lat, string lon)
    {
        Weather w = new Weather{Date = DateTime.Today, Temperature = 25.0, WeatherType = WeatherType.SUNNY};
        return Task.FromResult(new List<Weather> {w});
    }

    public Task<City?> GetCity(string name)
    {
        if (name == "Munich")
        {
            return Task.FromResult<City?>(new City {Name = name});
        }
        else
        {
            return Task.FromResult<City?>(null);
        }
    }
}
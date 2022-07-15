using DotnetWeather.Models;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace DotnetWeather.Data;

public class WeatherService
{
    public static DotnetWeatherContext WeatherContext;
    public static OpenWeatherConnector OpenWeatherConnector;
    
    private Random rnd = new Random();
    private AsyncPolicy _policy;

    public WeatherService()
    {
        var timeoutPolicy = Policy
            .TimeoutAsync(
                TimeSpan.FromSeconds(5), // _settings.TimeoutWhenCallingApi,
                Polly.Timeout.TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (_, __, ___, ____) =>
                {
                    Console.WriteLine("Timeout has occurred");
                    return Task.CompletedTask;
                }
            );
        
        var circuitBreakerPolicy = Policy.Handle<Exception>()
            .CircuitBreakerAsync(3, TimeSpan.FromMinutes(1),
                (ex, t) =>
                {
                    Console.WriteLine("Circuit broken!");
                },
                () =>
                {
                    Console.WriteLine("Circuit Reset!");
                });

        _policy = circuitBreakerPolicy.WrapAsync(timeoutPolicy);
    }

    public async Task<Weather?> GetWeather(City city, DateTime date)
    {
        try
        {
            return await _policy.ExecuteAsync(async () => await _getWeather(city, date));
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
    private async Task<Weather?> _getWeather(City city, DateTime date)
    {
        Weather? weather = await WeatherContext.Weather.FindAsync(city.Id, date);
        if (weather == null)
        {
            var weathers = await OpenWeatherConnector.GetWeather(city.Lat.ToString(), city.Lon.ToString());
            foreach (Weather w in weathers)
            {
                w.CityId = city.Id;
                Weather? foundWeather = await WeatherContext.Weather.FindAsync(w.CityId, w.Date);
                if (foundWeather == null)
                {
                    WeatherContext.Weather.Add(w);
                }
                else
                {
                    foundWeather.Temperature = w.Temperature;
                    foundWeather.WeatherType = w.WeatherType;
                    WeatherContext.Weather.Update(foundWeather);
                }

                if (w.Date == date)
                {
                    weather = w;
                }
            }
            await WeatherContext.SaveChangesAsync();
        }

        return weather;
    }

    public async Task<City?> GetCityFromName(string cityName)
    {
        try
        {
            return await _policy.ExecuteAsync(async () => await _getCityFromName(cityName));
        }
        catch (Exception e)
        {
            return null;
        }
    }
    private async Task<City?> _getCityFromName(string cityName)
    {
        City? city = await WeatherContext.City.FirstOrDefaultAsync(c => c.Name.Equals(cityName));
        
        if (city == null) // if city not in database
        {
            city = await OpenWeatherConnector.GetCity(cityName);
            if (city != null)
            {
                city = WeatherContext.City.Add(city).Entity;
                await WeatherContext.SaveChangesAsync();
            }
            else
            {
                return null;
            }
        }

        return city;
    }

    public async Task<List<City>?> GetAlternatives(string cityName)
    {
        try
        {
            return await _policy.ExecuteAsync(async () => await _getAlternatives(cityName));
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
    private async Task<List<City>> _getAlternatives(string cityName)
    {
        HashSet<City> cities = new HashSet<City>();
        if (await WeatherContext.City.AnyAsync())
        {
            for (int i = 0; i < 6; i++)
            {
                var skip = (int)(rnd.NextDouble() * WeatherContext.City.Count());
                City c = WeatherContext.City.OrderBy(c => c.Name).Skip(skip).First();
                cities.Add(c);
            }
        }

        return new List<City>(cities);
    }
}
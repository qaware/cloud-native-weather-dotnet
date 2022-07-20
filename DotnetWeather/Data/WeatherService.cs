using DotnetWeather.Models;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace DotnetWeather.Data;

public class WeatherService
{
    private DotnetWeatherContext _weatherContext;
    private IOpenWeatherConnector _openWeatherConnector;
    
    private Random rnd = new Random();
    private AsyncPolicy _policy;
    private ILogger<WeatherService> _logger;

    public WeatherService(ILogger<WeatherService> logger, DotnetWeatherContext context, IOpenWeatherConnector connector)
    {
        _logger = logger;
        _weatherContext = context;
        _openWeatherConnector = connector;
        
        var timeoutPolicy = Policy
            .TimeoutAsync(
                TimeSpan.FromSeconds(5), // _settings.TimeoutWhenCallingApi,
                Polly.Timeout.TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (_, __, ___, ____) =>
                {
                    _logger.LogInformation("Timeout has occurred");
                    return Task.CompletedTask;
                }
            );
        
        var circuitBreakerPolicy = Policy.Handle<Exception>()
            .CircuitBreakerAsync(3, TimeSpan.FromMinutes(1),
                (ex, t) =>
                {
                    _logger.LogInformation("Circuit broken!");
                },
                () =>
                {
                    _logger.LogInformation("Circuit Reset!");
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
            _logger.LogError(e, "Exception in GetWeather");
            return null;
        }
    }
    
    private async Task<Weather?> _getWeather(City city, DateTime date)
    {
        Weather? weather = await _weatherContext.Weather.FindAsync(city.Id, date);
        if (weather == null)
        {
            var weathers = await _openWeatherConnector.GetWeather(city.Lat.ToString(), city.Lon.ToString());
            foreach (Weather w in weathers)
            {
                w.CityId = city.Id;
                Weather? foundWeather = await _weatherContext.Weather.FindAsync(w.CityId, w.Date);
                if (foundWeather == null) // no weather for this date
                {
                    _weatherContext.Weather.Add(w);
                }
                else // weather for this date already exists but was updated longer ago
                {
                    foundWeather.Temperature = w.Temperature;
                    foundWeather.WeatherType = w.WeatherType;
                    _weatherContext.Weather.Update(foundWeather);
                }

                if (w.Date == date.Date) // weather is what we want to return
                {
                    weather = w;
                }
                await _weatherContext.SaveChangesAsync();
            }
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
            _logger.LogError(e, "Exception in GetCityFromName");
            return null;
        }
    }
    private async Task<City?> _getCityFromName(string cityName)
    {
        City? city = await _weatherContext.City.FirstOrDefaultAsync(c => c.Name.Equals(cityName));
        
        if (city == null) // if city not in database
        {
            city = await _openWeatherConnector.GetCity(cityName);
            if (city != null)
            {
                city = _weatherContext.City.Add(city).Entity;
                await _weatherContext.SaveChangesAsync();
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
            _logger.LogError(e, "Exception in GetAlternatives");
            return null;
        }
    }
    
    private async Task<List<City>> _getAlternatives(string cityName)
    {
        HashSet<City> cities = new HashSet<City>();
        if (await _weatherContext.City.AnyAsync())
        {
            for (int i = 0; i < 6; i++)
            {
                var skip = (int)(rnd.NextDouble() * _weatherContext.City.Count());
                City c = _weatherContext.City.OrderBy(c => c.Name).Skip(skip).First();
                cities.Add(c);
            }
        }

        return new List<City>(cities);
    }
}
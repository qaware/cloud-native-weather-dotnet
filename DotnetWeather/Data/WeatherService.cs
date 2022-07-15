using DotnetWeather.Models;
using System.Data.Entity.Migrations;

namespace DotnetWeather.Data;

public class WeatherService
{
    public static DotnetWeatherContext WeatherContext;
    public static OpenWeatherConnector OpenWeatherConnector;
    
    private Random rnd = new Random();

    public async Task<Weather?> GetWeather(City city, DateTime date)
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
        City? city = WeatherContext.City.FirstOrDefault(c => c.Name.Equals(cityName));
        
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

    public List<City> GetAlternatives(string cityName)
    {
        HashSet<City> cities = new HashSet<City>();
        if (WeatherContext.City.Any())
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
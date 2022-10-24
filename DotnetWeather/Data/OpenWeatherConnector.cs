using System.Text.Json;
using DotnetWeather.Models;

namespace DotnetWeather.Data;

public class OpenWeatherConnector : IOpenWeatherConnector
{
    private HttpClient client = new HttpClient();
    private string WeatherCallUrl = "https://api.openweathermap.org/data/2.5/forecast";
    private string GeocoderCallUrl = "https://api.openweathermap.org/geo/1.0/direct";
    private string ApiKey = "5b3f51e527ba4ee2ba87940ce9705cb5";
    private readonly IConfiguration _configuration;
    
    public OpenWeatherConnector(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<Weather>> GetWeather(string lat, string lon)
    {
        string url = $"{WeatherCallUrl}?lat={lat}&lon={lon}&units=metric&appid={ApiKey}";
        List<Weather> list = new List<Weather>();
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            Stream json = await response.Content.ReadAsStreamAsync();
            var root = JsonDocument.Parse(json).RootElement;
            foreach (var weatherData in root.GetProperty("list").EnumerateArray())
            {
                Weather w = new Weather();
                DateTime dt = DateTime.UnixEpoch.AddSeconds(weatherData.GetProperty("dt").GetInt32());
                if (dt.Hour ==  15 || (dt.Date == DateTime.Today && list.Count == 0))
                {
                    w.Date = dt.Date;
                    w.Temperature = weatherData.GetProperty("main").GetProperty("temp").GetDouble();
                    w.WeatherType = WeatherTypeFromCode(weatherData.GetProperty("weather").EnumerateArray().First().GetProperty("id").GetInt32());
                    list.Add(w);
                }
            }
        }

        return list;
    }

    private WeatherType WeatherTypeFromCode(int id)
    {
        switch (id)
        {
            case <= 299:
                return WeatherType.STORM;
            case <= 599:
                return WeatherType.RAINING;
            case <= 699:
                return WeatherType.SNOW;
            case <= 799:
                return WeatherType.CLOUDY;
            case <= 800:
                return WeatherType.SUNNY;
            case <= 899:
                return WeatherType.CLOUDY;
            default:
                return WeatherType.CLOUDY;
        }
    }

    public async Task<City?> GetCity(string name)
    {
        string url = $"{GeocoderCallUrl}?q={name}&limit=1&appid={ApiKey}";
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            Stream json = await response.Content.ReadAsStreamAsync();
            var array = JsonDocument.Parse(json).RootElement.EnumerateArray();
            if (!array.Any())
            {
                return null;
            }

            City newcity = new City();
            JsonElement cityValues = array.First();
            newcity.Name = cityValues.GetProperty("name").GetString() ?? "";
            newcity.Lat = cityValues.GetProperty("lat").GetDouble();
            newcity.Lon = cityValues.GetProperty("lon").GetDouble();
            return newcity;
        }

        return null;
    }
}
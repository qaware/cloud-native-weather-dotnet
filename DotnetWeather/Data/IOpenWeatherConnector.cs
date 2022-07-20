using System.Text.Json;
using DotnetWeather.Models;

namespace DotnetWeather.Data;

public interface IOpenWeatherConnector
{
    public Task<List<Weather>> GetWeather(string lat, string lon);

    public Task<City?> GetCity(string name);
}
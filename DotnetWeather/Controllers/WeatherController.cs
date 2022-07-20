using DotnetWeather.Data;
using DotnetWeather.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetWeather.Controllers;
public class WeatherController : Controller
{
    private readonly WeatherService _service;

    public WeatherController(WeatherService service)
    {
        _service = service;
    }
    
    public async Task<IActionResult> Index(string cityName, string date)
    {
        if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, 
                System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime dateT))
        {
            dateT = DateTime.Today;
        }
        if (cityName == "" || cityName == null)
        {
            cityName = "Munich";
        }

        City? city = await _service.GetCityFromName(cityName);
        if (city == null)
        {
            return new RedirectToActionResult("CityNotFound", "Weather", new {city=cityName});
        }

        Weather weather = await _service.GetWeather(city, dateT) ?? Weather.GetNotAvailable(city.Id, dateT);
        return View(new WeatherViewModel {City = city, Weather = weather});
    }
    
    public async Task<IActionResult> CityNotFound(string city)
    {
        List<City>? alternatives = await _service.GetAlternatives(city);
        if (alternatives == null)
        {
            return new RedirectToActionResult("ServiceNotAvailable", "Weather", new{});
        }
        City fake = new City {Name = city, SimilarCities = alternatives};
        return View(fake);
    }
    
    public IActionResult ServiceNotAvailable()
    {
        return View();
    }

    [Route("api/weather")]
    public async Task<string> SimpleApi(string city)
    {
        City? c = await _service.GetCityFromName(city);
        if (c == null)
        {
            return $"City {city} not found!";
        }

        Weather? weather = await _service.GetWeather(c, DateTime.Today);
        if (weather == null)
        {
            return $"Weather for {city} not found!";
        }

        return $"The weather in {city} is {weather.WeatherType.Name} today!";
    }

}
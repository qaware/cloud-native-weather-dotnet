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
        if (cityName == "")
        {
            cityName = "Munich";
        }

        City? city = await _service.GetCityFromName(cityName);
        if (city == null)
        {
            return new RedirectToActionResult("CityNotFound", "Weather", new {city=cityName});
        }

        Weather? weather = await _service.GetWeather(city, dateT) ?? Weather.GetNotAvailable(city.Id, dateT);
        return View(new WeatherViewModel {City = city, Weather = weather});
    }
    
    public IActionResult CityNotFound(string city)
    {
        List<City> alternatives = _service.GetAlternatives(city);
        City fake = new City {Name = city, SimilarCities = alternatives};
        return View(fake);
    }

}
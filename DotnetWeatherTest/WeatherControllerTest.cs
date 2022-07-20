using System.Globalization;
using DotnetWeather.Controllers;
using DotnetWeather.Data;
using DotnetWeather.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using static DotnetWeatherTest.Utils;

namespace DotnetWeatherTest;

[TestClass]
public class WeatherControllerTest
{
    [TestMethod]
    public async Task ValidCityFromDatabase()
    {
        var weatherController = GetWeatherController();
        var result = await weatherController.Index("Munich", "2022-07-14"); // 2022-07-14 seeded in mock database

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult) result;
        Assert.IsNotNull(viewResult.Model);
        Assert.IsInstanceOfType(viewResult.Model, typeof(WeatherViewModel));
        var model = (WeatherViewModel) viewResult.Model;
        Assert.AreEqual(DateTime.ParseExact("2022-07-14", "yyyy-MM-dd", CultureInfo.InvariantCulture), model.Weather.Date);
        Assert.AreEqual("Munich", model.City.Name);
    }

    [TestMethod]
    public async Task InvalidCity()
    {
        var weatherController = GetWeatherController();
        var redirect = await weatherController.Index("invalid city", DateTime.Today.ToString("yyyy-MM-dd"));
        
        Assert.IsInstanceOfType(redirect, typeof(RedirectToActionResult));
        var result = (RedirectToActionResult) redirect;
        
        Assert.AreEqual("CityNotFound", result.ActionName);
    }

    [TestMethod]
    public async Task WeatherNotAvailable()
    {
        var weatherController = GetWeatherController();
        var result = await weatherController.Index("Munich", "2022-08-01"); // database is seeded with weather only for 7/2022 -> MockConnector

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult) result;
        Assert.IsNotNull(viewResult.Model);
        Assert.IsInstanceOfType(viewResult.Model, typeof(WeatherViewModel));
        var model = (WeatherViewModel) viewResult.Model;
        Assert.AreEqual(WeatherType.NOT_AVAILABLE, model.Weather.WeatherType);
    }

    [TestMethod]
    public async Task ShowWeatherTest()
    {
        var weatherController = GetWeatherController();
    }
}
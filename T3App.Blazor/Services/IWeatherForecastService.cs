using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static T3App.Blazor.Pages.FetchData;

namespace T3App.Blazor.Services
{
    public interface IWeatherForecastService
    {
        Task<WeatherForecast[]> GetWeatherForecasts();
    }
}

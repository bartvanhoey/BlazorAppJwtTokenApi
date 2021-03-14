using Blazored.LocalStorage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static T3App.Blazor.Pages.FetchData;

namespace T3App.Blazor.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly HttpClient _httpClient;
        //private readonly ILocalStorageService _localStorage;

        public WeatherForecastService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherForecast[]> GetWeatherForecasts()
        {
            //var requestHeader = _httpClient.DefaultRequestHeaders.Authorization;
            //if (requestHeader == null || string.IsNullOrWhiteSpace(requestHeader.Parameter))
            //{
            //    var accessToken = await _localStorage.GetItemAsync<string>("authToken");
            //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            //}
            
            var response = await _httpClient.GetFromJsonAsync<WeatherForecast[]>("api/weatherforecast");
            return response;
        }
    }
}

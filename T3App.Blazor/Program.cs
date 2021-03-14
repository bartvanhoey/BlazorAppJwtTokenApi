using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using T3App.Blazor.Authentication;
using T3App.Blazor.ViewModels;
using Blazored.LocalStorage;
using T3App.Blazor.Services;

namespace T3App.Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            builder.RootComponents.Add<App>("#app");


            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            //builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
            //builder.Services.AddScoped<IAccessTokenProvider>();

            builder.Services.AddHttpClient("ServerAPI",
                client => client.BaseAddress = new Uri("https://localhost:44370"));
                //.AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

            builder.Services.AddScoped(sp 
                => sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient("ServerAPI"));

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();

            //.AddHttpMessageHandler<AuthorizationMessageHandler>(); ;


            await builder.Build().RunAsync();
        }
    }
}

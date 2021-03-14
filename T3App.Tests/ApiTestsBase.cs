
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using T3App.API;
using T3App.Shared;

namespace T3App.Tests
{
    public  class ApiTestsBase : IDisposable
    {
        protected HttpClient _httpClient { get; }
        protected IServiceProvider _serviceProvider { get; }

        protected LoginModel _validAdminCredentials = new LoginModel { Email = "admin", Password = "SecurePassword" };
        protected LoginModel _invalidAdminCredentials = new LoginModel { Email = "admin", Password = "SecurePassword!" };

        public ApiTestsBase()
        {
            var builder = Program.CreateHostBuilder(null)
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.UseEnvironment("Test");
                });
            var host = builder.Start();
            _serviceProvider = host.Services;
            _httpClient = host.GetTestClient();
            Console.WriteLine("TEST Host Started.");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

    }
}

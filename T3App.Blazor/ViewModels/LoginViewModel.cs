using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using T3App.Shared;

namespace T3App.Blazor.ViewModels
{
    public class LoginViewModel : ILoginViewModel
    {
        private readonly HttpClient _httpClient;
        public string Password { get; set; }
        public string EmailAddress { get; set; }

        public LoginViewModel() { }

        public LoginViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task LoginUser()
        {
            //var loginRequest = new LoginRequest { Password = "SecurePassword", UserName = "admin" };

            var response = await _httpClient.PostAsJsonAsync<LoginRequest>("api/account/login", this);

            var result = await response.Content.ReadAsStringAsync();


        }

        public static implicit operator LoginViewModel(User user)
        {
            return new LoginViewModel
            {
                EmailAddress = user.EmailAddress,
                Password = user.Password
            };
        }

        public static implicit operator LoginRequest(LoginViewModel loginViewModel)
        {
            return new LoginRequest
            {
                UserName = loginViewModel.EmailAddress,
                Password = loginViewModel.Password
            };
        }






    }

    public class LoginRequest
    {
        [Required]
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using T3App.API.Authentication;
using T3App.Shared;
using T3App.Shared.Extensions;
using Xunit;

namespace T3App.Tests
{
    public class AccountControllerTests : AccountControllerTestsBase
    {




        [Fact]
        public async Task ShouldExpect401WhenLoginWithInvalidCredentials()
        {
            var credentials = new LoginModel
            {
                Email = "admin",
                Password = "invalidPassword"
            };
            var response = await _httpClient.PostAsync("api/account/login",
                new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, MediaTypeNames.Application.Json));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ShouldReturnCorrectResponseForSuccessLogin()
        {
            //var loginModel = new LoginModel
            //{
            //    Email = "admin",
            //    Password = "SecurePassword"
            //};

            var response = await _httpClient.PostAsync("api/account/login",
                new StringContent(JsonSerializer.Serialize(_adminCredentials), Encoding.UTF8, MediaTypeNames.Application.Json));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var jsonContent = await response.Content.ReadAsStringAsync();
            var loginResult = jsonContent.ConvertJsonTo<LoginResult>();

            loginResult.Email.Should().Be(_adminCredentials.Email);
            loginResult.OriginalEMail.Should().BeNull();
            loginResult.Role.Should().Be(UserRoles.Admin);
            loginResult.AccessToken.Should().NotBeNullOrWhiteSpace();
            loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();

            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();
            var (principal, jwtSecurityToken) = jwtAuthManager.DecodeJwtToken(loginResult.AccessToken);
            principal.Identity.Name.Should().Be(_adminCredentials.Email);
            principal.FindFirst(ClaimTypes.Role).Value.Should().Be(UserRoles.Admin);
            jwtSecurityToken.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldBeAbleToLogout()
        {
            var response = await _httpClient.PostAsync("api/account/login", new StringContent(JsonSerializer.Serialize(_adminCredentials), Encoding.UTF8, MediaTypeNames.Application.Json));
            var jsonContent = await response.Content.ReadAsStringAsync();
            var loginResult = jsonContent.ConvertJsonTo<LoginResult>();

            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();
            //Assert.IsTrue(jwtAuthManager.UsersRefreshTokensReadOnlyDictionary.ContainsKey(loginResult.RefreshToken));

            jwtAuthManager.UsersRefreshTokensReadOnlyDictionary.Keys.Should().Contain(loginResult.RefreshToken);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, loginResult.AccessToken);
            var logoutResponse = await _httpClient.PostAsync("api/account/logout", null);
            logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            //Assert.IsFalse(jwtAuthManager.UsersRefreshTokensReadOnlyDictionary.ContainsKey(loginResult.RefreshToken));
        }
















    }


}

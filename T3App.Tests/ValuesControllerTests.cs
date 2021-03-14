using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using T3App.API.Authentication;
using T3App.Shared;
using T3App.Shared.Extensions;
using Xunit;

namespace T3App.Tests
{
    public class ValuesControllerTests : ApiTestsBase
    {

        [Fact]
        public async Task ShouldExpect401WhenNotLoggedIn()
        {
            var response = await _httpClient.GetAsync("api/values");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var result = await response.Content.ReadAsStringAsync();
            result.Should().BeNullOrWhiteSpace(); ;
        }

        [Fact]
        public async Task ShouldGetAllKeyValuePairsUsingSuccessLogin()
        {

            var loginResponse = await _httpClient.PostAsync("api/account/login",
                new StringContent(JsonSerializer.Serialize(_validAdminCredentials), Encoding.UTF8, MediaTypeNames.Application.Json));
            var content = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = content.ConvertJsonTo<LoginResult>();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, loginResult.AccessToken);
            var response = await _httpClient.GetAsync("api/values");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            result.Should().Be("[\"value1\",\"value2\"]");
        }

        [Fact]
        public async Task ShouldReturn401ForInvalidToken()
        {
            const string invalidTokenString =
                @"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYW8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTU5NDg1NzYxNSwiaXNzIjoiaHR0cHM6Ly9teXdlYmFwaS5jb20iLCJhdWQiOiJNeSBXZWJBcGkgVXNlcnMifQ.kjO-4siQxx3JVPVtV_jbmSP5fLp-SIJL92Zq3-weCIg";

            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();
            Action action = () => jwtAuthManager.DecodeJwtToken(invalidTokenString);
            action.Should().Throw<SecurityTokenInvalidSignatureException>();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, invalidTokenString);
            var response = await _httpClient.GetAsync("api/values");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ShouldReturn401ForExpiredToken()
        {
            const string userName = "admin";
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,userName),
                new Claim(ClaimTypes.Role, UserRoles.Admin)
            };
            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();
            var jwtTokenConfig = _serviceProvider.GetRequiredService<JwtTokenConfig>();

            // expired token
            var jwtResult = jwtAuthManager.GenerateTokens(userName, claims, DateTime.Now.AddMinutes(-jwtTokenConfig.AccessTokenExpiration - 1));
            var invalidTokenString = jwtResult.AccessToken;
            Action action = () => jwtAuthManager.DecodeJwtToken(invalidTokenString);
            action.Should().Throw<SecurityTokenExpiredException>();


            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, invalidTokenString);
            var response = await _httpClient.GetAsync("api/values");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // not expired
            jwtResult = jwtAuthManager.GenerateTokens(userName, claims, DateTime.Now.AddMinutes(-jwtTokenConfig.AccessTokenExpiration));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, jwtResult.AccessToken);
            response = await _httpClient.GetAsync("api/values");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // not expired token 2
            jwtResult = jwtAuthManager.GenerateTokens(userName, claims, DateTime.Now.AddMinutes(-jwtTokenConfig.AccessTokenExpiration - 1).AddSeconds(1));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, jwtResult.AccessToken);
            response = await _httpClient.GetAsync("api/values");
            response.StatusCode.Should().Be(HttpStatusCode.OK);


        }
    }
}

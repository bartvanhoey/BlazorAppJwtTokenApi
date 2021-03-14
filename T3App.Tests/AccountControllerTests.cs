using System;
using System.Collections.Generic;
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
    public class AccountControllerTests : ApiTestsBase
    {
        [Fact]
        public async Task ShouldExpect401WhenLoginWithInvalidCredentials()
        {
            var response = await _httpClient.PostAsync("api/account/login",
                new StringContent(JsonSerializer.Serialize(_invalidAdminCredentials), Encoding.UTF8, MediaTypeNames.Application.Json));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ShouldReturnCorrectResponseForSuccessLogin()
        {

            var response = await _httpClient.PostAsync("api/account/login",
                new StringContent(JsonSerializer.Serialize(_validAdminCredentials), Encoding.UTF8, MediaTypeNames.Application.Json));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var jsonContent = await response.Content.ReadAsStringAsync();
            var result = jsonContent.ConvertJsonTo<LoginResult>();

            result.Email.Should().Be(_validAdminCredentials.Email);
            result.OriginalEmail.Should().BeNull();
            result.Role.Should().Be(UserRoles.Admin);
            result.AccessToken.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBeNullOrWhiteSpace();

            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();
            var (principal, jwtSecurityToken) = jwtAuthManager.DecodeJwtToken(result.AccessToken);
            principal.Identity.Name.Should().Be(_validAdminCredentials.Email);
            principal.FindFirst(ClaimTypes.Role).Value.Should().Be(UserRoles.Admin);
            jwtSecurityToken.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldBeAbleToLogout()
        {
            var response = await _httpClient.PostAsync("api/account/login", new StringContent(JsonSerializer.Serialize(_validAdminCredentials), Encoding.UTF8, MediaTypeNames.Application.Json));
            var jsonContent = await response.Content.ReadAsStringAsync();
            var result = jsonContent.ConvertJsonTo<LoginResult>();

            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();
            jwtAuthManager.UsersRefreshTokensReadOnlyDictionary.Keys.Should().Contain(result.RefreshToken);

            jwtAuthManager.UsersRefreshTokensReadOnlyDictionary.Keys.Should().Contain(result.RefreshToken);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, result.AccessToken);
            var logoutResponse = await _httpClient.PostAsync("api/account/logout", null);
            logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            jwtAuthManager.UsersRefreshTokensReadOnlyDictionary.Keys.Should().NotContain(result.RefreshToken);
        }

        [Fact]
        public async Task ShouldCorrectlyRefreshToken()
        {
            const string userName = "admin";
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,userName),
                new Claim(ClaimTypes.Role, UserRoles.Admin)
            };
            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();
            var jwtResult = jwtAuthManager.GenerateTokens(userName, claims, DateTime.Now.AddMinutes(-1));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, jwtResult.AccessToken);
            var refreshRequest = new RefreshTokenRequest
            {
                RefreshToken = jwtResult.RefreshToken.TokenString
            };
            var response = await _httpClient.PostAsync("api/account/refresh-token",
                new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, MediaTypeNames.Application.Json));
            var content = await response.Content.ReadAsStringAsync();
            var result =  content.ConvertJsonTo<LoginResult>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);            

            var refreshToken2 = jwtAuthManager.UsersRefreshTokensReadOnlyDictionary.GetValueOrDefault(result.RefreshToken);
            result.RefreshToken.Should().Be(refreshToken2.TokenString);
            refreshToken2.TokenString.Should().NotBe(jwtResult.RefreshToken.TokenString);
            result.AccessToken.Should().NotBe(jwtResult.AccessToken);
        }

        [Fact]
        public async Task ShouldNotAllowToRefreshTokenWhenRefreshTokenIsExpired()
        {
            const string userName = "admin";
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,userName),
                new Claim(ClaimTypes.Role, UserRoles.Admin)
            };
            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();
            var jwtTokenConfig = _serviceProvider.GetRequiredService<JwtTokenConfig>();
            var jwtResult1 = jwtAuthManager.GenerateTokens(userName, claims, DateTime.Now.AddMinutes(-jwtTokenConfig.RefreshTokenExpiration - 1));
            var jwtResult2 = jwtAuthManager.GenerateTokens(userName, claims, DateTime.Now.AddMinutes(-1));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, jwtResult2.AccessToken); // valid JWT token
            var refreshRequest = new RefreshTokenRequest
            {
                RefreshToken = jwtResult1.RefreshToken.TokenString
            };
            var response = await _httpClient.PostAsync("api/account/refresh-token",
                new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, MediaTypeNames.Application.Json)); // expired Refresh token
            var content = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            content.Should().Be("Invalid token");
        }

        [Fact]
        public async Task ShouldAllowAdminImpersonateOthers()
        {
            const string userName = "admin";
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,userName),
                new Claim(ClaimTypes.Role, UserRoles.Admin)
            };
            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();
            var jwtResult = jwtAuthManager.GenerateTokens(userName, claims, DateTime.Now.AddMinutes(-1));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, jwtResult.AccessToken);
            var request = new ImpersonationRequest { Email = "test1" };

            var response = await _httpClient.PostAsync("api/account/impersonation",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, MediaTypeNames.Application.Json));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = content.ConvertJsonTo<LoginResult>();

            result.Email.Should().Be(request.Email);
            result.OriginalEmail.Should().Be(userName);
            result.Role.Should().Be(UserRoles.BasicUser);
            result.AccessToken.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBeNullOrWhiteSpace();

            var (principal, jwtSecurityToken) = jwtAuthManager.DecodeJwtToken(result.AccessToken);
            principal.Identity.Name.Should().Be(request.Email);
            principal.FindFirst(ClaimTypes.Role).Value.Should().Be(UserRoles.BasicUser);
            principal.FindFirst("OriginalEmail").Value.Should().Be(userName);
            jwtSecurityToken.Should().NotBeNull();
        }
        
        [Fact]
        public async Task ShouldReturnBadRequestIfStopImpersonationWhenNotImpersonating()
        {
            const string userName = "test1";
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, UserRoles.BasicUser)
            };
            var jwtAuthManager = _serviceProvider.GetRequiredService<IJwtAuthManager>();
            var jwtResult = jwtAuthManager.GenerateTokens(userName, claims, DateTime.Now.AddMinutes(-1));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, jwtResult.AccessToken);
            var request = new ImpersonationRequest { Email = "test2" };
            var response = await _httpClient.PostAsync("api/account/stop-impersonation",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, MediaTypeNames.Application.Json));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

           
        }

















    }


}

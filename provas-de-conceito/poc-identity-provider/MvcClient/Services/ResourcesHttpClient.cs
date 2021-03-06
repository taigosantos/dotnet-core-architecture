﻿using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace MvcClient.Services
{
    public class ResourcesHttpClient : IResourcesHttpClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient = new HttpClient();

        public ResourcesHttpClient(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HttpClient> GetClient()
        {
            string accessToken;
            var currentContext = _httpContextAccessor.HttpContext;

            // should we renew access & refresh tokens?
            // get expires_at value
            var expiresAt = await currentContext.GetTokenAsync("expires_at");

            // compare - make sure to use the exact date formats for comparison 
            // (UTC, in this case)
            if (string.IsNullOrWhiteSpace(expiresAt) || ((DateTime.Parse(expiresAt).AddSeconds(-60)).ToUniversalTime() < DateTime.UtcNow))
            {
                accessToken = await RenewTokens();
            }
            else
            {
                // get access token
                accessToken = await currentContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            }

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                _httpClient.SetBearerToken(accessToken);
            }

            _httpClient.BaseAddress = new Uri("http://localhost:3000/"); //Resources API Address
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return _httpClient;
        }

        private async Task<string> RenewTokens()
        {
            // get the current HttpContext to access the tokens
            var currentContext = _httpContextAccessor.HttpContext;

            // get the metadata
            var discoveryClient = new DiscoveryClient("https://localhost:44373/"); // IDP Address
            var metaDataResponse = await discoveryClient.GetAsync();

            // create a new token client to get new tokens
            var tokenClient = new TokenClient(metaDataResponse.TokenEndpoint, "taskmvc", "secret");

            // get the saved refresh token
            var currentRefreshToken = await currentContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            // refresh the tokens
            var tokenResult = await tokenClient.RequestRefreshTokenAsync(currentRefreshToken);

            if (tokenResult.IsError)
                throw new Exception("Problem encountered while refreshing tokens.", tokenResult.Exception);

            // Save the tokens. 

            // get auth info
            var authenticateInfo = await currentContext.AuthenticateAsync("Cookies");

            // create a new value for expires_at, and save it
            var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
            authenticateInfo.Properties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));

            authenticateInfo.Properties.UpdateTokenValue(OpenIdConnectParameterNames.AccessToken, tokenResult.AccessToken);
            authenticateInfo.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, tokenResult.RefreshToken);

            // we're signing in again with the new values.  
            await currentContext.SignInAsync("Cookies", authenticateInfo.Principal, authenticateInfo.Properties);

            // return the new access token 
            return tokenResult.AccessToken;
        }
    }
}

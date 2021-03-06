﻿using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Net;
using System.Security.Claims;

namespace MvcClient.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (!Url.IsLocalUrl(returnUrl)) returnUrl = "/";

            var props = new AuthenticationProperties
            {
                RedirectUri = returnUrl
            };

            return Challenge(props, "oidc");
        }

        [Authorize]
        public IActionResult Secure()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> GetApiClaims()
        {
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var client = new HttpClient();
            client.SetBearerToken(accessToken);
            var response = await client.GetAsync("http://localhost:3000/identity").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                ViewBag.Json = JArray.Parse(content).ToString();
                return View();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Denied");
            }

            throw new Exception($"Problema ao acessar a API: {response.ReasonPhrase}");
        }

        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            var discoverClient = new DiscoveryClient("https://localhost:44373/");
            var metaDataResponse = await discoverClient.GetAsync();

            var userInfoClient = new UserInfoClient(metaDataResponse.UserInfoEndpoint);

            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var response = await userInfoClient.GetAsync(accessToken);

            if (response.IsError)
                throw new Exception("Problem accessing the UserInfo endpoint", response.Exception);

            return View(response.Claims);
        }

        public IActionResult Denied(string returnUrl = null)
        {
            return View();
        }

        public async Task Logout()
        {
            // get the metadata
            var discoveryClient = new DiscoveryClient("https://localhost:44373/");
            var metaDataResponse = await discoveryClient.GetAsync();

            // create a TokenRevocationClient
            var revocationClient = new TokenRevocationClient(metaDataResponse.RevocationEndpoint, "taskmvc", "secret");

            // get the access token to revoke 
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var revokeAccessTokenResponse = await revocationClient.RevokeAccessTokenAsync(accessToken);

                if (revokeAccessTokenResponse.IsError)
                {
                    throw new Exception("Problem encountered while revoking the access token.", revokeAccessTokenResponse.Exception);
                }
            }

            // revoke the refresh token as well
            var refreshToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var revokeRefreshTokenResponse = await revocationClient.RevokeRefreshTokenAsync(refreshToken);

                if (revokeRefreshTokenResponse.IsError)
                {
                    throw new Exception("Problem encountered while revoking the refresh token.", revokeRefreshTokenResponse.Exception);
                }
            }

            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");
        }
    }
}
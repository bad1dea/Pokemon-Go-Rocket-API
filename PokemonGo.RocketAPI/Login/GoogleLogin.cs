﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Helpers;

namespace PokemonGo.RocketAPI.Login
{
    internal static class GoogleLogin
    {
        private const string OauthTokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        private const string OauthEndpoint = "https://accounts.google.com/o/oauth2/device/code";
        private const string ClientId = "848232511240-73ri3t7plvk96pj4f85uj8otdat2alem.apps.googleusercontent.com";
        private const string ClientSecret = "NCjF1TLi2CcY6t5mt0ZveuL7";

        internal static async Task<TokenResponseModel> GetAccessToken()
        {
            var deviceCodeResponse = await GetDeviceCode();
            Console.WriteLine("Please visit " + deviceCodeResponse.verification_url + " and enter " + deviceCodeResponse.user_code);

            //Poll until user submitted code..
            TokenResponseModel tokenResponse;
            do
            {
                await Task.Delay(2000);
                tokenResponse = await PollSubmittedToken(deviceCodeResponse.device_code);
            } while (tokenResponse.access_token == null || tokenResponse.refresh_token == null);

            return tokenResponse;
        }

        private static async Task<DeviceCodeModel> GetDeviceCode()
        {
            return await HttpClientHelper.PostFormEncodedAsync<DeviceCodeModel>(OauthEndpoint,
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("scope", "openid email https://www.googleapis.com/auth/userinfo.email"));
        }

        private static async Task<TokenResponseModel> PollSubmittedToken(string deviceCode)
        {
            return await HttpClientHelper.PostFormEncodedAsync<TokenResponseModel>(OauthTokenEndpoint,
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("code", deviceCode),
                new KeyValuePair<string, string>("grant_type", "http://oauth.net/grant_type/device/1.0"),
                new KeyValuePair<string, string>("scope", "openid email https://www.googleapis.com/auth/userinfo.email"));
        }

        public static async Task<TokenResponseModel> GetAccessToken(string refreshToken)
        {
            return await HttpClientHelper.PostFormEncodedAsync<TokenResponseModel>(OauthTokenEndpoint,
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("scope", "openid email https://www.googleapis.com/auth/userinfo.email"));
        }


        internal class ErrorResponseModel
        {
            public string error { get; set; }
            public string error_description { get; set; }
        }

        internal class TokenResponseModel
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public string id_token { get; set; }
        }


        public class DeviceCodeModel
        {
            public string verification_url { get; set; }
            public int expires_in { get; set; }
            public int interval { get; set; }
            public string device_code { get; set; }
            public string user_code { get; set; }
        }

    }
}

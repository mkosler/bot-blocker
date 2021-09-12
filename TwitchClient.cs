using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace bot_banner
{
    #region CustomExceptions
    public class TwitchInvalidTokenException : Exception
    {
        public TwitchInvalidTokenException()
            : base("Do you need to reauthenticate?")
        {
        }
    }
    #endregion

    public class TwitchClient
    {
        private readonly string _clientId;
        private readonly string _accessToken;
        private readonly string _broadcasterId;
        private readonly HttpClient _http;

        public TwitchClient(string clientId, string accessToken, string broadcasterId)
        {
            _clientId = clientId;
            _accessToken = accessToken;
            _broadcasterId = broadcasterId;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            _http = new HttpClient
            {
                BaseAddress = new Uri("https://api.twitch.tv/"),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _accessToken)
                }
            };
            _http.DefaultRequestHeaders.Add("Client-ID", _clientId);
        }

        public void Validate()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "https://id.twitch.tv/oauth2/validate")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("OAuth", _accessToken),
                }
            };
            req.Headers.Add("Client-ID", _clientId);

            using (var res = _http.SendAsync(req).Result)
            {
                if (!res.IsSuccessStatusCode) throw new TwitchInvalidTokenException();
            }
        }

        public IEnumerable<string> GetFollowersOfUserId(string userId)
        {
            var followers = new List<string>();

            using (var res = _http.GetAsync($"/helix/users/follows?to_id={userId}").Result)
            {
                res.EnsureSuccessStatusCode();

                dynamic body = JObject.Parse(res.Content.ReadAsStringAsync().Result);

                foreach (var f in body.data)
                {
                    followers.Add(Convert.ToString(f.from_name));
                }
            }

            return followers;
        }

        public IEnumerable<string> GetOwnBannedUsers()
        {
            var banned = new List<string>();

            using (var res = _http.GetAsync(
                $"/helix/moderation/banned?broadcaster_id={_broadcasterId}").Result)
            {
                res.EnsureSuccessStatusCode();

                dynamic body = JObject.Parse(res.Content.ReadAsStringAsync().Result);

                foreach (var b in body.data)
                {
                    banned.Add(Convert.ToString(b.user_name));
                }
            }

            return banned;
        }
    }
}
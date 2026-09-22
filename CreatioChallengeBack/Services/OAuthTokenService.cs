using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using CreatioChallengeBack.Interfaces;
using CreatioChallengeBack.Config;
using System.Text.Json;
using System.Collections.Generic;

namespace CreatioChallengeBack.Services
{
    public class OAuthTokenService : IOAuthTokenService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly CreatioOptions _options;
        private readonly IConfiguration _configuration;

        private const string CacheKey = "CreatioAccessToken";

        public OAuthTokenService(IHttpClientFactory httpClientFactory, IMemoryCache cache, IOptions<CreatioOptions> options, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _options = options.Value;
            _configuration = configuration;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue<string>(CacheKey, out var token))
                return token;

            // No hardcodear ClientSecret aquí. Intentar leer desde configuración (appsettings / user-secrets) y luego desde variables de entorno.
            var clientId = _options.ClientId;
            // Prioridad: appsettings (Creatio:ClientSecret) -> configuration key CLIENT_SECRET -> environment variable CREATIO_CLIENT_SECRET
            var clientSecret = _configuration?["Creatio:ClientSecret"];
            if (string.IsNullOrEmpty(clientSecret))
            {
                clientSecret = _configuration?["CLIENT_SECRET"];
            }
            if (string.IsNullOrEmpty(clientSecret))
            {
                clientSecret = Environment.GetEnvironmentVariable("CREATIO_CLIENT_SECRET");
            }
            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new InvalidOperationException("Client secret no configurado. Configure Creatio:ClientSecret en appsettings/user-secrets o la variable de entorno CREATIO_CLIENT_SECRET.");
            }

            var client = _httpClientFactory.CreateClient("creatio-token");

            var form = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

            if (!string.IsNullOrEmpty(_options.Scope))
            {
                form["scope"] = _options.Scope;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(form)
            };

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(content);
            if (!doc.RootElement.TryGetProperty("access_token", out var accessTokenElement))
                throw new InvalidOperationException("Respuesta del token no contiene access_token");

            var accessToken = accessTokenElement.GetString() ?? string.Empty;

            var expiresIn = 300; // default
            if (doc.RootElement.TryGetProperty("expires_in", out var expiresEl) && expiresEl.TryGetInt32(out var ei))
            {
                expiresIn = ei;
            }

            var cacheSeconds = _options.TokenCacheSeconds > 0 ? _options.TokenCacheSeconds : expiresIn;
            _cache.Set(CacheKey, accessToken, TimeSpan.FromSeconds(Math.Min(cacheSeconds, expiresIn)));

            return accessToken;
        }
    }
}

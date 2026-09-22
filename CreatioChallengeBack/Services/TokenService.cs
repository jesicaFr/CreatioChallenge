using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CreatioChallengeBack.Services
{
    internal class TokenService : ITokenService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TokenService> _logger;

        private string _accessToken;
        private DateTimeOffset _expiry = DateTimeOffset.MinValue;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public TokenService(IHttpClientFactory httpClientFactory, ILogger<TokenService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            // If token still valid for next 60 seconds, return cached
            if (!string.IsNullOrEmpty(_accessToken) && _expiry > DateTimeOffset.UtcNow.AddSeconds(60))
            {
                return _accessToken;
            }

            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!string.IsNullOrEmpty(_accessToken) && _expiry > DateTimeOffset.UtcNow.AddSeconds(60))
                    return _accessToken;

                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tokenUrl = Environment.GetEnvironmentVariable("TOKEN_URL") ?? "https://122375-studio-is.creatio.com/connect/token";

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    _logger.LogError("CLIENT_ID or CLIENT_SECRET environment variables are not set.");
                    throw new InvalidOperationException("Client credentials are not configured.");
                }

                var client = _httpClientFactory.CreateClient();

                var form = new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["grant_type"] = "client_credentials"
                };

                HttpResponseMessage resp;
                try
                {
                    resp = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(form), cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error connecting to token endpoint {TokenUrl}", tokenUrl);
                    throw;
                }

                var body = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("Token endpoint returned {StatusCode}: {Body}", resp.StatusCode, body);
                    throw new InvalidOperationException("Unable to obtain access token from token endpoint.");
                }

                TokenResponse tokenResp;
                try
                {
                    tokenResp = JsonSerializer.Deserialize<TokenResponse>(body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize token response: {Body}", body);
                    throw;
                }

                if (tokenResp == null || string.IsNullOrEmpty(tokenResp.AccessToken))
                {
                    _logger.LogError("Token response did not contain access_token: {Body}", body);
                    throw new InvalidOperationException("Token response invalid.");
                }

                _accessToken = tokenResp.AccessToken;
                var expiresIn = tokenResp.ExpiresIn > 0 ? TimeSpan.FromSeconds(tokenResp.ExpiresIn) : TimeSpan.FromMinutes(5);
                // Renew a bit before expiry
                _expiry = DateTimeOffset.UtcNow.Add(expiresIn).AddSeconds(-30);

                _logger.LogInformation("Obtained access token, expires in {Seconds} seconds", expiresIn.TotalSeconds);

                return _accessToken;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}

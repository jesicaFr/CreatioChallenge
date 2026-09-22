using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CreatioChallengeBack.Services
{
    internal class CreatioApiClient : ICreatioApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;
        private readonly ILogger<CreatioApiClient> _logger;
        private readonly string _baseUrl;

        public CreatioApiClient(IHttpClientFactory httpClientFactory, ITokenService tokenService, ILogger<CreatioApiClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _logger = logger;
            _baseUrl = Environment.GetEnvironmentVariable("CREATIO_API_BASE") ?? "https://122375-studio-is.creatio.com";
        }

        public async Task<string> GetStringAsync(string relativeUrl, CancellationToken cancellationToken = default)
        {
            var token = await _tokenService.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage resp;
            try
            {
                resp = await client.GetAsync(relativeUrl, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to Creatio API at {Url}", relativeUrl);
                throw new ApiException("Error connecting to external API. Por favor intente nuevamente más tarde.");
            }

                var body = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                {
                    // Log full diagnostic: composed URL, status code and response body to help debug 404
                    var composed = client.BaseAddress is null ? relativeUrl : new Uri(client.BaseAddress, relativeUrl).ToString();
                    _logger.LogWarning("Creatio API returned {StatusCode} for {ComposedUrl} (relative: {RelativeUrl}). Response body: {Body}", resp.StatusCode, composed, relativeUrl, body);

                    // Include the composed URL in the ApiException message so caller can see exact target
                    throw new ApiException($"External API error (HTTP {(int)resp.StatusCode}) when calling {composed}.", (int)resp.StatusCode);
                }

                return body;
        }
    }
}

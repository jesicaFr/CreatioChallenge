using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using CreatioChallengeBack.Config;
using CreatioChallengeBack.Dtos;
using CreatioChallengeBack.Interfaces;

namespace CreatioChallengeBack.Clients
{
    /// <summary>
    /// Cliente que consume OData de Creatio para obtener Accounts.
    /// </summary>
    public class CreatioODataClient : ICreatioClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOAuthTokenService _tokenService;
        private readonly CreatioOptions _options;

        public CreatioODataClient(IHttpClientFactory httpClientFactory, IOAuthTokenService tokenService, IOptions<CreatioOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _options = options.Value;
        }

        public Task<IEnumerable<CreatioContactDto>> GetContactsAsync(CancellationToken cancellationToken = default)
        {
            // For now fallback to empty or could map to Accounts; keep Fake for contacts.
            return Task.FromResult<IEnumerable<CreatioContactDto>>(Array.Empty<CreatioContactDto>());
        }

        public async Task<PagedResult<AccountListItemDto>> GetAccountsAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var top = pageSize;
            var skip = (page - 1) * pageSize;

            var select = "Id,Name,Code";
            var expand = "Type($select=Name)";

            string? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var safe = search.Replace("'", "''");
                filter = $"contains(Name, '{safe}')";
            }

            var query = new List<string>();
            query.Add("$select=" + Uri.EscapeDataString(select));
            if (!string.IsNullOrEmpty(expand)) query.Add("$expand=" + Uri.EscapeDataString(expand));
            if (!string.IsNullOrEmpty(filter)) query.Add("$filter=" + Uri.EscapeDataString(filter));
            query.Add("$top=" + top);
            query.Add("$skip=" + skip);
            query.Add("$count=true");

            var requestUri = _options.BaseUrl.TrimEnd('/') + "/Account" + "?" + string.Join("&", query);

            var client = _httpClientFactory.CreateClient("creatio-odata");
            var token = await _tokenService.GetAccessTokenAsync(cancellationToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync(requestUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync(cancellationToken);
                var reason = response.ReasonPhrase;
                var uri = requestUri;
                var error = $"Creatio OData error: {response.StatusCode} {reason} - RequestUri: {uri} - Body: {msg}";
                throw new HttpRequestException(error, null, response.StatusCode);
            }

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var result = new PagedResult<AccountListItemDto>
            {
                Page = page,
                PageSize = pageSize,
                Items = new List<AccountListItemDto>(),
                Total = 0
            };

            if (doc.RootElement.TryGetProperty("@odata.count", out var countEl) && countEl.TryGetInt32(out var total))
            {
                result.Total = total;
            }

            if (doc.RootElement.TryGetProperty("value", out var values) && values.ValueKind == JsonValueKind.Array)
            {
                var items = new List<AccountListItemDto>();
                foreach (var item in values.EnumerateArray())
                {
                    var dto = new AccountListItemDto();
                    if (item.TryGetProperty("Id", out var idEl)) dto.Id = idEl.GetString() ?? string.Empty;
                    if (item.TryGetProperty("Name", out var nameEl)) dto.Name = nameEl.GetString() ?? string.Empty;
                    if (item.TryGetProperty("Code", out var codeEl)) dto.Code = codeEl.GetString() ?? string.Empty;

                    if (item.TryGetProperty("Type", out var typeEl) && typeEl.ValueKind == JsonValueKind.Object)
                    {
                        if (typeEl.TryGetProperty("Name", out var tname)) dto.TypeName = tname.GetString();
                    }

                    items.Add(dto);
                }

                result.Items = items;
            }

            return result;
        }

        public async Task<object> CreateAccountAsync(CreateAccountRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("El nombre es obligatorio.", nameof(request));
            if (string.IsNullOrWhiteSpace(request.Code)) throw new ArgumentException("El código es obligatorio.", nameof(request));
            if (string.IsNullOrWhiteSpace(request.TypeId)) throw new ArgumentException("El TypeId es obligatorio.", nameof(request));

            var payload = new
            {
                Name = request.Name,
                Code = request.Code,
                Phone = request.Phone,
                Web = request.Web,
                TypeId = request.TypeId
            };

            var json = JsonSerializer.Serialize(payload);
            var client = _httpClientFactory.CreateClient("creatio-odata");
            var token = await _tokenService.GetAccessTokenAsync(cancellationToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestUri = _options.BaseUrl.TrimEnd('/') + "/Account";
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Creatio create account error: {response.StatusCode} - {body}", null, response.StatusCode);
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return new { success = true };
            }

            try
            {
                return JsonDocument.Parse(body).RootElement;
            }
            catch
            {
                return new { success = true, raw = body };
            }
        }
    }
}

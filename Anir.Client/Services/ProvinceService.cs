using Anir.Shared.Contracts;
using System.Net.Http.Json;

namespace Anir.Client.Services
{
    public class ProvinceService
    {
        private readonly HttpClient _http;
        public ProvinceService(HttpClient http) => _http = http;

        public async Task<List<ProvinceDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<ProvinceDto>>("api/provinces");
    }
}

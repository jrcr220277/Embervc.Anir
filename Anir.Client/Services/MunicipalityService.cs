using Anir.Shared.Contracts;
using System.Net.Http.Json;

namespace Anir.Client.Services
{
    public class MunicipalityService
    {
        private readonly HttpClient _http;
        public MunicipalityService(HttpClient http) => _http = http;

        public async Task<List<MunicipalityDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<MunicipalityDto>>("api/municipalities");

        public async Task<List<MunicipalityDto>> GetByProvinceAsync(int provinceId)
            => await _http.GetFromJsonAsync<List<MunicipalityDto>>(
                $"api/municipalities/by-province/{provinceId}");
    }
}

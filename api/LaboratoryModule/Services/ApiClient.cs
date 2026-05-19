using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LaboratoryModule.Models;

namespace LaboratoryModule.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiClient(string baseUrl = "http://localhost:63519")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        private JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<ProductionLot>> GetLotsForLabAsync(LotType? type = null)
        {
            string url = $"{_baseUrl}/api/lab/lots";
            if (type.HasValue)
                url += $"?type={type.Value}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProductionLot>>(json, GetJsonOptions()) ?? new();
        }

        public async Task<ProductionLot?> GetLotAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/lab/lots/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductionLot>(json, GetJsonOptions());
        }

        public async Task<LabTest?> GetActiveTestAsync(int lotId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/lab/lots/{lotId}/test");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LabTest>(json, GetJsonOptions());
        }

        public async Task<LabTest> CreateTestAsync(int lotId)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/lab/lots/{lotId}/test", null);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LabTest>(json, GetJsonOptions())!;
        }

        public async Task<List<LabParameter>> GetTestTemplateAsync(int productId, LotType lotType)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/lab/templates?productId={productId}&type={lotType}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<LabParameter>>(json, GetJsonOptions()) ?? new();
        }

        public async Task<LabResult> SaveResultAsync(int testId, int parameterId, object value)
        {
            var data = new { ParameterId = parameterId, Value = value };
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/lab/tests/{testId}/results", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LabResult>(json, GetJsonOptions())!;
        }

        public async Task<LabTest> CompleteTestAsync(int testId, LabDecision decision, string? blockReason = null)
        {
            var data = new { Decision = decision, BlockReason = blockReason };
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/lab/tests/{testId}/complete", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LabTest>(json, GetJsonOptions())!;
        }

        public async Task<byte[]> GenerateProtocolAsync(int testId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/lab/tests/{testId}/protocol");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<List<AuditEntry>> GetAuditLogAsync(int lotId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/lab/lots/{lotId}/audit");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<AuditEntry>>(json, GetJsonOptions()) ?? new();
        }
    }
}
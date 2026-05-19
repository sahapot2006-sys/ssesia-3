using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OperatorModule.Models;

namespace OperatorModule.Services
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

        // ========== ПАРТИИ ==========

        // Получить активные партии (в работе или на паузе)
        public async Task<List<ProductionLot>> GetActiveLotsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/operator/lots/active");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProductionLot>>(json, GetJsonOptions()) ?? new();
        }

        // Получить партию по ID
        public async Task<ProductionLot?> GetLotAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/operator/lots/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductionLot>(json, GetJsonOptions());
        }

        // ========== ШАГИ ==========

        // Получить все шаги для партии
        public async Task<List<TechCardStep>> GetLotStepsAsync(int lotId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/operator/lots/{lotId}/steps");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TechCardStep>>(json, GetJsonOptions()) ?? new();
        }

        // Получить выполнение текущего шага
        public async Task<StepExecution?> GetCurrentStepExecutionAsync(int lotId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/operator/lots/{lotId}/current-step");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StepExecution>(json, GetJsonOptions());
        }

        // Начать шаг
        public async Task<StepExecution> StartStepAsync(int lotId, int stepId)
        {
            var data = new { LotId = lotId, StepId = stepId };
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/operator/lots/{lotId}/steps/{stepId}/start", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StepExecution>(json, GetJsonOptions())!;
        }

        // Сохранить значение параметра шага
        public async Task SaveStepParameterAsync(int executionId, int parameterId, string value)
        {
            var data = new { ExecutionId = executionId, ParameterId = parameterId, Value = value };
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/operator/step-executions/{executionId}/parameters", content);
            response.EnsureSuccessStatusCode();
        }

        // Завершить шаг
        public async Task CompleteStepAsync(int executionId, string? deviationNote = null)
        {
            var data = new { DeviationNote = deviationNote };
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/operator/step-executions/{executionId}/complete", content);
            response.EnsureSuccessStatusCode();
        }

        // Завершить всю партию
        public async Task CompleteLotAsync(int lotId)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/operator/lots/{lotId}/complete", null);
            response.EnsureSuccessStatusCode();
        }

        // Приостановить партию
        public async Task PauseLotAsync(int lotId)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/operator/lots/{lotId}/pause", null);
            response.EnsureSuccessStatusCode();
        }

        // Возобновить партию
        public async Task ResumeLotAsync(int lotId)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/operator/lots/{lotId}/resume", null);
            response.EnsureSuccessStatusCode();
        }

        // ========== ОТКЛОНЕНИЯ ==========

        // Сообщить об отклонении
        public async Task ReportDeviationAsync(ProcessDeviation deviation)
        {
            var content = new StringContent(JsonSerializer.Serialize(deviation), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/operator/deviations", content);
            response.EnsureSuccessStatusCode();
        }

        // ========== ТЕЛЕМЕТРИЯ ==========

        // Получить телеметрию оборудования
        public async Task<TelemetryData?> GetTelemetryAsync(int equipmentId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/operator/telemetry/{equipmentId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TelemetryData>(json, GetJsonOptions());
        }

        // Отправить команду на оборудование
        public async Task SendEquipmentCommandAsync(int equipmentId, string command, object? parameters = null)
        {
            var data = new { Command = command, Parameters = parameters };
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/operator/equipment/{equipmentId}/command", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
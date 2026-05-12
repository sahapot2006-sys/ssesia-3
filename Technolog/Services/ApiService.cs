using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Technolog.Models;

namespace Technolog.Services
{
    public class ApiService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string BaseUrl = "http://localhost:63519/api";

        public static User CurrentUser { get; set; }

        // ==================== АВТОРИЗАЦИЯ ====================

        public async Task<User> Login(string login, string password)
        {
            var data = new { login, password };
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BaseUrl}/auth/login", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                CurrentUser = JsonConvert.DeserializeObject<User>(result);
                return CurrentUser;
            }

            throw new Exception(result);
        }

        public async Task<string> Register(RegisterModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BaseUrl}/auth/register", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return result;

            throw new Exception(result);
        }

        // ==================== ПРОДУКТЫ ====================

        public async Task<List<Product>> GetProducts()
        {
            var response = await client.GetAsync($"{BaseUrl}/reference/products");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<Product>>(result);

            throw new Exception(result);
        }
        public async Task<Product> CreateProduct(Product product)
        {
            var json = JsonConvert.SerializeObject(product);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BaseUrl}/reference/products", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Product>(result);
            }
            else
            {
                throw new Exception($"Ошибка API: {result}");
            }
        }

        // ==================== РЕЦЕПТУРЫ ====================

        public async Task<List<Recipe>> GetRecipes()
        {
            var response = await client.GetAsync($"{BaseUrl}/recipes");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<Recipe>>(result);

            throw new Exception(result);
        }

        public async Task<Recipe> GetRecipe(int id)
        {
            var response = await client.GetAsync($"{BaseUrl}/recipes/{id}");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Recipe>(result);

            throw new Exception(result);
        }

        public async Task<Recipe> CreateRecipe(RecipeCreateModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BaseUrl}/recipes", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Recipe>(result);

            throw new Exception(result);
        }

        public async Task<string> AddComposition(int recipeId, CompositionModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BaseUrl}/recipes/{recipeId}/composition", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return result;

            throw new Exception(result);
        }

        // ==================== ТЕХНОЛОГИЧЕСКИЕ КАРТЫ ====================

        public async Task<List<ProcessCard>> GetProcessCards()
        {
            var response = await client.GetAsync($"{BaseUrl}/processcards");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<ProcessCard>>(result);

            throw new Exception(result);
        }

        public async Task<ProcessCard> CreateProcessCard(ProcessCardModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BaseUrl}/processcards", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ProcessCard>(result);

            throw new Exception(result);
        }

        public async Task<string> ApproveProcessCard(int id)
        {
            var response = await client.PostAsync($"{BaseUrl}/processcards/{id}/approve", null);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return result;

            throw new Exception(result);
        }

        // ==================== ЗАКАЗЫ ====================

        public async Task<List<ProductionOrder>> GetOrders()
        {
            var response = await client.GetAsync($"{BaseUrl}/orders");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<ProductionOrder>>(result);

            throw new Exception(result);
        }

        public async Task<ProductionOrder> CreateOrder(OrderModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BaseUrl}/orders", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ProductionOrder>(result);

            throw new Exception(result);
        }

        // ==================== ПАРТИИ ====================

        public async Task<List<Batch>> GetBatches()
        {
            var response = await client.GetAsync($"{BaseUrl}/batches");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<Batch>>(result);

            throw new Exception(result);
        }

        public async Task<Batch> CreateBatch(BatchModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BaseUrl}/batches", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Batch>(result);

            throw new Exception(result);
        }

        public async Task<Batch> GetBatch(int id)
        {
            var response = await client.GetAsync($"{BaseUrl}/batches/{id}");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Batch>(result);

            throw new Exception(result);
        }

        // ==================== ОТКЛОНЕНИЯ ====================

        public async Task<List<Deviation>> GetDeviations()
        {
            var response = await client.GetAsync($"{BaseUrl}/deviations");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<Deviation>>(result);

            throw new Exception(result);
        }

        public async Task<List<Deviation>> GetDeviationsByBatch(int batchId)
        {
            var response = await client.GetAsync($"{BaseUrl}/deviations/batch/{batchId}");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<Deviation>>(result);

            throw new Exception(result);
        }

        public async Task<List<EventLog>> GetEvents(int limit = 50)
        {
            var response = await client.GetAsync($"{BaseUrl}/events?limit={limit}");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<EventLog>>(result);

            throw new Exception(result);
        }

        // ==================== ЭКСТРУДЕР ====================

        public async Task<List<ExtruderProgram>> GetExtruderPrograms()
        {
            var response = await client.GetAsync($"{BaseUrl}/extruder/programs");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<ExtruderProgram>>(result);

            throw new Exception(result);
        }

        public async Task<ExtruderProgram> CreateExtruderProgram(ExtruderProgramModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BaseUrl}/extruder/programs", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ExtruderProgram>(result);

            throw new Exception(result);
        }

        // ==================== ОТЧЁТЫ ====================

        public async Task<byte[]> GetBatchesReport(DateTime from, DateTime to)
        {
            var response = await client.GetAsync($"{BaseUrl}/reports/batches?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsByteArrayAsync();

            throw new Exception("Ошибка получения отчёта");
        }
    }
}
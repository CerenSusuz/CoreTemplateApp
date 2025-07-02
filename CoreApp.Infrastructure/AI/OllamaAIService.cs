using CoreApp.Application.Common.Interfaces.AI;
using CoreApp.Application.Common.Models;
using System.Runtime;
using System.Text;
using System.Text.Json;
using CoreApp.Application.Common.Settings;

namespace CoreApp.Infrastructure.AI
{
    public class OllamaAiService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly AISettings _settings;

        public OllamaAiService(AISettings settings)
        {
            _settings = settings;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:11434/")
            };
        }

        public async Task<string> PromptAsync(string prompt, AiRequestOptions? options = null)
        {
            var requestBody = new
            {
                model = _settings.Model, // : "mistral"
                prompt = prompt,
                stream = false
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/generate", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"[ERROR]: Ollama API Error - {response.StatusCode}";

            try
            {
                using var doc = JsonDocument.Parse(responseString);
                return doc.RootElement.GetProperty("response").GetString() ?? string.Empty;
            }
            catch
            {
                return "[Ollama AI response error]";
            }
        }
    }

}

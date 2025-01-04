using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AI21Service
{
    private const string ApiUrl = "https://api.ai21.com/studio/v1/chat/completions";
    private const string ApiKey = "ssfbyqBcn6lJSnpb0cDOEFLfXeSBsolv"; // Zamień na swój klucz API
    private readonly HttpClient _httpClient;

    public AI21Service(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string> GetResponseFromAI21Async(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));
        }

        // Budowa treści żądania
        var requestBody = new
        {
            model = "jamba-1.5-mini",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            temperature = 0.8,
            max_tokens = 200
        };

        // Serializacja JSON
        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Konfiguracja nagłówków żądania
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

        try
        {
            // Wysyłanie żądania do API
            var response = await _httpClient.PostAsync(ApiUrl, content);

            // Odczyt odpowiedzi jako tekst
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(jsonResponse))
            {
                throw new Exception("API response is empty.");
            }

            if (response.IsSuccessStatusCode)
            {
                // Deserializacja odpowiedzi
                var result = JsonSerializer.Deserialize<AI21Response>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Choices?[0]?.Message?.Content ?? "No response received.";
            }

            throw new HttpRequestException($"Error calling AI21 API: {response.StatusCode}, {jsonResponse}");
        }
        catch (Exception ex)
        {
            // Logowanie błędu (opcjonalnie możesz dodać logger)
            Console.WriteLine($"Error: {ex.Message}");
            throw; // Przekazanie wyjątku dalej
        }
    }

    // Klasy modelu dla odpowiedzi JSON
    private class AI21Response
    {
        public string? Id { get; set; }
        public Choice[]? Choices { get; set; }
        public Usage? Usage { get; set; }
    }

    private class Choice
    {
        public int Index { get; set; }
        public Message? Message { get; set; }
        public string? FinishReason { get; set; }
    }

    private class Message
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
    }

    private class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}

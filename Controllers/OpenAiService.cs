using System.Text;
using System.Text.Json;

public class OpenAiService
{
    private readonly HttpClient _httpClient;

    public OpenAiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetResponseFromOpenAiAsync(string prompt)
    {
        // Endpoint API OpenAI
        var apiUrl = "https://api.openai.com/v1/chat/completions";

        // Model używany w żądaniu
        var requestBody = new
        {
            model = "babbage-002", // Możesz podać tutaj odpowiedni model
            messages = new[]
            {
                new { role = "system", content = "You are an expert in financial analysis, but answer in one word if that is possiblle." },
                new { role = "user", content = prompt }
            },
            max_tokens = 10,
            temperature = 0.7
        };

        // Serializacja żądania do JSON
        var jsonContent = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Ręczne dodanie nagłówka z kluczem API
        var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        request.Headers.Add("Authorization", "Bearer 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");
        request.Content = httpContent;

        // Wysłanie zapytania i odbiór odpowiedzi
        var response = await _httpClient.SendAsync(request);

        // Obsługa błędów - upewnij się, że odpowiedź zakończyła się sukcesem
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error: {response.StatusCode}, Content: {errorContent}");
        }

        // Odczytanie zawartości odpowiedzi
        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }
}

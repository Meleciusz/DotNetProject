using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

public class ChatController : Controller
{
    private readonly OpenAiService _openAiService;
    private readonly AI21Service _ai21Service;

    private static readonly List<string> Keywords = new List<string>
    {
        "waluta", "kurs", "inwestowanie", "euro", "dolar", "złoty",
        "usd", "eur", "gbp", "chf", "jpy", "pln", "sek", "cad", "aud", "nzd",
        "frank", "yuan", "bitcoin", "akcje", "gielda", "sp500", "nasdaq", "ftse", "walory",
        "ropa", "złoto", "srebro", "obligacje", "fundusze", "spekulacja", "dividenda",
        "rynek", "rynek finansowy", "inwestor", "rynki", "portfel inwestycyjny", "dywidenda",
        "ryzyko", "stopy procentowe", "przewalutowanie", "wymiana walut", "obligacje", "broker",
        "handel", "forex", "forexowy", "trader", "strategia inwestycyjna", "obligacja", "portfel"
    };

    public ChatController(OpenAiService openAiService, AI21Service ai21Service)
    {
        _openAiService = openAiService;
        _ai21Service = ai21Service;
    }

    [HttpGet]
    public IActionResult Ask()
    {
        return View();
    }


    // Obsługa pytań predefiniowanych (OpenAI)
    [HttpPost]
    public async Task<IActionResult> Ask(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            ViewBag.Error = "Musisz wybrać pytanie.";
            return View();
        }

        try
        {
            var response = await _openAiService.GetResponseFromOpenAiAsync(question);
            ViewBag.Response = response;
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Wystąpił błąd: {ex.Message}";
        }

        return View("Ask");
    }

    // Obsługa pytań niestandardowych (AI21)
    [HttpPost]
    public async Task<IActionResult> AskCustom(string customQuestion)
    {
        if (string.IsNullOrWhiteSpace(customQuestion))
        {
            ViewBag.Error = "Musisz wpisać pytanie.";
            return View("Ask");
        }

        // Normalizujemy tekst na małe litery i usuwamy niepotrzebne znaki
        var normalizedInput = customQuestion.ToLower();

        // Tworzymy wyrażenie regularne, które sprawdza występowanie słów kluczowych
        var pattern = string.Join("|", Keywords.Select(kw => $@"\b{Regex.Escape(kw)}\b"));

        // Sprawdzamy, czy tekst zawiera któreś ze słów kluczowych
        var regex = new Regex(pattern);
        bool ifHaveKeyWord =  regex.IsMatch(normalizedInput);


        if (!ifHaveKeyWord)
        {
            ViewBag.Error = "Czat jest stworzony, aby zadawać pytania odnośnie kursów, walut etc. :)";
            return View("Ask");
        }

        try
        {
            var response = await _ai21Service.GetResponseFromAI21Async(customQuestion);
            ViewBag.Response = response;
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Wystąpił błąd: {ex.Message}";
        }

        return View("Ask");
    }
}

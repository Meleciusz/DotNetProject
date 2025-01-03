using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Dodane
using Project.Data;
using Project.Models;
using static System.Reflection.Metadata.BlobBuilder;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class CurrenciesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CurrencyService _currencyService;

    public CurrenciesController(ApplicationDbContext context, CurrencyService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    public async Task<IActionResult> Index()
    {
        // Pobierz dane o kursach walut z API dla kilku walut
        var currencyCodes = new List<string> { "USD", "EUR", "GBP" };

        var currencyRates = new List<CurrencyRate>();
        int i = 0;
        foreach(var x in currencyCodes)
        {
            currencyRates.Add(await _currencyService.GetCurrencyRateAsync(currencyCodes[i]));
            i++;
        }


        // Sprawdź i zaktualizuj kursy w bazie danych
        foreach (var rates in currencyRates)
        {
            // Pobierz pierwszy (i jedyny) element z listy "Rates"
            var rate = rates.Rates.First();

            // Sprawdź, czy waluta już istnieje w bazie za pomocą pola "Code"
            var existingCurrency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == rates.Code);

            // Zaktualizuj kurs
            existingCurrency.Rate = rate.Mid;
            _context.Currencies.Update(existingCurrency);

        }

        // Zapisz zmiany w bazie danych
        await _context.SaveChangesAsync();

        // Pobierz wszystkie waluty i przekaż je do widoku
        var currencies = await _context.Currencies.ToListAsync();
        return View(currencies);
    }

    //Refresh rates 
    [HttpPost]
    public async Task<IActionResult> RefreshRates()
    {
        // Pobierz dane o kursach walut z API dla kilku walut
        var currencyCodes = new List<string> { "USD", "EUR", "GBP" };

        var currencyRates = new List<CurrencyRate>();
        int i = 0;
        foreach (var x in currencyCodes)
        {
            currencyRates.Add(await _currencyService.GetCurrencyRateAsync(currencyCodes[i]));
            i++;
        }


        // Sprawdź i zaktualizuj kursy w bazie danych
        foreach (var rates in currencyRates)
        {
            // Pobierz pierwszy (i jedyny) element z listy "Rates"
            var rate = rates.Rates.First();

            // Sprawdź, czy waluta już istnieje w bazie za pomocą pola "Code"
            var existingCurrency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == rates.Code);

            // Zaktualizuj kurs
            existingCurrency.Rate = rate.Mid;
            _context.Currencies.Update(existingCurrency);

        }

        // W metodzie RefreshRates
        foreach (var rates in currencyRates)
        {
            var rate = rates.Rates.First();
            var existingCurrency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == rates.Code);

            existingCurrency.Rate = rate.Mid;
            _context.Currencies.Update(existingCurrency);

            // Dodaj dane historyczne
            var historicalData = new HistoricalData
            {
                CurrencyCode = rates.Code,
                Rate = rate.Mid,
                Timestamp = DateTime.UtcNow
            };
            await _context.HistoricalDatas.AddAsync(historicalData);
        }


        // Zapisz zmiany w bazie danych
        await _context.SaveChangesAsync();

        TempData["Message"] = "Kursy zostały odświeżone!";

        // Przekieruj użytkownika z powrotem do strony głównej
        return RedirectToAction(nameof(Index));
    }

    // GET: Currencies/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var currency = await _context.Currencies.FirstOrDefaultAsync(m => m.Id == id);
        if (currency == null)
        {
            return NotFound();
        }

        // Pobierz dane historyczne dla danej waluty
        var historicalData = await _context.HistoricalDatas
            .Where(h => h.CurrencyCode == currency.Code)
            .OrderBy(h => h.Timestamp)
            .ToListAsync();

        // Przekaż dane historyczne do widoku za pomocą ViewBag
        ViewBag.HistoricalData = historicalData;

        return View(currency);
    }



    // GET: Currencies/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Currencies/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,Description,Author")] Currency currency)
    {
        if (ModelState.IsValid)
        {
            _context.Add(currency);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(currency);
    }

}

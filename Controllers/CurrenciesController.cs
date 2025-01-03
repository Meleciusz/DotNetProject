using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Dodane
using Project.Data;
using Project.Models;
using static System.Reflection.Metadata.BlobBuilder;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
        // Pobierz dynamiczną listę kodów walut z API
        var currencyCodes = await _currencyService.GetAllCurrencyCodesAsync();

        var currencyRates = new List<CurrencyRate>();

        // Pobierz szczegółowe dane dla każdej waluty
        foreach (var code in currencyCodes)
        {
            currencyRates.Add(await _currencyService.GetCurrencyRateAsync(code));
        }

        // Aktualizacja bazy danych
        foreach (var rates in currencyRates)
        {
            var rate = rates.Rates.First();
            var existingCurrency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == rates.Code);

            if (existingCurrency == null)
            {
                // Dodaj nową walutę do bazy, jeśli nie istnieje
                _context.Currencies.Add(new Currency
                {
                    Code = rates.Code,
                    Rate = rate.Mid
                });
            }
            else
            {
                // Zaktualizuj istniejący kurs
                existingCurrency.Rate = rate.Mid;
                _context.Currencies.Update(existingCurrency);
            }


            // Dodaj dane historyczne
            var historicalData = new HistoricalData
            {
                CurrencyCode = rates.Code,
                Rate = rate.Mid,
                Timestamp = DateTime.UtcNow
            };
            await _context.HistoricalDatas.AddAsync(historicalData);
        }

    await _context.SaveChangesAsync();

        var currencies = await _context.Currencies.ToListAsync();
        return View(currencies);
    }


    // Refresh rates
    [HttpPost]
    public async Task<IActionResult> RefreshRates()
    {
        // Pobierz dynamiczną listę kodów walut z API
        var currencyCodes = await _currencyService.GetAllCurrencyCodesAsync();

        // Pobierz szczegółowe dane o kursach dla każdej waluty
        var currencyRates = new List<CurrencyRate>();
        foreach (var code in currencyCodes)
        {
            currencyRates.Add(await _currencyService.GetCurrencyRateAsync(code));
        }

        // Sprawdź i zaktualizuj kursy w bazie danych
        foreach (var rates in currencyRates)
        {
            // Pobierz pierwszy (i jedyny) element z listy "Rates"
            var rate = rates.Rates.First();

            // Sprawdź, czy waluta już istnieje w bazie za pomocą pola "Code"
            var existingCurrency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == rates.Code);

            if (existingCurrency == null)
            {
                // Dodaj nową walutę do bazy, jeśli nie istnieje
                _context.Currencies.Add(new Currency
                {
                    Code = rates.Code,
                    Rate = rate.Mid
                });
            }
            else
            {
                // Zaktualizuj istniejący kurs
                existingCurrency.Rate = rate.Mid;
                _context.Currencies.Update(existingCurrency);
            }

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

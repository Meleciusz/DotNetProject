using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Dodane
using Project.Data;
using Project.Models;

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
                    Rate = rate.Mid,
                    Name = rates.Currency,
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

        // Uwzględnij waluty dodane przez użytkownika
        var userAddedCurrencies = await _context.Currencies
            .Where(c => c.isAddedByUser)
            .ToListAsync();

        foreach (var userCurrency in userAddedCurrencies)
        {
            // Dodaj dane historyczne dla walut użytkownika
            var historicalData = new HistoricalData
            {
                CurrencyCode = userCurrency.Code,
                Rate = userCurrency.Rate, // Aktualny kurs użytkownika
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
                    Rate = rate.Mid,
                    Name = rates.Currency,
                });
            }
            else
            {
                // Zaktualizuj istniejący kurs
                existingCurrency.Rate = rate.Mid;
                existingCurrency.Name = rates.Currency;
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

        // Uwzględnij waluty dodane przez użytkownika
        var userAddedCurrencies = await _context.Currencies
            .Where(c => c.isAddedByUser)
            .ToListAsync();

        foreach (var userCurrency in userAddedCurrencies)
        {
            // Dodaj dane historyczne dla walut użytkownika
            var historicalData = new HistoricalData
            {
                CurrencyCode = userCurrency.Code,
                Rate = userCurrency.Rate, // Aktualny kurs użytkownika
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
    public async Task<IActionResult> Create([Bind("Name,Code,Rate")] Currency currency)
    {

        var existingCurrency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Code == currency.Code);

        if (existingCurrency != null)
        {
            ModelState.AddModelError("Code", "Currency code must be unique.");
        }


        if (ModelState.IsValid)
        {
            currency.isAddedByUser = true;
            _context.Add(currency);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(currency);
    }

    // GET: Currencies/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var currency = await _context.Currencies.FindAsync(id);
        if (currency == null || !currency.isAddedByUser) // Tylko waluty użytkownika mogą być edytowane
        {
            return NotFound();
        }

        return View(currency);
    }

    // POST: Currencies/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Rate,Name")] Currency currency)
    {
        if (id != currency.Id)
        {
            return NotFound();
        }

        var existingCurrency = await _context.Currencies.FindAsync(id);
        if (existingCurrency == null || !existingCurrency.isAddedByUser)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            existingCurrency.Code = currency.Code;
            existingCurrency.Rate = currency.Rate;
            existingCurrency.Name = currency.Name;

            _context.Update(existingCurrency);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(currency);
    }

    // GET: Currencies/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == id && c.isAddedByUser); // Tylko waluty użytkownika
        if (currency == null)
        {
            return NotFound();
        }

        return View(currency);
    }


    // POST: Currencies/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var currency = await _context.Currencies.FindAsync(id);
        if (currency != null && currency.isAddedByUser) // Usuń tylko waluty użytkownika
        {
            _context.Currencies.Remove(currency);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }


}

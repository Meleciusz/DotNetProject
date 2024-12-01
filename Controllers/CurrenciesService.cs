using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json; // Dodaj za pomocą NuGet

public class CurrencyService
{
    private readonly HttpClient _httpClient;

    public CurrencyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CurrencyRate> GetCurrencyRateAsync(string currencyCode)
    {
        var response = await _httpClient.GetStringAsync($"http://api.nbp.pl/api/exchangerates/rates/A/{currencyCode}/?format=json");
        var currencyRate = JsonConvert.DeserializeObject<CurrencyRate>(response);

        if (currencyRate?.Rates == null || !currencyRate.Rates.Any())
        {
            throw new Exception("Nie znaleziono kursów waluty.");
        }

        // Zwracamy pierwszy kurs z listy
        return new CurrencyRate
        {
            Currency = currencyRate.Currency,
            Code = currencyRate.Code,
            Rates = new List<Rate>
        {
            currencyRate.Rates.First()
        }
        };
    }

}

public class CurrencyRate
{
    public string Table { get; set; }
    public string Currency { get; set; }
    public string Code { get; set; }
    public List<Rate> Rates { get; set; }
}

public class Rate
{
    public string No { get; set; }
    public string EffectiveDate { get; set; }
    public decimal Mid { get; set; }
}



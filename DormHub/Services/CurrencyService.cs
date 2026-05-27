using System.Text.Json;
using System.Text.Json.Serialization;

namespace DormHub.Services
{
    public class NbpRate
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "";

        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("mid")]
        public decimal Mid { get; set; }
    }

    public class NbpTable
    {
        [JsonPropertyName("table")]
        public string Table { get; set; } = "";

        [JsonPropertyName("no")]
        public string No { get; set; } = "";

        [JsonPropertyName("effectiveDate")]
        public string EffectiveDate { get; set; } = "";

        [JsonPropertyName("rates")]
        public List<NbpRate> Rates { get; set; } = new();
    }

    public class CurrencyService
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public CurrencyService(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri("https://api.nbp.pl/");
            _http.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<NbpTable?> GetTableAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/exchangerates/tables/A/?format=json");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var tables = JsonSerializer.Deserialize<List<NbpTable>>(json, _jsonOpts);
                return tables?.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<decimal?> GetRateAsync(string code)
        {
            var table = await GetTableAsync();
            return table?.Rates.FirstOrDefault(r => r.Code.Equals(code, StringComparison.OrdinalIgnoreCase))?.Mid;
        }

        public async Task<(decimal? Eur, decimal? Usd, decimal? Cny, decimal? Ils, string? Date)> ConvertPlnAsync(decimal amountPln)
        {
            var table = await GetTableAsync();
            if (table == null) return (null, null, null, null, null);

            var eurRate = table.Rates.FirstOrDefault(r => r.Code == "EUR")?.Mid;
            var usdRate = table.Rates.FirstOrDefault(r => r.Code == "USD")?.Mid;
            var cnyRate = table.Rates.FirstOrDefault(r => r.Code == "CNY")?.Mid;
            var ilsRate = table.Rates.FirstOrDefault(r => r.Code == "ILS")?.Mid;

            decimal? eur = eurRate.HasValue ? Math.Round(amountPln / eurRate.Value, 2) : null;
            decimal? usd = usdRate.HasValue ? Math.Round(amountPln / usdRate.Value, 2) : null;
            decimal? cny = cnyRate.HasValue ? Math.Round(amountPln / cnyRate.Value, 2) : null;
            decimal? ils = ilsRate.HasValue ? Math.Round(amountPln / ilsRate.Value, 2) : null;

            return (eur, usd, cny, ils, table.EffectiveDate);
        }
    }
}

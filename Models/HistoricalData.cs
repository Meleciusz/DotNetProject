using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class HistoricalData
    {
        public int Id { get; set; }

        [Required]
        public string CurrencyCode { get; set; }  // Kod waluty, np. USD, EUR

        [Required]
        public decimal Rate { get; set; }  // Kurs waluty

        [Required]
        public DateTime Timestamp { get; set; }  // Data i czas zapisu kursu
    }
}

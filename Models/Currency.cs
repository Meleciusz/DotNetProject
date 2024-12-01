using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Currency
    {
        public int Id { get; set; }

        [Required]
        [StringLength(3, ErrorMessage = "Currency code must be 3 characters.")]
        public string Code { get; set; }  // Kod waluty, np. USD, EUR

        [Required]
        [DataType(DataType.Currency)]
        public decimal Rate { get; set; }  // Kurs waluty względem PLN lub innej bazowej
    }
}

using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Currency
    {
        public int Id { get; set; }

        [Required]
        [StringLength(3, ErrorMessage = "Currency code must be 3 characters.")]
        public string Code { get; set; }  // Kod waluty, np. USD

        [Required]
        [DataType(DataType.Currency)]
        public decimal Rate { get; set; }  // Kurs waluty względem PLN

        [Required]
        public string Name { get; set; }  // Name

        public bool isAddedByUser { get; set; } // Flaga

        public bool IsFavorite { get; set; } // Flaga
    }
}

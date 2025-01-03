using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Currency> Currencies { get; set; } //zbiór kursów
        public DbSet<HistoricalData> HistoricalDatas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seeding danych początkowych
            modelBuilder.Entity<Currency>().HasData(
                new Currency { Id = 1, Code = "USD", Rate = 4.00m },   // Dolar amerykański
                new Currency { Id = 2, Code = "EUR", Rate = 4.50m },   // Euro
                new Currency { Id = 3, Code = "GBP", Rate = 5.20m },   // Funt brytyjski
                new Currency { Id = 4, Code = "PLN", Rate = 1.00m }    // Złoty (bazowy)
            );

            modelBuilder.Entity<HistoricalData>().HasIndex(h => new { h.CurrencyCode, h.Timestamp });

        }
    }
}

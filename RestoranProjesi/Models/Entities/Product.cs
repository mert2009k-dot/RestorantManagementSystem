using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestoranProjesi.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public bool IsDailyMeal { get; set; } // Günlük değişen yemek mi?
        public bool IsActive { get; set; } = true; // Şu an menüde görünsün mü?
        public bool IsFeatured { get; set; } // Öne çıkan lezzet mi?
    }
}

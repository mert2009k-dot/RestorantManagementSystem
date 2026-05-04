using System.ComponentModel.DataAnnotations.Schema;

namespace RestoranProjesi.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        // Delivery Type: "Pickup" (Gel Al), "Delivery" (Paket Servis)
        public string DeliveryType { get; set; } = "Pickup"; 
        
        // Payment Type: "CreditCard", "Cash"
        public string PaymentMethod { get; set; } = "CreditCard";
        
        public string Status { get; set; } = "Pending"; // Pending, Preparing, Ready, Completed, Cancelled
        
        public bool IsRestored { get; set; } = false; // İptalden dönenleri takip etmek için
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        public string? DeliveryAddress { get; set; }
        
        public int? TableNumber { get; set; } // QR menüden gelen masa numarası
        
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
